using System.IO;
using System.Security.Cryptography;
using PemUtils;

namespace WaykDen.Utils
{
    public class RsaKeyGenerator
    {
        public string PrivateKey {get; set;} = string.Empty;
        public string PublicKey {get; set;} = string.Empty;
        public RsaKeyGenerator()
        {
            RSA rsa = RSA.Create();
            rsa.KeySize = 2048;
            MemoryStream stream = new MemoryStream();
            var writer = new PemWriter(stream);
            writer.WritePrivateKey(rsa);
            stream.Seek(0, SeekOrigin.Begin);
            this.PrivateKey = new StreamReader(stream).ReadToEnd();
            stream.SetLength(0);
            writer.WritePublicKey(rsa);
            stream.Seek(0, SeekOrigin.Begin);
            this.PublicKey = new StreamReader(stream).ReadToEnd();
        }
    }
}