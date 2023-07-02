using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace AccountingBot
{
    /// <summary>
    /// 密码Hash帮助类
    /// </summary>
    public class PasswordHasher
    {
        /// <summary>
        /// 获取经过Hash后的密码
        /// </summary>
        /// <param name="password">原密码</param>
        /// <param name="saltText">盐</param>
        public static string GetHashedPassword(string password, ref string saltText)
        {
            byte[] salt = null;
            if (saltText?.Length > 0)
            {
                salt = Convert.FromBase64String(saltText);
            }
            else
            {
                // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
                salt = RandomNumberGenerator.GetBytes(128 / 8);
                saltText = Convert.ToBase64String(salt);
            }

            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return hashed;
        }
    }
}
