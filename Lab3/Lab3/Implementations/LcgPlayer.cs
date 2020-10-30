using Lab3.Interfaces;
using System;
using System.Net.Http;

namespace Lab3.Implementations
{
    public class LcgPlayer
    {
        private static readonly HttpClient _client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private readonly IAccountProvider _accountProvider;
        private readonly ConnectionSettings _connectionSettings;

        public LcgPlayer(
            IAccountProvider accountProvider, 
            ConnectionSettings connectionSettings)
        {
            _accountProvider = accountProvider ?? throw new ArgumentNullException(nameof(accountProvider));
            _connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
        }


    }
}
