using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Lab4.Hashers
{
    public class SHA1Hasher : BaseHasher
    {
        public SHA1Hasher(int saltLength = 16) 
            : base(saltLength)
        {
        }

        public override PasswordRecord GetHashedRecord(string input)
        {
            byte[] salt = CreateSalt();

            using SHA1Managed sha1 = new SHA1Managed();
            byte[] hashBytes = sha1.ComputeHash(salt.ToList().Concat(Encoding.UTF8.GetBytes(input)).ToArray());

            return new PasswordRecord
            {
                PasswordHash = Convert.ToBase64String(hashBytes),
                Salt = Convert.ToBase64String(salt)
            };
        }
    }
}
