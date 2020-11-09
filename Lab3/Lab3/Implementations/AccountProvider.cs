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
        private readonly int _playerId;
        private readonly string _accountFilePath;
        private readonly HttpClient _httpClient;
        private readonly ConnectionSettings _connectionSettings;

        private Account _account;

        public AccountProvider(
            int playerId,
            HttpClient httpClient,
            ConnectionSettings connectionSettings,
            string accountFilePath = null)
        {
            _playerId = playerId;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
            _accountFilePath = accountFilePath;
        }

        public async Task<Account> GetAccountAcync()
        {
            // Checking if already exists
            if (_account != null)
                return _account;

            // Reading from file
            string accountJsonData = File.ReadAllText(_accountFilePath);
            _account = JsonConvert.DeserializeObject<Account>(accountJsonData);
            if (_account != null && _account.Id.HasValue && _account.Id.Value == _playerId)
                return _account;

            // Getting at first time
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_connectionSettings.CreateAccAddress}?id={_playerId}");
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;
            string responseStr = await response.Content.ReadAsStringAsync();
            _account = JsonConvert.DeserializeObject<Account>(responseStr);           
            
            if (_account == null || !_account.Id.HasValue)
                throw new InvalidOperationException($"Unknown data was reseived from the external source: {responseStr}");

            if(!string.IsNullOrWhiteSpace(_accountFilePath))
                File.WriteAllText(_accountFilePath, responseStr);

            return _account;
        }
    }
}
