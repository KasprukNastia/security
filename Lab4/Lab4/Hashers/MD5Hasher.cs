using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Lab4.Hashers
{
    public class MD5Hasher : BaseHasher
    {
        public MD5Hasher(int saltLength = 16) 
            : base(saltLength)
        {
        }

        public override PasswordRecord GetHashedRecord(string input)
        {
            byte[] salt = CreateSalt();

            var md5CryptoProvider = new MD5CryptoServiceProvider();
            byte[] hashBytes = md5CryptoProvider.ComputeHash(salt.ToList().Concat(Encoding.UTF8.GetBytes(input)).ToArray());

            return new PasswordRecord
            {
                PasswordHash = Convert.ToBase64String(hashBytes),
                Salt = Convert.ToBase64String(salt)
            };
        }
    }
}
