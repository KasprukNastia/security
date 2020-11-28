using Konscious.Security.Cryptography;
using Lab5_6.DAL;
using Lab5_6.Misc;
using Lab5_6.Models;
using NSec.Cryptography;
using System;
using System.Linq;
using System.Text;

namespace Lab5_6.Business
{
    public class UserManager : IUserManager
    {
        private const int saltLength = 16;

        private readonly IUserRepository _userRepository;

        private readonly KeyBlobFormat _keyBlobFormat;
        private readonly byte[] _key;

        public UserManager(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

            using Key key = Key.Create(
                AeadAlgorithm.Aes256Gcm, 
                new KeyCreationParameters 
                { 
                    ExportPolicy = KeyExportPolicies.AllowPlaintextExport 
                });
            _keyBlobFormat = KeyBlobFormat.NSecSymmetricKey;
            _key = key.Export(_keyBlobFormat);
        }

        public bool CreateUser(UserViewModel registerUserViewModel)
        {
            byte[] saltBytes = CreateSalt(saltLength);
            byte[] passwordHash = GetStronglyHashedRecord(
                HashToSameSize(registerUserViewModel.Password), saltBytes);
            
            User user = new User
            {
                Email = registerUserViewModel.Email,
                PasswordHash = passwordHash.ByteArrayToHexString(),
                PasswordSalt = saltBytes.ByteArrayToHexString()
            };
            
            return _userRepository.StoreUser(user);
        }

        public bool LoginUser(UserViewModel registerUserViewModel)
        {
            User user = _userRepository.GetUserByEmail(registerUserViewModel.Email);

            if(user == null)
            {
                string randomPassword = CreateRandomPassword(new Random().Next(8, 20));
                GetStronglyHashedRecord(
                    HashToSameSize(randomPassword), CreateSalt(saltLength));
                return false;
            }
            else
            {
                byte[] passwordHash = GetStronglyHashedRecord(
                    HashToSameSize(registerUserViewModel.Password), user.PasswordSalt.HexStringToByteArray());
                return user.PasswordHash.HexStringToByteArray().SequenceEqual(passwordHash);
            }
        }

        public bool StoreSensitiveData(SensitiveDataViewModel sensitiveDataViewModel)
        {
            User user = _userRepository.GetUserByEmail(sensitiveDataViewModel.Email);

            byte[] phoneNumberNonce = CreateSalt(AeadAlgorithm.Aes256Gcm.NonceSize);
            byte[] creditCardNonce = CreateSalt(AeadAlgorithm.Aes256Gcm.NonceSize);

            user.PhoneNumberEncrypted = EncryptSensitiveData(sensitiveDataViewModel.PhoneNumber, _key, phoneNumberNonce);
            user.PhoneNumberNonce = phoneNumberNonce.ByteArrayToHexString();
            user.CreditCardEncrypted = EncryptSensitiveData(sensitiveDataViewModel.CreditCard, _key, creditCardNonce);
            user.CreditCardNonce = creditCardNonce.ByteArrayToHexString();

            return _userRepository.UpdateSensitiveUserData(user);
        }

        public SensitiveDataViewModel GetSensitiveData(string userEmail)
        {
            User user = _userRepository.GetUserByEmail(userEmail);

            return new SensitiveDataViewModel
            {
                Email = userEmail,
                PhoneNumber = DecryptSensitiveData(user.PhoneNumberEncrypted, _key, user.PhoneNumberNonce.HexStringToByteArray()),
                CreditCard = DecryptSensitiveData(user.CreditCardEncrypted, _key, user.CreditCardNonce.HexStringToByteArray()),
            };
        }

        private string EncryptSensitiveData(string data, byte[] keyBytes, byte[] nonce)
        {
            AeadAlgorithm aeadAlgorithm = AeadAlgorithm.Aes256Gcm;

            using Key key = Key.Import(AeadAlgorithm.Aes256Gcm, keyBytes, _keyBlobFormat);

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            byte[] encrypted =
                aeadAlgorithm.Encrypt(key, nonce, null, Encoding.UTF8.GetBytes(data));

            return encrypted.ByteArrayToHexString();
        }

        private string DecryptSensitiveData(string encryptedData, byte[] keyBytes, byte[] nonce)
        {
            AeadAlgorithm aeadAlgorithm = AeadAlgorithm.Aes256Gcm;

            using Key key = Key.Import(AeadAlgorithm.Aes256Gcm, keyBytes, _keyBlobFormat);

            byte[] dataBytes = encryptedData.HexStringToByteArray();

            byte[] decrypted =
                aeadAlgorithm.Decrypt(key, nonce, null, Encoding.UTF8.GetBytes(encryptedData));

            return decrypted.ByteArrayToHexString();
        }

        private static byte[] HashToSameSize(string data)
        {
            HashAlgorithm hashAlgorithm = HashAlgorithm.Sha512;

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            return hashAlgorithm.Hash(dataBytes);
        }

        public static byte[] GetStronglyHashedRecord(byte[] input, byte[] salt)
        {
            var argon2 = new Argon2id(input);

            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 8;
            argon2.Iterations = 4;
            argon2.MemorySize = 1024 * 1024;

            byte[] hash = argon2.GetBytes(saltLength);

            return hash;
        }

        private static byte[] CreateSalt(int saltLength)
        {
            var buffer = new byte[saltLength];
            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            return buffer;
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
    }
}
