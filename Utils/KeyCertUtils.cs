using System;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DerConverter.Asn;
using DerConverter.Asn.KnownTypes;
using PemUtils;

namespace WaykDen.Utils
{
    public static class KeyCertUtils
    {
        public static string CERTIFICATE_HEADER = "-----BEGIN CERTIFICATE-----";
        public static string CERTIFICATE_FOOTER = "-----END CERTIFICATE-----";
        public static byte[] PemToDer(string pem)
        {
            var encoder = new DefaultDerAsnEncoder();
            return encoder.Encode(new DerAsnUtf8String(pem)).ToArray();
        }

        public static string DerToPem(byte[] der)
        {
            var decoder = new DefaultDerAsnDecoder();
            return decoder.Decode(der).Value as string;
        }

        public static CertificateObject GetPkcs12CertificateInfo(string path, string importPasswd)
        {
            X509Certificate2Collection coll = new X509Certificate2Collection();
            coll.Import(path, importPasswd, X509KeyStorageFlags.PersistKeySet);
            CertificateObject cert = new CertificateObject();

            foreach(X509Certificate2 c in coll)
            {
                string key = GetPrivateKeyFromPkcs12(c);
                cert.Privatekey += !string.IsNullOrEmpty(key) ? key : string.Empty;
                cert.Certificate =  $"{GetCertificateFromPkcs12(c)}{cert.Certificate}";
            }

            return cert;
        }

        public static string GetPrivateKeyFromPkcs12(X509Certificate2 cert)
        {
            RSA rsa = cert.GetRSAPrivateKey();
            if(rsa != null)
            {
                MemoryStream stream = new MemoryStream();
                var writer = new PemWriter(stream);
                writer.WritePrivateKey(rsa);
                stream.Seek(0, SeekOrigin.Begin);
                string privateKey = new StreamReader(stream).ReadToEnd();
                return privateKey;
            }

            return null;
        }

        public static string GetCertificateFromPkcs12(X509Certificate2 cert)
        {
            return string.Format("{0}{1}{2}", $"{CERTIFICATE_HEADER}\n", AdjustCertificateString(Convert.ToBase64String(cert.RawData)), $"{CERTIFICATE_FOOTER}\n");
        }

        private static string AdjustCertificateString(string cert)
        {
            StringBuilder sb = new StringBuilder();
            while(cert.Length/64 > 0)
            {
                string s = cert.Substring(0,64);
                sb.Append($"{s}\n");
                cert = cert.Remove(0, 64);
            }
            sb.Append($"{cert}\n");
            return sb.ToString();
        }
    }

    public class CertificateObject
    {
        public string Certificate {get; set;} = string.Empty;
        public string Privatekey {get; set;} = string.Empty;
    }
}