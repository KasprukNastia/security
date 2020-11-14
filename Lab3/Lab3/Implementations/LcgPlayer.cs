using Lab3.Interfaces;
using Lab3.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lab3.Implementations
{
    public class LcgPlayer : Player
    {
        private readonly ILcgParamsProvider _lcgParamsProvider;

        public override string Mode => "Lcg";

        public LcgPlayer(
            ILcgParamsProvider lcgParamsProvider,
            IAccountProvider accountProvider,
            HttpClient httpClient,
            ConnectionSettings connectionSettings,
            string playStateFilePath = null,
            string accountFilePath = null,
            int betPersentage = 20) 
            : base(accountProvider, httpClient, connectionSettings, playStateFilePath, accountFilePath, betPersentage)
        {
            _lcgParamsProvider = lcgParamsProvider ?? throw new ArgumentNullException(nameof(lcgParamsProvider));
        }

        public override async Task Play()
        {
            _playState = await GetRandomPlayStateAsync();
            LcgParams lcgParams = await _lcgParamsProvider.GetLcgParamsAcync();

            long nextNumber;
            while (_playState.Account.Money < 1000000)
            {
                nextNumber = (lcgParams.Multiplier * _playState.RealNumber + lcgParams.Increment) % lcgParams.Modulus;
                _playState = await GetSuccessfulPlayResponseAsync(
                    account: _playState.Account,
                    bet: (int)_playState.Account.Money.Value * _betPersentage / 100,
                    number: nextNumber);
            }
        }

        private async Task<PlayResult> GetRandomPlayStateAsync()
        {
            // Checking if already exists
            if (_playState != null)
                return _playState;

            // Reading from file
            string playStateJsonData = File.ReadAllText(_playStateFilePath);
            _playState = JsonConvert.DeserializeObject<PlayResult>(playStateJsonData);
            if (_playState != null)
                return _playState;

            // Getting from external source
            Account account = await _accountProvider.GetAccountAcync();
            _playState = await GetSuccessfulPlayResponseAsync(account, 1, new Random().Next(0, 100));
            return _playState;
        }
    }
}
