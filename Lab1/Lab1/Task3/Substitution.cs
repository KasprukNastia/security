using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1.Task3
{
    public class Substitution
    {
        public string Alphabet { get; }
        public List<string> Keys { get; }

        public Substitution(string alphabet, List<string> keys)
        {
            Alphabet = alphabet ?? throw new ArgumentNullException(nameof(alphabet));
            Keys = keys ?? throw new ArgumentNullException(nameof(keys));

            if(keys.Count == 0)
                throw new ArgumentException("Keys collection must have at least one value");
            if (keys.Any(k => k.Length != Alphabet.Length))
                throw new ArgumentException($"Alphabet length ({Alphabet.Length}) must be equal to the key length");
        }

        public string Encrypt(string message) =>
            new string(message.Select((letter, letterIndex) => {               
                string key = Keys.Count == 1 ? Keys.First() : Keys.ElementAt(letterIndex % Keys.Count);
                int alphabetIndex = Alphabet.IndexOf(letter);
                return alphabetIndex == -1 ? letter : key.ElementAt(alphabetIndex);
            }).ToArray());

        public string Decrypt(string message) =>
            new string(message.Select((letter, letterIndex) => {
                string key = Keys.Count == 1 ? Keys.First() : Keys.ElementAt(letterIndex % Keys.Count);
                int index = key.IndexOf(letter);
                return index == -1 ? letter : Alphabet.ElementAt(index);
            }).ToArray());
    }
}
