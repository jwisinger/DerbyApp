using System;

namespace DerbyApp.Helpers
{
    public static class ShortGuid
    {
        public static string GenerateShortGuid()
        {
            Guid guid = Guid.NewGuid();
            // Convert the Guid to a byte array
            byte[] bytes = guid.ToByteArray();

            // Convert the byte array to a Base64 string
            string base64 = Convert.ToBase64String(bytes);

            // Make it URL safe by replacing non-URL-friendly characters
            // and remove padding characters (=)
            string shortGuid = base64.Replace("/", "_").Replace("+", "-").TrimEnd('=');

            return shortGuid; // Example result: FEx1sZbSD0ugmgMAF_RGHw (22 chars)
        }

        public static Guid DecodeShortGuid(string shortGuid)
        {
            // Restore URL-friendly characters
            string base64 = shortGuid.Replace("_", "/").Replace("-", "+");

            // Add padding back (Base64 string length must be a multiple of 4)
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            // Convert the Base64 string back to a byte array and then a Guid
            byte[] bytes = Convert.FromBase64String(base64);
            return new Guid(bytes);
        }
    }
}
