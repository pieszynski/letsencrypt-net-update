using Oocx.Acme.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Oocx.Acme.Console;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Oocx.Pkcs;

namespace Pieszynski.LenuManager.Core.Helpers
{
    public class ConfigureToFiles : IServerConfigurationProvider
    {
        class Container
        {
            public string Cert;
            public string Key;
        }

        protected readonly Options _options;
        private Container midMethods = null;

        public ConfigureToFiles(Options options)
        {
            this._options = options;
        }

        public byte[] InstallCertificateWithPrivateKey(
            string certificatePath,
            string certificateStoreName,
            RSAParameters privateKey)
        {
            var xCert = new X509Certificate2(certificatePath);
            var bCertBytes = xCert.Export(X509ContentType.Cert);

            var privk = new RSAPrivateKey(privateKey);
            var pemEncodedPrivateKey = privk.ToPemString();

            midMethods = new Container
            {
                Cert = Oocx.Pkcs.Pem.Encode(bCertBytes, "CERTIFICATE"),
                Key = pemEncodedPrivateKey
            };

            return null;
        }

        public void ConfigureServer(
            string domain,
            byte[] certificateHash,
            string certificateStoreName,
            string siteName,
            string binding)
        {
            if (null != this.midMethods)
            {
                var mid = this.midMethods;
                this.midMethods = null;

                string sDerCrtPath = Path.Combine(
                    this._options.StoreDirectory,
                    $"{domain}.der.crt"
                    );
                string sKeyPath = Path.Combine(
                    this._options.StoreDirectory,
                    $"{domain}.key"
                    );

                File.WriteAllText(sDerCrtPath, mid.Cert);
                File.WriteAllText(sKeyPath, mid.Key);
            }
        }
    }
}
