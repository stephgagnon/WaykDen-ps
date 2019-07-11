using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace WaykPS
{
    public static class DenServiceUtils
    {
        public static string Generate(int length)
        {
            string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890--";
            StringBuilder builder = new StringBuilder();

            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

            byte[] random = new byte[sizeof(uint)];

            while(length-- > 0)
            {
                rngCsp.GetBytes(random);
                uint num = BitConverter.ToUInt32(random, 0);
                builder.Append(valid[(int)(num % (uint)valid.Length)]);
            }

            return builder.ToString();
        }
    }
}