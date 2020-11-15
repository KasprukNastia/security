using Lab4.Hashers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lab4
{
    class Program
    {
        static void Main(string[] args)
        {
            string basePath = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName}\\Data";

            List<string> top100Passwords = File.ReadAllLines($"{basePath}\\top_100.txt")
                .SelectMany(line => line.Split(" ", StringSplitOptions.RemoveEmptyEntries).TakeLast(1).Select(s => s.Trim()))
                .ToList();
            List<string> top100000Passwords = File.ReadAllLines($"{basePath}\\top_100000.txt")
                .Select(line => line.Trim())
                .ToList();
            var passwordGenerator = new PasswordsGenerator(top100Passwords, top100000Passwords);

            int passwordsPerFile = 100_000;

            BaseHasher hasher = new MD5Hasher();
            //passwordGenerator.GeneratePasswords(passwordsCount: passwordsPerFile)
            //    .Select(p => hasher.GetHashedRecord(p))
            //    .WriteListToCsvFile($"{basePath}\\md5_hashed.csv");

            //hasher = new SHA1Hasher();
            //passwordGenerator.GeneratePasswords(passwordsCount: passwordsPerFile)
            //    .Select(p => hasher.GetHashedRecord(p))
            //    .WriteListToCsvFile($"{basePath}\\sha1_hashed.csv");

            hasher = new Argon2idHasher();
            passwordGenerator.GeneratePasswords(passwordsCount: passwordsPerFile)
                .Select(p => hasher.GetHashedRecord(p))
                .WriteListToCsvFile($"{basePath}\\argon2id_hashed.csv");
        }
    }
}
