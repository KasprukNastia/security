using System.Linq;

namespace Lab1.Task2
{
    public class RepeatingKeyXor
    {
        public string Encrypt(string message, string key)
        {
            if (string.IsNullOrEmpty(key))
                return message;

            char[] keyChars = key.ToCharArray();

            return new string(
                message.Select((letter, index) => (char)(letter ^ keyChars[index % keyChars.Length])).ToArray());
        }

        public string Decrypt(string message, string key) => Encrypt(message, key);
    }
}
