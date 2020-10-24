using Lab1.Task1;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Lab1.Task3
{
    public class SubstitutionAttacker
    {
        private readonly Dictionary<string, float> _twoLettersFrequencies;
        private readonly List<EtalonMember> _threeLettersFrequencies;
        private readonly SingleByteXorAttacker _singleByteXorAttacker;

        public string Alphabet { get; }
        public string EncryptedText { get; }
        public int PopulationSize { get; }
        public int IterationsCount { get; }
        public float MutationPercentage { get; }
        public int BestPercentage { get; }
        public int BestCount => PopulationSize * BestPercentage / 100;
        public int ToNextGenerationPercentage { get; }       
        public int ToNextGenerationCount => PopulationSize * ToNextGenerationPercentage / 100;
        public float OneLetterFittingQuotientCoef { get; }
        public float TwoLettersFittingQuotientCoef { get; }
        public float ThreeLettersFittingQuotientCoef { get; }

        public SubstitutionAttacker(
            string encryptedText,
            int populationSize,
            int iterationsCount,
            float mutationPercentage,
            int bestPercentage,
            int toNextGenerationPercentage,
            Dictionary<string, float> twoLettersFrequencies,
            List<EtalonMember> threeLettersFrequencies,
            float oneLetterFittingQuotientCoef = 1,
            float twoLettersFittingQuotientCoef = 1,
            float threeLettersFittingQuotientCoef = 1)
        {
            EncryptedText = encryptedText ?? throw new ArgumentNullException(nameof(encryptedText));
            EncryptedText = EncryptedText.ToLower();

            if (populationSize <= 0)
                throw new ArgumentException($"{nameof(populationSize)} must be  greather than 0");
            PopulationSize = populationSize;

            if (iterationsCount <= 0)
                throw new ArgumentException($"{nameof(iterationsCount)} must be  greather than 0");
            IterationsCount = iterationsCount;

            if (mutationPercentage <= 0 || mutationPercentage > 100)
                throw new ArgumentException($"{nameof(mutationPercentage)} must be value from 0 to 100");
            MutationPercentage = mutationPercentage;

            if (bestPercentage <= 0 || bestPercentage > 100)
                throw new ArgumentException($"{nameof(bestPercentage)} must be value from 0 to 100");
            BestPercentage = bestPercentage;

            if (toNextGenerationPercentage <= 0 || toNextGenerationPercentage > 100)
                throw new ArgumentException($"{nameof(toNextGenerationPercentage)} must be value from 0 to 100");
            ToNextGenerationPercentage = toNextGenerationPercentage;
            
            _twoLettersFrequencies = twoLettersFrequencies ?? throw new ArgumentNullException(nameof(twoLettersFrequencies));
            _threeLettersFrequencies = threeLettersFrequencies ?? throw new ArgumentNullException(nameof(threeLettersFrequencies));

            _singleByteXorAttacker = new SingleByteXorAttacker();
            Alphabet = new string(SingleByteXorAttacker.OneLetterEnglishFrequency.Select(c => c.Key).ToArray());

            OneLetterFittingQuotientCoef = oneLetterFittingQuotientCoef;
            TwoLettersFittingQuotientCoef = twoLettersFittingQuotientCoef;
            ThreeLettersFittingQuotientCoef = threeLettersFittingQuotientCoef;
        }

        public async Task<List<Individual>> Evaluate()
        {
            Dictionary<string, Task<Individual>> population =
                GeneratePoputation(PopulationSize).ToDictionary(i => i.Key, i => CalcFitness(i));
            await Task.WhenAll(population.Values);

            var random = new Random();
            IOrderedEnumerable<KeyValuePair<string, Task<Individual>>> orderedPopulation;
            IEnumerable<KeyValuePair<string, Task<Individual>>> best;
            IEnumerable<KeyValuePair<string, Task<Individual>>> toNextGeneration;
            int totalGenerated = 0;
            int mutatedCount = 0;
            bool needsMutation = false;
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < IterationsCount; i++)
            {
                Console.WriteLine($"Iteration: {i}");

                sw.Start();
                orderedPopulation = population.OrderByDescending(i => i.Value.Result.Fitness);
                sw.Stop();
                Console.WriteLine($"Ordering time: {sw.ElapsedMilliseconds} ms");
                sw.Reset();

                best = orderedPopulation.Take(BestCount);
                toNextGeneration = orderedPopulation.Skip(BestCount).Take(ToNextGenerationCount);

                population = new Dictionary<string, Task<Individual>>(best);
                totalGenerated = population.Count;
                mutatedCount = 0;
                needsMutation = false;

                sw.Start();
                while (totalGenerated < PopulationSize)
                {
                    needsMutation = mutatedCount * 100 / (float)(totalGenerated - population.Count) < MutationPercentage;
                    if (needsMutation)
                        mutatedCount += 2;
                    Crossover(best.ElementAt(random.Next(0, BestCount - 1)).Value.Result,
                        toNextGeneration.ElementAt(random.Next(0, ToNextGenerationCount - 1)).Value.Result,
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
            while(population.Count != size)
            {
                individual = new Individual(new string(Alphabet.ToCharArray().OrderBy(s => (random.Next(2) % 2) == 0).ToArray()), Alphabet.Length);
                population.Add(individual);
            }

            return population;
        }

        private Task<Individual> CalcFitness(Individual individual)
        {
            var substitution = new Substitution(Alphabet, individual.Key);
            string decryptedMessage = substitution.Decrypt(EncryptedText);

            return CalcThreeLettersFittingQuotient(decryptedMessage)
                .ContinueWith(fitness => 
                {
                    individual.Fitness = fitness.Result;
                    return individual;
                });
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

        //private Task<float> CalcTwoLettersFittingQuotient(string decryptedMessage)
        //{
        //    List<string> bigrams = decryptedMessage.Select(
        //        (letter, index) => index != decryptedMessage.Length - 1 ? $"{letter}{decryptedMessage[index + 1]}" : letter.ToString())
        //        .Take(decryptedMessage.Length - 1)
        //        .ToList();

        //    return CalcFittingQuotient(bigrams, _twoLettersFrequencies);
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private Task<float> CalcThreeLettersFittingQuotient(string decryptedMessage)
        //{
        //    List<string> trigrams = decryptedMessage.Select((letter, index) => 
        //        index != decryptedMessage.Length - 2 ? $"{letter}{decryptedMessage[index + 1]}{decryptedMessage[index + 2]}" : letter.ToString())
        //        .Take(decryptedMessage.Length - 2)
        //        .ToList();

        //    return CalcFittingQuotient(trigrams, _threeLettersFrequencies);
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private Task<float> CalcFittingQuotient(List<string> splittedMessage, Dictionary<string, float> etalon)
        //{
        //    float currentFrequency;
        //    float tempDeviationSum = 0;
        //    foreach (var frequency in etalon.Select(e => (e.Key, e.Value)).ToList())
        //    {
        //        currentFrequency =
        //            splittedMessage.Count(val => val.Equals(frequency.Key, StringComparison.Ordinal)) * 100 /
        //            (float)splittedMessage.Count;

        //        tempDeviationSum += Math.Abs(frequency.Value - currentFrequency);
        //    }

        //    return Task.FromResult(tempDeviationSum / etalon.Count);
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task<float> CalcThreeLettersFittingQuotient(string decryptedMessage)
        {
            int counter = 0, index = -3;
            float currentFrequency;
            float tempDeviationSum = 0;
            foreach (var frequency in _threeLettersFrequencies)
            {
                while(index >= 0 && index < decryptedMessage.Length - 2)
                {
                    index = decryptedMessage.IndexOf(frequency.TriGram, index + 1, StringComparison.Ordinal);
                    if (index >= 0)
                        counter++;
                }               

                currentFrequency = counter * 100 / decryptedMessage.Length - 2;
                tempDeviationSum += Math.Abs(frequency.Frequency - currentFrequency);
            }

            return Task.FromResult(tempDeviationSum / _threeLettersFrequencies.Count);
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
