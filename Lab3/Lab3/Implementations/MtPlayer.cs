using Lab3.Interfaces;
using Lab3.Models;
using Microsoft.VisualBasic;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lab3.Implementations
{
    public class MtPlayer : Player
    {
        public override string Mode => "Mt";

        public MtPlayer(
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

            long seed = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _playState = await GetSuccessfulPlayResponseAsync(account, 1, seed);          

            int counter = 0;
            MT19937 mt;
            long nextNum = 0;
            do
            {
                seed += counter;
                mt = new MT19937();
                mt.init_genrand((ulong)seed);
                counter++;
                nextNum = (long)mt.genrand_int32();
            } while (_playState.RealNumber != nextNum);

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
