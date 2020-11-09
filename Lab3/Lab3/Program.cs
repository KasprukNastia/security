using Lab3.Implementations;
using Lab3.Interfaces;
using System;
using System.IO;
using System.Net.Http;

namespace Lab3
{
    class Program
    {
        public static int PlayerId = 1718;

        static void Main(string[] args)
        {
            RunMt();
        }

        public static void RunLcg()
        {
            HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            string baseAddress = "http://95.217.177.249/casino";
            ConnectionSettings connectionSettings = new ConnectionSettings(
                baseAddress: baseAddress,
                createAccAddress: $"{baseAddress}/createacc",
                playAddress: $"{baseAddress}/play");

            string baseFilePath = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent}\\Data";
            string accountFilePath = $"{baseFilePath}\\my_account_data.json";            
            IAccountProvider accountProvider = new AccountProvider(
                playerId: PlayerId,
                httpClient,
                connectionSettings,
                accountFilePath);
            string playStateFilePath = $"{baseFilePath}\\current_lcg_play_state.json";
            ILcgParamsProvider lcgParamsProvider = new LcgParamsProvider(              
                httpClient,
                connectionSettings,
                accountProvider,
                lcgParamsFilePath: $"{baseFilePath}\\lcg_params.json",
                currentPlayStateFilePath: playStateFilePath,
                accountFilePath: accountFilePath);
            Player lcgPlayer = new LcgPlayer(
                lcgParamsProvider,
                accountProvider,
                httpClient,
                connectionSettings,
                playStateFilePath: playStateFilePath,
                accountFilePath: accountFilePath,
                betPersentage: 1);

            lcgPlayer.Play().Wait();
        }

        public static void RunMt()
        {
            HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            string baseAddress = "http://95.217.177.249/casino";
            ConnectionSettings connectionSettings = new ConnectionSettings(
                baseAddress: baseAddress,
                createAccAddress: $"{baseAddress}/createacc",
                playAddress: $"{baseAddress}/play");

            string baseFilePath = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent}\\Data";
            string accountFilePath = $"{baseFilePath}\\my_account_data.json";
            IAccountProvider accountProvider = new AccountProvider(
                playerId: PlayerId,
                httpClient,
                connectionSettings,
                accountFilePath);
            string playStateFilePath = $"{baseFilePath}\\current_mt_play_state.json";
            Player mtPlayer = new MtPlayer(
                accountProvider,
                httpClient,
                connectionSettings,
                playStateFilePath: playStateFilePath,
                accountFilePath: accountFilePath,
                betPersentage: 1);

            mtPlayer.Play().Wait();
        }

        public static void RunBetterMt()
        {
            HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            string baseAddress = "http://95.217.177.249/casino";
            ConnectionSettings connectionSettings = new ConnectionSettings(
                baseAddress: baseAddress,
                createAccAddress: $"{baseAddress}/createacc",
                playAddress: $"{baseAddress}/play");

            string baseFilePath = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent}\\Data";
            string accountFilePath = $"{baseFilePath}\\my_account_data.json";
            IAccountProvider accountProvider = new AccountProvider(
                playerId: PlayerId,
                httpClient,
                connectionSettings,
                accountFilePath);
            string playStateFilePath = $"{baseFilePath}\\current_better_mt_play_state.json";
            Player betterMtPlayer = new BetterMtPlayer(
                accountProvider,
                httpClient,
                connectionSettings,
                playStateFilePath: playStateFilePath,
                accountFilePath: accountFilePath,
                betPersentage: 1);

            betterMtPlayer.Play().Wait();
        }
    }
}
