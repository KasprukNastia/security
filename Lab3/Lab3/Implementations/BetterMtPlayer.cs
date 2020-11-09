using Lab3.Interfaces;
using Lab3.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lab3.Implementations
{
    public class BetterMtPlayer : Player
    {
        private const int stateSize = 624;
        public override string Mode => "BetterMt";

        public BetterMtPlayer(
            IAccountProvider accountProvider, 
            HttpClient httpClient, 
            ConnectionSettings connectionSettings, 
            string playStateFilePath = null, 
            string accountFilePath = null, 
            int betPersentage = 20) 
            : base(accountProvider, httpClient, connectionSettings, playStateFilePath, accountFilePath, betPersentage)
        {
        }

        public override async Task Play()
        {
            Account account = await _accountProvider.GetAccountAcync();

            var random = new Random();
            ulong[] states = new ulong[stateSize];
            var mt = new MT19937();
            for (int i = 0; i < stateSize; i++)
            {
                _playState = await GetSuccessfulPlayResponseAsync(
                    account: account,
                    bet: 1,
                    number: random.Next(0, 100));
                states[i] = mt.untempering((ulong)_playState.RealNumber);
            }

            mt = new MT19937();
            mt.init_genrand(states);
            long nextNum;
            while (_playState.Account.Money < 1000000)
            {
                nextNum = (long)mt.genrand_int32();
                _playState = await GetSuccessfulPlayResponseAsync(
                    account: _playState.Account,
                    bet: (int)_playState.Account.Money.Value * _betPersentage / 100,
                    number: nextNum);
            }
        }
    }
}
