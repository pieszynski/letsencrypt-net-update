using Oocx.Acme;
using Oocx.Acme.Console;
using Oocx.Acme.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Pieszynski.LenuManager.Core.Helpers;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace Pieszynski.LenuManager.Core
{
    public static class LenuHelpers
    {
        public static Options GetAppSettingsOptions()
        {
            string appSettingsPath = Path.Combine(
                Directory.GetCurrentDirectory(), 
                "appsettings.json"
                );
            if (!File.Exists(appSettingsPath))
                throw new FileNotFoundException("No appsettings.json", appSettingsPath);

            Options jopts = JsonConvert.DeserializeObject<Options>(
                File.ReadAllText(appSettingsPath)
                );

            string acmeServer = jopts.AcmeServer;
            if (string.IsNullOrEmpty(jopts.AcmeServer)) 
                acmeServer = jopts.IsTest
                    ? "https://acme-staging.api.letsencrypt.org/directory" // TEST
                    : "https://acme-v01.api.letsencrypt.org/directory";    // PROD

            Options response = GetDefaultOptions(
                isTest: jopts.IsTest,
                acmeServer: acmeServer,
                storeDirectory: jopts.StoreDirectory,
                email: jopts.Contact,
                acmeContainerName: jopts.ACME_CONTAINER,
                azureStorageConnectionString: jopts.ACME_CONNECTION_STRING,
                pfxPassword: jopts.PfxPassword,
                tosUri: jopts.TermsOfServiceUri,
                domains: jopts.Domains.ToArray()
                );
            return response;
        }

        public static Options GetDefaultOptions(
            bool isTest,
            string acmeServer,
            string storeDirectory,
            string email,
            string acmeContainerName,
            string azureStorageConnectionString,
            string pfxPassword,
            string tosUri,
            params string[] domains
            )
        {

            Options options = new Options
            {
                AcmeServer = acmeServer,
                AccountKeyName = "acme-key",
                ChallengeProvider = "manual-http-01",
                
                Domains = domains,
                ServerConfigurationProvider = "manual",

                PfxPassword = pfxPassword,
                Contact = $"mailto:{email}",

                IgnoreSSLCertificateErrors = false,
                AcceptInstructions = true, //false, // powinno być - AutomaticAcceptInstructions...
                AcceptTermsOfService = true,
                //TermsOfServiceUri = "https://letsencrypt.org/documents/LE-SA-v1.1.1-August-1-2016.pdf"
                TermsOfServiceUri = tosUri,

                IsTest = isTest,
                ACME_CONNECTION_STRING = azureStorageConnectionString,
                ACME_CONTAINER = acmeContainerName,
                StoreDirectory = storeDirectory
            };
            return options;
        }

        public static async Task RequestSslCertificate(Options options)
        {
            Log.Level = Oocx.Acme.Services.LogLevel.Info;

            if (!Directory.Exists(options.StoreDirectory))
                Directory.CreateDirectory(options.StoreDirectory);

            IKeyStore keyStore = new FileKeyStore(options.StoreDirectory);

            IAcmeClient client = new AcmeClient(
                options.AcmeServer,
                options.AccountKeyName,
                keyStore
                );

            var challenge = new AzureStorageChallengeProvider(
                options,
                client
                );

            IServerConfigurationProvider server = new ConfigureToFiles(options);

            var acmep = new AcmeProcess(
                options,
                challenge,
                server,
                client
                );

            await acmep.StartAsync();

            await challenge.CleanupUsedChallengeFilesAsync();
        }
    }
}
