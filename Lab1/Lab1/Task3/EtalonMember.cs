using System;

namespace Lab1.Task3
{
    public class EtalonMember
    {
        public string Key { get; set; }
        public double Frequency { get; set; }

        public EtalonMember(string key, float frequency)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Frequency = frequency;
        }
    }
}
