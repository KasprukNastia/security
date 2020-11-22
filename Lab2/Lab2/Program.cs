using Logos.Utility.Security.Cryptography;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Lab2
{
    static class Program
    {
        static void Main(string[] args)
        {
            var message1Enc = "ad924af7a9cdaf3a1bb0c3fe1a20a3f367d82b0f05f8e75643ba688ea2ce8ec88f4762fbe93b50bf5138c7b699".ToByteArray();
            var message2Enc = "a59a0eaeb4d1fc325ab797b31425e6bc66d36e5b18efe8060cb32edeaad68180db4979ede43856a24c7d".ToByteArray();

            var mes1mes2 = Xor(message1Enc, message2Enc);

            Console.WriteLine(Encoding.UTF8.GetString(Xor("the".ToByteArray(), mes1mes2)));
        }

        public static void Demonstrate()
        {
            using var salsa20 = new Salsa20();
            var key = GenerateKey();

            string message1 = "Hi! How are you? What are you doing?";
            byte[] message1Bytes = Encoding.UTF8.GetBytes(message1);
            string message2 = "I am fine, super cool. Bla-bla-bla. And how are you?";
            byte[] message2Bytes = Encoding.UTF8.GetBytes(message2);

            var encryptor1 = salsa20.CreateEncryptor(key, new byte[8]);
            var encryptor2 = salsa20.CreateEncryptor(key, new byte[8]);

            var message1Enc = encryptor1.TransformFinalBlock(message1Bytes, 0, message1.Length);
            var message2Enc = encryptor2.TransformFinalBlock(message2Bytes, 0, message2.Length);

            var mes1mes2 = Xor(message1Enc, message2Enc);

            Console.WriteLine(Encoding.UTF8.GetString(Xor(message1Bytes, mes1mes2)));
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
            byte[] smaller, bigger;
            if(first.Length < second.Length)
            {
                smaller = first;
                bigger = second;
            }
            else
            {
                smaller = second;
                bigger = first;
            }

            byte[] result = new byte[bigger.Length];
            int smallerCounter = 0;
            for (int biggerCounter = 0; biggerCounter < bigger.Length; biggerCounter++)
            {
                result[biggerCounter] = (byte)(bigger[biggerCounter] ^ smaller[smallerCounter]);

                smallerCounter++;
                if (smallerCounter == smaller.Length)
                    smallerCounter = 0;
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
