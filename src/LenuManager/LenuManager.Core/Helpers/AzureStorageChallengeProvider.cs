using Oocx.Acme.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Oocx.Acme.Protocol;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Oocx.Acme.Console;

namespace Pieszynski.LenuManager.Core.Helpers
{
    public class AzureStorageChallengeProvider : IChallengeProvider
    {
        public const string AcmeChallengeDir = "acme-challenge";

        private readonly Options _options;
        private readonly IAcmeClient _client;
        private readonly Stack<string> _usedKeys;

        public AzureStorageChallengeProvider(Options options, IAcmeClient client)
        {
            this._options = options;
            this._client = client;
            this._usedKeys = new Stack<string>();
        }

        public async Task<PendingChallenge> AcceptChallengeAsync(
            string domain, 
            string siteName, 
            AuthorizationResponse authorization
            )
        {
            var challenge = authorization?.Challenges
                .FirstOrDefault(c => c.Type == "http-01");

            if (challenge == null)
            {
                Log.Error("the server does not accept challenge type http-01");
                return null;
            }

            Log.Info($"accepting challenge {challenge.Type}");

            var keyAuthorization = this._client.GetKeyAuthorization(challenge.Token);
            
            await this.PutChallengeFileAsync(challenge.Token, keyAuthorization);

            return new PendingChallenge
            {
                Instructions = $"Putting challange to Azure Storage. Please wait.",
                Complete = () =>
                {
                    this.RemoveChallengeFileAsync(challenge.Token)
                        .GetAwaiter().GetResult();
                    return this._client.CompleteChallengeAsync(challenge);
                }
            };
        }

        protected async Task PutChallengeFileAsync(string key, string content)
        {
            await CloudStorageAccount.Parse(this._options.ACME_CONNECTION_STRING)
                .CreateCloudBlobClient()
                .GetContainerReference(this._options.ACME_CONTAINER)
                .GetBlockBlobReference($"{AcmeChallengeDir}/{key}")
                .UploadTextAsync(content);

            Log.Verbose($"Key {key} has been put on server.");
            this._usedKeys.Push(key);
        }

        protected async Task RemoveChallengeFileAsync(string key)
        {
            // niestety, w tym miejscu za szybko usuwa klucz i Lets Encrypt dostaje błąd
        }

        public async Task CleanupUsedChallengeFilesAsync()
        {
            var azureRef = CloudStorageAccount.Parse(this._options.ACME_CONNECTION_STRING)
                       .CreateCloudBlobClient()
                       .GetContainerReference(this._options.ACME_CONTAINER);
            while (0 < this._usedKeys.Count)
            {
                string key = this._usedKeys.Pop();

                await azureRef
                    .GetBlockBlobReference($"{AcmeChallengeDir}/{key}")
                    .DeleteIfExistsAsync();
            }
        }
    }
}
