using Lab1.Task1;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Lab1.Task2
{
    public class RepeatingKeyXorAttacker
    {
        private readonly SingleByteXorAttacker _singleByteXorAttacker = new SingleByteXorAttacker();

        public List<string> GetRepeatingKeyXorPossibleKeys(string encryptedMessage)
        {
            int keyLength = GetKeyLength(encryptedMessage);

            List<byte[]> resultKeys = new List<byte[]> { new byte[keyLength] };

            int remainder;
            List<byte> possibleKeysForMessagePart;
            for (int i = 0; i < keyLength; i++)
            {
                remainder = i % keyLength;
                possibleKeysForMessagePart = _singleByteXorAttacker.GetSingleByteXorPossibleKeys(
                    new string(encryptedMessage.Where((letter, index) => index % keyLength == remainder).ToArray()))
                    .Distinct(new RepeatingKeyXorAttackerComparer()).ToList();

                if (possibleKeysForMessagePart.Count == 1)
                {
                    resultKeys.ForEach(key => key[i] = possibleKeysForMessagePart.First());
                }
                else
                {
                    List<byte[]> newResultKeys =
                        new List<byte[]>(resultKeys.Count * possibleKeysForMessagePart.Count);
                    byte[] newKey;
                    foreach (byte possibleKey in possibleKeysForMessagePart)
                    {
                        foreach (byte[] resultKey in resultKeys)
                        {
                            newKey = resultKey.ToArray();
                            newKey[i] = possibleKey;
                            newResultKeys.Add(newKey);
                        }
                    }
                    resultKeys = newResultKeys;
                }
            }

            return resultKeys.Select(key => Encoding.UTF8.GetString(key)).ToList();
        }

        public int GetKeyLength(string encryptedMessage)
        {
            Dictionary<int, float> coincidenceIndices =
                new Dictionary<int, float>(encryptedMessage.Length - 1);

            IEnumerable<char> comparedMessage;
            int numberOfMatches = 0;
            for (int offset = 1; offset < encryptedMessage.Length; offset++)
            {
                comparedMessage = encryptedMessage.TakeLast(offset)
                    .Concat(encryptedMessage.Take(encryptedMessage.Length - offset));
                for (int i = 0; i < encryptedMessage.Length; i++)
                {
                    if (encryptedMessage.ElementAt(i).Equals(comparedMessage.ElementAt(i)))
                        numberOfMatches++;
                }
                coincidenceIndices[offset] = (float)numberOfMatches / encryptedMessage.Length;
                numberOfMatches = 0;
            }

            float maxCoincidenceIndex = coincidenceIndices.Max(ci => ci.Value);
            return coincidenceIndices.FirstOrDefault(ci => ci.Value == maxCoincidenceIndex).Key;
        }

        class RepeatingKeyXorAttackerComparer : IEqualityComparer<byte>
        {
            public bool Equals([AllowNull] byte x, [AllowNull] byte y)
            {
                return ((char)x).ToString().Equals(((char)y).ToString(), StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode([DisallowNull] byte obj)
            {
                return ((char)obj).ToString().ToLower().GetHashCode();
            }
        }
    }
}