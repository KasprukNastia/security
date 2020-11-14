using Lab3.Interfaces;
using Lab3.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Lab3.Implementations
{
    public abstract class Player
    {
        private readonly HttpClient _httpClient;        
        private readonly ConnectionSettings _connectionSettings;

        private readonly string _accountFilePath;

        protected readonly string _playStateFilePath;
        protected readonly int _betPersentage;
        protected readonly IAccountProvider _accountProvider;

        protected PlayResult _playState;

        public abstract string Mode { get; }

        public Player(
            IAccountProvider accountProvider,
            HttpClient httpClient,
            ConnectionSettings connectionSettings,
            string playStateFilePath = null,
            string accountFilePath = null,
            int betPersentage = 20)
        {
            _accountProvider = accountProvider ?? throw new ArgumentNullException(nameof(accountProvider));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));

            _playStateFilePath = playStateFilePath;
            _accountFilePath = accountFilePath;

            if (betPersentage < 0 || betPersentage > 100)
                throw new ArgumentException($"{nameof(betPersentage)} must be value from 0 to 100");
            _betPersentage = betPersentage;
        }

        public abstract Task Play();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected async Task<PlayResult> GetSuccessfulPlayResponseAsync(Account account, int bet, long number)
        {
            string requestStr = $"{_connectionSettings.PlayAddress}{Mode}?id={account.Id}&bet={bet}&number={number}";
            HttpRequestMessage request;
            HttpResponseMessage response;
            do
            {
                request = new HttpRequestMessage(HttpMethod.Get, requestStr);
                response = await _httpClient.SendAsync(request);
            } while (!response.IsSuccessStatusCode);

            string responseStr = await response.Content.ReadAsStringAsync();
            PlayResult playState = JsonConvert.DeserializeObject<PlayResult>(responseStr);

            if (playState == null)
                throw new InvalidOperationException($"Unknown data was reseived from the external source: {responseStr}");

            if(!string.IsNullOrWhiteSpace(_playStateFilePath))
                File.WriteAllText(_playStateFilePath, responseStr);
            if (!string.IsNullOrWhiteSpace(_accountFilePath))
                File.WriteAllText(_accountFilePath, JsonConvert.SerializeObject(playState.Account));

            return playState;
        }
    }
}
