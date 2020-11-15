using System.Security.Cryptography;

namespace Lab4.Hashers
{
    public abstract class BaseHasher
    {
        protected readonly int _saltLength;

        public BaseHasher(int saltLength)
        {
            _saltLength = saltLength;
        }

        protected byte[] CreateSalt()
        {
            var buffer = new byte[_saltLength];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            return buffer;
        }

        public abstract PasswordRecord GetHashedRecord(string input);
    }
}
