namespace Lab3
{
    public class ConnectionSettings
    {
        public string BaseAddress => "http://95.217.177.249/casino";
        public string CreateAccAddress => $"{BaseAddress}/createacc";
        public string PlayAddress => $"{BaseAddress}/play";

        public ConnectionSettings(string baseAddress, string createAccAddress, string playAddress)
        {

        }
    }
}
