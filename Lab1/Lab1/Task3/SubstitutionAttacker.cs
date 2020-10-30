using Lab1.Task1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Lab1.Task3
{
    public class SubstitutionAttacker
    {
        private readonly List<EtalonMember> _twoLettersFrequencies;
        private readonly List<EtalonMember> _threeLettersFrequencies;

        public string Alphabet { get; }
        public string EncryptedText { get; }
        public int MinPopulationSize { get; }
        public int MaxPopulationSize { get; }
        public int IterationsCount { get; }
        public double MutationPercentage { get; }
        public int BestPercentage { get; }
        public double TwoLettersFittingQuotientCoef { get; }
        public double ThreeLettersFittingQuotientCoef { get; }

        public SubstitutionAttacker(
            string encryptedText,
            int minPopulationSize,
            int maxPopulationSize,
            int iterationsCount,
            double mutationPercentage,
            int bestPercentage,
            List<EtalonMember> twoLettersFrequencies,
            List<EtalonMember> threeLettersFrequencies,
            double twoLettersFittingQuotientCoef = 1,
            double threeLettersFittingQuotientCoef = 1)
        {
            EncryptedText = encryptedText ?? throw new ArgumentNullException(nameof(encryptedText));
            EncryptedText = EncryptedText.ToLower();

            if (minPopulationSize <= 0)
                throw new ArgumentException($"{nameof(minPopulationSize)} must be  greather than 0");
            MinPopulationSize = minPopulationSize;

            if (maxPopulationSize <= 0)
                throw new ArgumentException($"{nameof(maxPopulationSize)} must be  greather than 0");
            MaxPopulationSize = maxPopulationSize;

            if(minPopulationSize > maxPopulationSize)
                throw new ArgumentException($"{nameof(maxPopulationSize)} must be equal to or greather than {nameof(minPopulationSize)}");

            if (iterationsCount <= 0)
                throw new ArgumentException($"{nameof(iterationsCount)} must be  greather than 0");
            IterationsCount = iterationsCount;

            if (mutationPercentage <= 0 || mutationPercentage > 100)
                throw new ArgumentException($"{nameof(mutationPercentage)} must be value from 0 to 100");
            MutationPercentage = mutationPercentage;

            if (bestPercentage <= 0 || bestPercentage > 100)
                throw new ArgumentException($"{nameof(bestPercentage)} must be value from 0 to 100");
            BestPercentage = bestPercentage;

            _twoLettersFrequencies = twoLettersFrequencies ?? throw new ArgumentNullException(nameof(twoLettersFrequencies));
            _threeLettersFrequencies = threeLettersFrequencies ?? throw new ArgumentNullException(nameof(threeLettersFrequencies));

            Alphabet = new string(SingleByteXorAttacker.OneLetterEnglishFrequency.Select(c => c.Key).ToArray());

            TwoLettersFittingQuotientCoef = twoLettersFittingQuotientCoef;
            ThreeLettersFittingQuotientCoef = threeLettersFittingQuotientCoef;
        }

        public async Task<List<Individual>> Evaluate()
        {
            Dictionary<string, Task<Individual>> population =
                GeneratePoputation(MaxPopulationSize).ToDictionary(i => i.Key, i => CalcFitness(i));
            await Task.WhenAll(population.Values);

            var random = new Random();
            IOrderedEnumerable<KeyValuePair<string, Task<Individual>>> orderedPopulation;
            IEnumerable<KeyValuePair<string, Task<Individual>>> best;
            int currentBestCount;
            int totalGenerated = 0;
            int mutatedCount = 0;
            bool needsMutation = false;
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < IterationsCount; i++)
            {
                Console.WriteLine($"Iteration: {i}");
                orderedPopulation = population.OrderByDescending(i => i.Value.Result.Fitness);
                Console.WriteLine($"Best key: {population.First().Key}");

                currentBestCount = orderedPopulation.Count() * BestPercentage / 100;                
                best = orderedPopulation.Take(currentBestCount);

                population = new Dictionary<string, Task<Individual>>(best);
                totalGenerated = population.Count;
                mutatedCount = 0;
                needsMutation = false;

                sw.Start();
                while (totalGenerated < MaxPopulationSize || population.Count < MinPopulationSize)
                {
                    needsMutation = mutatedCount * 100 / (double)(totalGenerated - population.Count) < MutationPercentage;
                    if (needsMutation)
                        mutatedCount += 2;
                    Crossover(best.ElementAt(random.Next(0, currentBestCount - 1)).Value.Result,
                        best.ElementAt(random.Next(0, currentBestCount - 1)).Value.Result,
                        needsMutation)
                        .ForEach(i =>
                        {
                            population.TryAdd(i.Key, CalcFitness(i));
                        });
                    totalGenerated += 2;
                }
                await Task.WhenAll(population.Values);
                sw.Stop();
                Console.WriteLine($"Population generation time: {sw.ElapsedMilliseconds} ms");
                
                sw.Reset();
                Console.WriteLine();
            }

            return population.Values.Select(val => val.Result).ToList();
        }

        private IEnumerable<Individual> GeneratePoputation(int size)
        {
            var random = new Random();
            var population = new HashSet<Individual>(size, new IndividualsComparer());
            Individual individual;
            while (population.Count != size)
            {
                individual = new Individual(new string(Alphabet.ToCharArray().OrderBy(s => (random.Next(2) % 2) == 0).ToArray()), Alphabet.Length);
                population.Add(individual);
            }

            return population;
        }

        private async Task<Individual> CalcFitness(Individual individual)
        {
            var substitution = new Substitution(Alphabet, individual.Key);
            string decryptedMessage = substitution.Decrypt(EncryptedText);

            IEnumerable<double> results = await Task.WhenAll(
                CalcFittingQuotient(decryptedMessage, _twoLettersFrequencies),
                CalcFittingQuotient(decryptedMessage, _threeLettersFrequencies));

            individual.Fitness = TwoLettersFittingQuotientCoef * results.ElementAt(0) +
                ThreeLettersFittingQuotientCoef * results.ElementAt(1);

            return individual;
        }

        private List<Individual> Crossover(Individual first, Individual second, bool needsMutation = false)
        {
            var random = new Random();
            int randomPos = random.Next(1, Alphabet.Length - 1);

            IEnumerable<char> keyPart = first.Key.Take(randomPos);
            char[] firstChildKey = keyPart.Concat(second.Key.Where(l => !keyPart.Contains(l))).ToArray();

            keyPart = second.Key.Take(randomPos);
            char[] secondChildKey = keyPart.Concat(first.Key.Where(l => !keyPart.Contains(l))).ToArray();

            if (needsMutation)
            {
                int firstLetterIndex = random.Next(0, randomPos);
                int secondLetterIndex = random.Next(randomPos, Alphabet.Length - 1);

                Swap(firstChildKey, firstLetterIndex, secondLetterIndex);
                Swap(secondChildKey, firstLetterIndex, secondLetterIndex);
            }

            var result = new List<Individual>
            {
                new Individual(new string(firstChildKey), Alphabet.Length),
                new Individual(new string(secondChildKey), Alphabet.Length)
            };

            return result;

            void Swap(char[] childKey, int firstLetterIndex, int secondLetterIndex)
            {
                char temp = childKey[firstLetterIndex];
                childKey[firstLetterIndex] = childKey[secondLetterIndex];
                childKey[secondLetterIndex] = temp;
            }
        }

        private Task<double> CalcFittingQuotient(string decryptedMessage, List<EtalonMember> etalons)
        {
            int numberOfMatches = 0,
                index = -1,
                etalonMemberKeyLength = etalons.First().Key.Length;
            double currentFrequency;
            double tempDeviationSum = 0;
            foreach (var frequency in etalons)
            {
                do
                {
                    index = decryptedMessage.IndexOf(frequency.Key, index + 1, StringComparison.Ordinal);
                    numberOfMatches++;
                } while (index >= 0 && index < decryptedMessage.Length - etalonMemberKeyLength - 1);
                numberOfMatches--;

                currentFrequency = numberOfMatches * 100 / decryptedMessage.Length - etalonMemberKeyLength - 1;
                tempDeviationSum += Math.Abs(frequency.Frequency - currentFrequency);
            }

            return Task.FromResult(tempDeviationSum / etalons.Count);
        }

        class IndividualsComparer : IEqualityComparer<Individual>
        {
            public bool Equals([AllowNull] Individual x, [AllowNull] Individual y)
            {
                return x.Key.Equals(y.Key, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode([DisallowNull] Individual obj)
            {
                return obj.Key.ToLower().GetHashCode();
            }
        }
    }
}