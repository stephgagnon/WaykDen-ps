using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Management.Automation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PemUtils;
using DerConverter;
using DerConverter.Asn;
using DerConverter.Asn.KnownTypes;

namespace DenRsa
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