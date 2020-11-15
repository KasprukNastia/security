using Konscious.Security.Cryptography;
using System;
using System.Text;

namespace Lab4.Hashers
{
    public class Argon2idHasher : BaseHasher
    {
        public Argon2idHasher(int saltLength = 16) 
            : base(saltLength)
        {
        }

        public override PasswordRecord GetHashedRecord(string input)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(input));

            argon2.Salt = CreateSalt();
            argon2.DegreeOfParallelism = 8;
            argon2.Iterations = 4;
            argon2.MemorySize = 1024 * 1024;

            byte[] hashBytes = argon2.GetBytes(_saltLength);

            return new PasswordRecord
            {
                PasswordHash = Convert.ToBase64String(hashBytes),
                Salt = Convert.ToBase64String(argon2.Salt)
            };
        }
    }
}
