using System.Security.Cryptography;

namespace Core_API.Web.Utilities
{
    public static class SecretKeyGenerator
    {
        public static string GenerateSecretKey(int length = 32)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var byteArray = new byte[length];
                rng.GetBytes(byteArray);
                return Convert.ToBase64String(byteArray);
            }
        }
    }
}
