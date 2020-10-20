using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1.Task1
{
    public class SingleByteXorAttacker
    {
        public static readonly Dictionary<char, float> OneLetterEnglishFrequency =
            new Dictionary<char, float>
            {
                ['a'] = 8.2389258F, ['b'] = 1.5051398F, ['c'] = 2.8065007F,
                ['d'] = 4.2904556F, ['e'] = 12.813865F, ['f'] = 2.2476217F,
                ['g'] = 2.0327458F, ['h'] = 6.1476691F, ['i'] = 6.1476691F,
                ['j'] = 0.1543474F, ['k'] = 0.7787989F, ['l'] = 4.0604477F,
                ['m'] = 2.4271893F, ['n'] = 6.8084376F, ['o'] = 7.5731132F,
                ['p'] = 1.9459884F, ['q'] = 0.0958366F, ['r'] = 6.0397268F,
                ['s'] = 6.3827211F, ['t'] = 9.1357551F, ['u'] = 2.7822893F,
                ['v'] = 0.9866131F, ['w'] = 2.3807842F, ['x'] = 0.1513210F,
                ['y'] = 1.9913847F, ['z'] = 0.0746517F
            };

        public static readonly int EnglishLettersCount = OneLetterEnglishFrequency.Keys.Count();
        
        private readonly SingleByteXor _singleByteXor;

        public SingleByteXorAttacker()
        {
            _singleByteXor = new SingleByteXor();
        }

        public List<byte> GetSingleByteXorPossibleKeys(string encryptedMessage)
        {
            string decryptedMessage;
            float tempFittingQuotient;
            float minFittingQuotient = float.MaxValue;
            List<byte> resKeys = new List<byte>();
            for (byte key = 1; key <= byte.MaxValue; key++)
            {
                decryptedMessage = _singleByteXor.Decrypt(encryptedMessage, key).ToLower();

                tempFittingQuotient = CalcOneLetterFittingQuotient(decryptedMessage);

                if (tempFittingQuotient < minFittingQuotient)
                {
                    minFittingQuotient = tempFittingQuotient;
                    resKeys.Clear();
                    resKeys.Add(key);
                }
                else if(tempFittingQuotient == minFittingQuotient)
                {
                    resKeys.Add(key);
                }

                if (key == byte.MaxValue)
                    break;
            }

            return resKeys;
        }

        public float CalcOneLetterFittingQuotient(string decryptedMessage)
        {
            float currentLetterFrequency;
            float tempDeviationSum = 0;
            foreach (var letterFrequency in OneLetterEnglishFrequency)
            {
                currentLetterFrequency =
                    decryptedMessage.Count(l => l.Equals(letterFrequency.Key)) * 100 /
                    (float)decryptedMessage.Length;

                tempDeviationSum += Math.Abs(letterFrequency.Value - currentLetterFrequency);
            }

            return tempDeviationSum / EnglishLettersCount;
        }
    }
}
