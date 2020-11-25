using Logos.Utility.Security.Cryptography;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Lab2
{
    static class Program
    {
        static void Main(string[] args)
        {
            
        }

        public static void Demonstrate()
        {
            byte[] message1 = "Reusing the same key in streaming chiphers is a big mistake!".ToByteArray();
            byte[] message2 = "Really? Well, I won't do that again".ToByteArray();
            byte[] key = GenerateKey();

            using var salsa20 = new Salsa20();

            ICryptoTransform encryptor1 = salsa20.CreateEncryptor(key, new byte[8]);
            ICryptoTransform encryptor2 = salsa20.CreateEncryptor(key, new byte[8]);

            byte[] message1Enc = encryptor1.TransformFinalBlock(message1, 0, message1.Length);
            byte[] message2Enc = encryptor2.TransformFinalBlock(message2, 0, message2.Length);

            byte[] mes1mes2 = Xor(message1Enc, message2Enc);

            string cribWord = "Well";
            for (int i = 0; i < mes1mes2.Length - cribWord.Length; i++)
            {
                Console.WriteLine(
                    $"[{i}]: {Encoding.UTF8.GetString(Xor(cribWord.ToByteArray(), mes1mes2.Skip(i).Take(mes1mes2.Length - i).ToArray()))}");
            }
        }

        public static byte[] GenerateKey()
        {
            var buffer = new byte[16];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            return buffer;
        }

        public static byte[] Xor(byte[] first, byte[] second)
        {
            int length = first.Length < second.Length ? first.Length : second.Length;

            var result = new byte[length];
            for(int i = 0; i < length; i++)
            {
                result[i] = (byte)(first[i] ^ second[i]);
            }

            return result;
        }

        public static byte[] ToByteArray(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string ToHexString(this string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.UTF8.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        public static byte[] HexStringToByteArray(this string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }
    }
}
