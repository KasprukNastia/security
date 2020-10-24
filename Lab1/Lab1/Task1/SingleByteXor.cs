using System.Linq;
using System.Text;

namespace Lab1.Task1
{
    public class SingleByteXor
    {
        public string Encrypt(string message, byte key) =>
            Encoding.UTF8.GetString(message.Select(e => (byte)(e ^ key)).ToArray());

        public string Decrypt(string message, byte key) => Encrypt(message, key);
    }
}
