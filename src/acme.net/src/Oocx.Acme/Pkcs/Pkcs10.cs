using System;
using System.Security.Cryptography;

namespace Oocx.Pkcs
{
    public static class Pkcs10
    {
        public static CertificationRequest Encode(CertificateRequestData requestData)
        {
            var publicKeyBytes = Asn1.Encode(new Sequence(
                new DerInteger(requestData.Key.Modulus),
                new DerInteger(requestData.Key.Exponent)
            ));

            var certificationRequestInfo = new CertificationRequestInfo(
                new DerInteger(0),
                new Name(
                    /*new RelativeDistinguishedName(
                        new AttributeTypeAndValue(new ObjectIdentifier(Oid.Attribute.C),
                            new PrintableString(requestData.C))),
                    new RelativeDistinguishedName(
                        new AttributeTypeAndValue(new ObjectIdentifier(Oid.Attribute.S), new UTF8String(requestData.S))),
                    new RelativeDistinguishedName(
                        new AttributeTypeAndValue(new ObjectIdentifier(Oid.Attribute.L), new UTF8String(requestData.L))),
                    new RelativeDistinguishedName(
                        new AttributeTypeAndValue(new ObjectIdentifier(Oid.Attribute.O), new UTF8String(requestData.O))),
                    new RelativeDistinguishedName(
                        new AttributeTypeAndValue(new ObjectIdentifier(Oid.Attribute.OU), new UTF8String(requestData.OU))),*/
                    new RelativeDistinguishedName(
                        new AttributeTypeAndValue(new ObjectIdentifier(Oids.Attribute.CN), new UTF8String(requestData.CN)))),
                new SubjectPublicKeyInfo(
                    new AlgorithmIdentifier(Oids.Algorithm.RSA),
                    new BitString(publicKeyBytes)),
                new ContextSpecific());

            var certificationRequestInfoBytes = Asn1.Encode(certificationRequestInfo);

            var rsa = new RSACryptoServiceProvider();

            rsa.ImportParameters(requestData.Key);

            var signatureBytes = rsa.SignData(certificationRequestInfoBytes, SHA256.Create());

            return new CertificationRequest(
                certificationRequestInfo : certificationRequestInfo,
                signatureAlgorithm       : new AlgorithmIdentifier(Oids.Algorithm.Sha256RSA),
                signature                : new BitString(signatureBytes)
            );
        }

        public static byte[] EncodeAsDer(CertificateRequestData requestData)
        {
            var asn1 = Encode(requestData);

            return Asn1.Encode(asn1);
        }

        public static string EncodeAsBase64(CertificateRequestData requestData)
        {
            var bytes = EncodeAsDer(requestData);

            return Pem.Encode(bytes, "NEW CERTIFICATE REQUEST");
        }

        public static string EncodeAsBase64Url(CertificateRequestData requestData)
        {
            var bytes = EncodeAsDer(requestData);
            var base64 = bytes.Base64UrlEncoded();
            string base64lines = "";

            for (int i = 0; i < base64.Length; i += 64)
            {
                base64lines += base64.Substring(i, Math.Min(64, base64.Length - i)) + "\n";
            }

            return $"-----BEGIN NEW CERTIFICATE REQUEST-----\n{base64lines}-----END NEW CERTIFICATE REQUEST-----";
        }
    }
}

// https://en.wikipedia.org/wiki/Certificate_signing_request