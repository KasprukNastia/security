using System;

namespace Lab1.Task3
{
    public class Individual
    {
        public string Key { get; }
        public double Fitness { get; set; }

        public Individual(string key, int neededKeyLength)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (key.Length != neededKeyLength)
                throw new ArithmeticException(
                    $"Invalid {nameof(Individual)} {nameof(key)} parameter length. Expected: {neededKeyLength}, actual: {key.Length}");
            Key = key.ToLower();
        }
    }
}
