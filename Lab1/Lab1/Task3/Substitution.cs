using System;
using System.Linq;

namespace Lab1.Task3
{
    public class Substitution
    {
        public string Alphabet { get; }
        public string Key { get; }

        public Substitution(string alphabet, string key)
        {
            Alphabet = alphabet ?? throw new ArgumentNullException(nameof(alphabet));
            Key = key ?? throw new ArgumentNullException(nameof(key));

            if (Alphabet.Length != key.Length)
                throw new ArgumentException($"Alphabet length ({Alphabet.Length}) must be equal to the key length ({Key.Length})");
        }

        public string Encrypt(string message) =>
            new string(message.Select(l => {
                int index = Alphabet.IndexOf(l);
                return index == -1 ? l : Key.ElementAt(index);
            }).ToArray());

        public string Decrypt(string message) =>
            new string(message.Select(l => {
                int index = Key.IndexOf(l);
                return index == -1 ? l : Alphabet.ElementAt(index);
            }).ToArray());
    }
}
