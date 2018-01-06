using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pieszynski.LenuAzureAspMiddleware
{
    /// <summary>
    /// Opcje konfiguracji: Connection string do kontenera danych Azure
    /// </summary>
    public class LenuOptions
    {
        public string ACME_CONNECTION_STRING { get; set; }
        public string ACME_CONTAINER { get; set; }
    }

    /// <summary>
    /// Funkcje pomocnicze do rejestracji w Startup.cs
    /// </summary>
    public static class LenuRegisterAzureMiddlewareExtensions
    {
        public static void AddLetsEncryptAzureUpdate(this IServiceCollection services,
            IConfiguration config
            )
        {
            services.AddRouting();
            services.AddOptions();
            services.Configure<LenuOptions>(config);
        }

        public static void UseLetsEncryptAzureUpdate(this IApplicationBuilder app)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
        }
    }

    /// <summary>
    /// Kontroler serwujący dane z kontenera Azure
    /// </summary>
    public class LenuAzureAcmeController : ControllerBase
    {
        protected const string AcmeChallengeDir = "acme-challenge";

        protected readonly LenuOptions options;
        protected readonly ILogger<LenuAzureAcmeController> logger;
        protected readonly CloudBlobContainer container;

        public LenuAzureAcmeController(IOptionsSnapshot<LenuOptions> optionsAccessor, 
            ILogger<LenuAzureAcmeController> logger
            )
        {
            this.options = optionsAccessor.Value;
            this.logger = logger;

            try
            {
                string connectionString = this.options.ACME_CONNECTION_STRING;
                string acmeContainerName = this.options.ACME_CONTAINER;
                this.container = CloudStorageAccount.Parse(connectionString)
                    .CreateCloudBlobClient()
                    .GetContainerReference(acmeContainerName);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex, 
                    "Missing 'ACME_CONNECTION_STRING' parameter/app setting"
                    );
                throw;
            }
        }

        [Route(".well-known/acme-challenge/{*fileName}")]
        public virtual async Task<IActionResult> GetFilesAsync(string fileName)
        {
            var data = await this.GetChallengeFileContentAsync(fileName);
            if (string.IsNullOrEmpty(data))
                return this.NotFound("Unable to serve file: " + fileName);

            return Content(data, "text/plain");
        }

        protected async Task<string> GetChallengeFileContentAsync(string fileName)
        {
            string blobName = fileName
                ?.Replace(".", "")
                .Replace("/", "")
                .Replace("\\", "");

            if (string.IsNullOrEmpty(blobName))
                return "";

            try
            {
                string response = await this.container
                    .GetBlockBlobReference($"{AcmeChallengeDir}/{blobName}")
                    .DownloadTextAsync();

                return response;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Requested file: {fileName}. " +
                    $"Requested blob: {AcmeChallengeDir}/{blobName}");
                return "";
            }
        }
    }
}
