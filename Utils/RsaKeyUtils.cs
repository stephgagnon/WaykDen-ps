using System.Linq;
using DerConverter.Asn;
using DerConverter.Asn.KnownTypes;

namespace WaykDen.Utils
{
    public static class RsaKeyutils
    {
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
    }
}