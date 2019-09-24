using System;
using System.Runtime.InteropServices;
using System.Security;

namespace WaykDen.Utils
{
    public static class SecureStringUtils
    {
        public static SecureString FromString(string str)
        {
            if (str == null)
            {
                return null;
            }

            SecureString result = new SecureString();

            foreach (var chr in str)
            {
                result.AppendChar(chr);
            }

            return result;
        }

        public static string ToString(SecureString secureString)
        {
            if (secureString == null)
            {
                return null;
            }

            using (var handle = new SecureStringHandle(secureString))
            {
                return Marshal.PtrToStringUni(handle.Value);
            }
        }

        private sealed class SecureStringHandle : IDisposable
        {
            private IntPtr? value;

            public SecureStringHandle(SecureString secureString)
            {
                this.SecureString = secureString;
            }

            public IntPtr Value => this.value ?? this.InitializeHandle();

            public SecureString SecureString { get; private set; }

            public IntPtr CreateCopy()
            {
                return Marshal.SecureStringToGlobalAllocUnicode(this.SecureString.Copy());
            }

            public void Dispose()
            {
                if (this.value.HasValue && this.value.Value != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(this.value.Value);

                    this.value = IntPtr.Zero;
                }

                this.SecureString = null;
            }

            private IntPtr InitializeHandle()
            {
                this.value = this.SecureString == null ? IntPtr.Zero : Marshal.SecureStringToGlobalAllocUnicode(this.SecureString);

                return this.value.Value;
            }
        }
    }
}
