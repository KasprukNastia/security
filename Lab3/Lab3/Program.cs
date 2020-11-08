using Lab3.Implementations;
using Lab3.Interfaces;
using System;
using System.IO;
using System.Net.Http;

namespace Lab3
{
    class Program
    {
        public static object RandomMT { get; private set; }

        static void Main(string[] args)
        {
            var mt = new MT19937();
            mt.init_genrand(1131464071);

            for(int i = 0; i < 100; i++)
            {
                Console.WriteLine(mt.genrand_int32());
            }
        }

        public static void Run()
        {
            HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            string baseAddress = "http://95.217.177.249/casino";
            ConnectionSettings connectionSettings = new ConnectionSettings(
                baseAddress: baseAddress,
                createAccAddress: $"{baseAddress}/createacc",
                playAddress: $"{baseAddress}/play");

            string baseFilePath = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent}\\Data";
            IAccountProvider accountProvider = new AccountProvider(
                playerId: 1701,
                $"{baseFilePath}\\my_account_data.json",
                httpClient,
                connectionSettings);
            string currentLcgPlayStateFilePath = $"{baseFilePath}\\current_lcg_play_state.json";
            ILcgParamsProvider lcgParamsProvider = new LcgParamsProvider(
                $"{baseFilePath}\\lcg_params.json",
                httpClient,
                connectionSettings,
                accountProvider,
                currentPlayStateFilePath: currentLcgPlayStateFilePath);
            Player lcgPlayer = new Player(
                accountProvider,
                lcgParamsProvider,
                currentLcgPlayStateFilePath,
                httpClient,
                connectionSettings,
                betPersentage: 1);

            lcgPlayer.PlayLcg().Wait();
        }
    }
}
