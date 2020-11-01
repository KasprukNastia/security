﻿using Lab3.Interfaces;
using Lab3.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lab3.Implementations
{
    public class LcgParamsProvider : ILcgParamsProvider
    {
        public static string Mode => "Lcg";

        private readonly string _lcgParamsFilePath;
        private readonly string _currentPlayStateFilePath;
        private readonly HttpClient _httpClient;
        private readonly ConnectionSettings _connectionSettings;
        private readonly IAccountProvider _accountProvider;

        private LcgParams _lcgParams;

        public long Modulus { get; }

        public LcgParamsProvider(
            string lcgParamsFilePath,
            HttpClient httpClient,
            ConnectionSettings connectionSettings,
            IAccountProvider accountProvider,
            long? modulus = null,
            string currentPlayStateFilePath = null)
        {
            _lcgParamsFilePath = lcgParamsFilePath ?? throw new ArgumentNullException(nameof(lcgParamsFilePath));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
            _accountProvider = accountProvider ?? throw new ArgumentNullException(nameof(accountProvider));
            Modulus = modulus.HasValue ? modulus.Value : (long)Math.Pow(2, 32);
            _currentPlayStateFilePath = currentPlayStateFilePath;
        }

        public async Task<LcgParams> GetLcgParamsAcync()
        {
            // Checking if already exists
            if (_lcgParams != null)
                return _lcgParams;

            // Reading from file
            string lcgParamsJsonData = File.ReadAllText(_lcgParamsFilePath);
            _lcgParams = JsonConvert.DeserializeObject<LcgParams>(lcgParamsJsonData);
            if (_lcgParams != null)
                return _lcgParams;

            // Calculating at first time
            Account account = await _accountProvider.GetAccountAcync();
            string requestStr = $"{_connectionSettings.PlayAddress}{Mode}?id={account.Id}&bet={1}&number={new Random().Next(0, 100)}";
            List<PlayResult> playResults = new List<PlayResult>(3);

            HttpRequestMessage request;
            HttpResponseMessage response;
            string responseStr;
            PlayResult playResult;
            while (playResults.Count < 3)
            {
                request = new HttpRequestMessage(HttpMethod.Get, requestStr);
                response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    continue;

                responseStr = await response.Content.ReadAsStringAsync();
                playResult = JsonConvert.DeserializeObject<PlayResult>(responseStr);
                if (playResult != null)
                    playResults.Add(playResult);
                else
                    throw new InvalidOperationException($"Unknown data was reseived from the external source: {responseStr}");
            }

            var lcgParams = new LcgParams { Modulus = Modulus };
            lcgParams.Multiplier = 
                (playResults[2].RealNumber.Value - playResults[1].RealNumber.Value) *
                (playResults[1].RealNumber.Value - playResults[0].RealNumber.Value).ModInverse(Modulus) 
                % Modulus;
            lcgParams.Increment = 
                (playResults[1].RealNumber.Value - lcgParams.Multiplier * playResults[0].RealNumber.Value) % Modulus;

            File.WriteAllText(_lcgParamsFilePath, JsonConvert.SerializeObject(lcgParams));
            if(!string.IsNullOrWhiteSpace(_currentPlayStateFilePath))
                File.WriteAllText(_currentPlayStateFilePath, JsonConvert.SerializeObject(playResults.Last()));

            return lcgParams;
        }
    }
}