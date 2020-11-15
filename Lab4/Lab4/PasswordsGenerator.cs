using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lab4
{
    public class PasswordsGenerator
    {
        private static readonly List<Func<string, string>> passwordGeneratingRules
            = new List<Func<string, string>>
            {
                AppendNumbers, PrependNumbers, Transliterate, ReplaceLetters, ReplaceNumbers, ChangeCase
            };

        private readonly List<string> _top100Passwords;
        private readonly List<string> _top100000Passwords;

        public int Top100Persentage { get; }
        public int Top100000Persentage { get; }
        public int RandomPasswordsPersentage { get; }
        public int OtherPersentage { get; }

        public PasswordsGenerator(List<string> top100Passwords, 
            List<string> top100000Passwords,
            int top100Persentage = 5,
            int top100000Persentage = 70,
            int randomPasswordsPersentage = 5)
        {
            _top100Passwords = top100Passwords ?? throw new ArgumentNullException(nameof(top100Passwords));
            _top100000Passwords = top100000Passwords ?? throw new ArgumentNullException(nameof(top100000Passwords));

            if (top100Persentage <= 0 || top100Persentage > 100)
                throw new ArgumentException($"{nameof(top100Persentage)} must be value from 0 to 100");
            Top100Persentage = top100Persentage;

            if (top100000Persentage <= 0 || top100000Persentage > 100)
                throw new ArgumentException($"{nameof(top100000Persentage)} must be value from 0 to 100");
            Top100000Persentage = top100000Persentage;

            if (randomPasswordsPersentage <= 0 || randomPasswordsPersentage > 100)
                throw new ArgumentException($"{nameof(randomPasswordsPersentage)} must be value from 0 to 100");
            RandomPasswordsPersentage = randomPasswordsPersentage;

            int persentagesSum = Top100Persentage + Top100000Persentage + RandomPasswordsPersentage;
            if (persentagesSum > 100)
                throw new ArgumentException($"Sum of {nameof(top100Persentage)} {nameof(top100000Persentage)} and {nameof(randomPasswordsPersentage)} must be value from 0 to 100");

            OtherPersentage = 100 - persentagesSum;
        }

        public List<string> GeneratePasswords(int passwordsCount)
        {
            List<string> result = new List<string>(passwordsCount);

            var random = new Random();
            int top100Count = passwordsCount * Top100Persentage / 100;
            int top100000Count = passwordsCount * Top100000Persentage / 100;
            int randomPasswordsCount = passwordsCount * RandomPasswordsPersentage / 100;
            while (result.Count < top100Count)
            {
                result.Add(_top100Passwords.ElementAt(random.Next(0, _top100Passwords.Count)));
            }
            while(result.Count < top100Count + top100000Count)
            {
                result.Add(_top100000Passwords.ElementAt(random.Next(0, _top100000Passwords.Count)));
            }
            while (result.Count < top100Count + top100000Count + randomPasswordsCount)
            {
                result.Add(CreateRandomPassword(random.Next(8, 18)));
            }
            while (result.Count < passwordsCount)
            {
                result.Add(passwordGeneratingRules.ElementAt(random.Next(0, passwordGeneratingRules.Count))
                    .Invoke(_top100000Passwords.ElementAt(random.Next(0, _top100000Passwords.Count))));
            }

            return result.Shuffle();
        }

        private static string CreateRandomPassword(int length)
        {
            string source = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!:$_";
            StringBuilder generatedPassword = new StringBuilder();
            var random = new Random();
            for (int i = 0; i < length; i++)
            {
                generatedPassword.Append(source[random.Next(source.Length)]);
            }
            return generatedPassword.ToString();
        }


        #region Rules

        private static string AppendNumbers(string str)
        {
            var random = new Random();
            return $"{str}{string.Concat(Enumerable.Range(1, random.Next(2, 6)).Select(e => random.Next(0, 10)))}";
        }

        private static string PrependNumbers(string str)
        {
            var random = new Random();
            return $"{string.Concat(Enumerable.Range(1, random.Next(2, 6)).Select(e => random.Next(0, 10)))}{str}";
        }

        private static string Transliterate(string str)
        {
            string[] lat_up = { "A", "B", "V", "G", "D", "E", "Zh", "Z", "I", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "Kh", "Ts", "Ch", "Sh", "Shch", "Y", "Yu", "Ya" };
            string[] lat_low = { "a", "b", "v", "g", "d", "e", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch", "y", "yu", "ya" };
            string[] rus_up = { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ы", "Ю", "Я" };
            string[] rus_low = { "а", "б", "в", "г", "д", "е", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ы", "ю", "я" };
            for (int i = 0; i < lat_up.Length; i++)
            {
                str = str.Replace(lat_up[i], rus_up[i]);
                str = str.Replace(lat_low[i], rus_low[i]);
            }
            return str;
        }

        private static string ReplaceLetters(string str)
        {
            Dictionary<char, char> replacements = new Dictionary<char, char>
            {
                ['O'] = '0', ['o'] = '0',
                ['L'] = '1', ['l'] = '1', ['I'] = '1', ['i'] = '1',
                ['Z'] = '2', ['z'] = '2', ['R'] = '2',
                ['E'] = '3',
                ['A'] = '4', ['a'] = '@',
                ['S'] = '5', ['s'] = '5',
                ['G'] = '6', ['b'] = '6',
                ['T'] = '7',
                ['B'] = '8',
            };
            foreach (var pair in replacements)
                str = str.Replace(pair.Key, pair.Value);
            return str;
        }

        private static string ReplaceNumbers(string str)
        {
            Dictionary<char, int> replacements = new Dictionary<char, int>
            {
                ['0'] = 'o',
                ['1'] = 'l',
                ['2'] = 'z',
                ['3'] = 'E',
                ['4'] = 'A',
                ['5'] = 's',
                ['6'] = 'G',
                ['7'] = 'T',
                ['8'] = 'B',
            };
            foreach (var pair in replacements)
                str = str.Replace(pair.Key, (char)pair.Value);
            return str;
        }

        private static string ChangeCase(string str)
        {
            Func<char, string> ifCase;
            Func<char, string> elseCase;
            if (new Random().Next(1, 3) % 2 == 0)
            {
                ifCase = l => l.ToString().ToUpper();
                elseCase = l => l.ToString().ToLower();
            }
            else
            {
                ifCase = l => l.ToString().ToLower();
                elseCase = l => l.ToString().ToUpper();
            }

            return string.Concat(str.Select((l, i) => i % 2 == 0 ? ifCase(l) : elseCase(l)));
        }

        #endregion
    }
}
