using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1.Task1
{
    public class SingleByteXorAttacker
    {
        private readonly Dictionary<char, float> _englishLettersFrequency =
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

        private readonly int _englishLettersCount;
        private readonly SingleByteXor _singleByteXor;

        public SingleByteXorAttacker()
        {
            _englishLettersCount = _englishLettersFrequency.Keys.Count();
            _singleByteXor = new SingleByteXor();
        }

        public byte GetSingleByteXorKey(string encryptedMessage)
        {
            string decryptedMessage;
            float tempFittingQuotient;
            float minFittingQuotient = float.MaxValue;            
            byte resKey = 0;
            for (byte key = 1; key <= byte.MaxValue; key++)
            {
                decryptedMessage = _singleByteXor.Decrypt(encryptedMessage, key).ToLower();

                tempFittingQuotient = CalcFittingQuotient(decryptedMessage);

                if (tempFittingQuotient < minFittingQuotient)
                {
                    minFittingQuotient = tempFittingQuotient;
                    resKey = key;
                }

                if (key == byte.MaxValue)
                    break;
            }

            return resKey;
        }

        private float CalcFittingQuotient(string decryptedMessage)
        {
            float currentLetterFrequency;
            float tempDeviationSum = 0;
            foreach (var letterFrequency in _englishLettersFrequency)
            {
                currentLetterFrequency =
                    decryptedMessage.Count(l => l.Equals(letterFrequency.Key)) * 100 /
                    (float)decryptedMessage.Length;

                tempDeviationSum += Math.Abs(letterFrequency.Value - currentLetterFrequency);
            }

            return tempDeviationSum / _englishLettersCount;
        }
    }
}
