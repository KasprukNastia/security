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
    public class Player
    {
        public static string LcgMode => "Lcg";
        public static string MtMode => "Mt";
        public static string BetterMtMode => "BetterMt";

        private readonly string _lcgStateFilePath;

        private readonly IAccountProvider _accountProvider;
        private readonly ILcgParamsProvider _lcgParamsProvider;
        private readonly HttpClient _httpClient;        
        private readonly ConnectionSettings _connectionSettings;
        private readonly int _betPersentage;

        private PlayResult _playState;

        public Player(
            IAccountProvider accountProvider,
            ILcgParamsProvider lcgParamsProvider,
            string playStateFilePath,
            HttpClient httpClient,
            ConnectionSettings connectionSettings,
            int betPersentage = 20)
        {
            _lcgParamsProvider = lcgParamsProvider ?? throw new ArgumentNullException(nameof(lcgParamsProvider));
            _accountProvider = accountProvider ?? throw new ArgumentNullException(nameof(accountProvider));
            _lcgStateFilePath = playStateFilePath ?? throw new ArgumentNullException(nameof(playStateFilePath));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
            
            if(betPersentage < 0 || betPersentage > 100)
                throw new ArgumentException($"{nameof(betPersentage)} must be value from 0 to 100");
            _betPersentage = betPersentage;
        }

        public async Task PlayLcg()
        {
            await GetCurrentPlayStateAsync();
            LcgParams lcgParams = await _lcgParamsProvider.GetLcgParamsAcync();

            long nextNumber;
            while(_playState.Account.Money < 1000000)
            {
                nextNumber = (lcgParams.Multiplier * _playState.RealNumber.Value + lcgParams.Increment) % lcgParams.Modulus;
                _playState = await GetSuccessfulPlayResponse(
                    mode: LcgMode,
                    account: _playState.Account, 
                    bet: (int)_playState.Account.Money.Value * _betPersentage / 100, 
                    number: nextNumber);
            }
        }

        private async Task<PlayResult> GetCurrentPlayStateAsync(string mode, string playStateFilePath)
        {
            // Checking if already exists
            if (_playState != null)
                return _playState;

            // Reading from file
            string playStateJsonData = File.ReadAllText(playStateFilePath);
            _playState = JsonConvert.DeserializeObject<PlayResult>(playStateJsonData);
            if (_playState != null)
                return _playState;

            // Getting from external source
            Account account = await _accountProvider.GetAccountAcync();
            _playState = await GetSuccessfulPlayResponse(mode, account, 1, new Random().Next(0, 100));
            return _playState;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<PlayResult> GetSuccessfulPlayResponse(string mode, Account account, int bet, long number)
        {
            string requestStr = $"{_connectionSettings.PlayAddress}{mode}?id={account.Id}&bet={bet}&number={number}";
            HttpRequestMessage request;
            HttpResponseMessage response;
            do
            {
                request = new HttpRequestMessage(HttpMethod.Get, requestStr);
                response = await _httpClient.SendAsync(request);
            } while (!response.IsSuccessStatusCode);

            string responseStr = await response.Content.ReadAsStringAsync();

            PlayResult playState = JsonConvert.DeserializeObject<PlayResult>(responseStr);
            if (playState != null)
                File.WriteAllText(_lcgStateFilePath, responseStr);
            else
                throw new InvalidOperationException($"Unknown data was reseived from the external source: {responseStr}");

            return playState;
        }
    }
}
