using Lab3.Interfaces;
using Lab3.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lab3.Implementations
{
    public class AccountProvider : IAccountProvider
    {
        private static readonly HttpClient _client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private readonly string _accountFilePath;
        private readonly ConnectionSettings _connectionSettings;

        private Account _account;

        public AccountProvider(string accountFilePath, ConnectionSettings connectionSettings)
        {
            _accountFilePath = accountFilePath ?? throw new ArgumentNullException(nameof(accountFilePath));
            _connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
        }

        public async Task<Account> GetAccount()
        {
            if (_account != null)
                return _account;

            string accountJsonData = File.ReadAllText(_accountFilePath);
            _account = JsonConvert.DeserializeObject<Account>(accountJsonData);
            if (_account != null && _account.Id.HasValue)
                return _account;               

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_connectionSettings.CreateAccAddress}");
            HttpResponseMessage response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;
            string responseStr = await response.Content.ReadAsStringAsync();

            _account = JsonConvert.DeserializeObject<Account>(responseStr);
            if (_account != null && _account.Id.HasValue)
                File.WriteAllText(_accountFilePath, responseStr);                

            return _account;
        }
    }
}
