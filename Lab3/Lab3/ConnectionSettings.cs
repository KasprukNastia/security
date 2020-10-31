using System;

namespace Lab3
{
    public class ConnectionSettings
    {
        public string BaseAddress { get; }
        public string CreateAccAddress { get; }
        public string PlayAddress { get; }

        public ConnectionSettings(string baseAddress, string createAccAddress, string playAddress)
        {
            if (!Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
                throw new UriFormatException($"Bad formed url: {baseAddress}");
            BaseAddress = baseAddress;

            if (!Uri.IsWellFormedUriString(createAccAddress, UriKind.Absolute))
                throw new UriFormatException($"Bad formed url: {createAccAddress}");
            CreateAccAddress = createAccAddress;

            if (!Uri.IsWellFormedUriString(playAddress, UriKind.Absolute))
                throw new UriFormatException($"Bad formed url: {playAddress}");
            PlayAddress = playAddress;
        }
    }
}
