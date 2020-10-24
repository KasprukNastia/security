using Lab1.Task1;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Lab1.Task3
{
    public class SubstitutionAttacker
    {
        private readonly Dictionary<string, float> _twoLettersFrequencies;
        private readonly Dictionary<string, float> _threeLettersFrequencies;
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
            Dictionary<string, float> threeLettersFrequencies,
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

        public List<Individual> Evaluate()
        {
            List<Individual> population = GeneratePoputation(PopulationSize);

            var random = new Random();

            IOrderedEnumerable<Individual> orderedPopulation;
            List<Individual> best;
            List<Individual> other;
            int randomPos;
            int totalGenerated = 0;
            int mutatedCount = 0;
            bool needsMutation = false;
            List<Individual> toNextGeneration;
            for (int i = 0; i < IterationsCount; i++)
            {
                orderedPopulation = population.OrderByDescending(i => i.Fitness);
                best = orderedPopulation.Take(BestCount).ToList();
                toNextGeneration = orderedPopulation.Skip(BestCount).Take(ToNextGenerationCount).ToList();

                population = new List<Individual>(best);
                totalGenerated = 0;
                mutatedCount = 0;
                needsMutation = false;
                while (population.Count < PopulationSize)
                {
                    needsMutation = mutatedCount * 100 / (float)totalGenerated < MutationPercentage;
                    if (needsMutation)
                        mutatedCount += 2;
                    Crossover(best[random.Next(0, best.Count - 1)], toNextGeneration[random.Next(0, toNextGeneration.Count - 1)], needsMutation)
                        .ForEach(i => population.Add(i));
                    totalGenerated++;
                }

                population = population.Distinct(new IndividualsComparer()).ToList();
            }

            return population;
        }

        private List<Individual> GeneratePoputation(int size)
        {
            var random = new Random();
            HashSet<Individual> population = new HashSet<Individual>(size, new IndividualsComparer());
            Individual individual;
            while(population.Count != size)
            {
                individual = new Individual(new string(Alphabet.ToCharArray().OrderBy(s => (random.Next(2) % 2) == 0).ToArray()), Alphabet.Length);
                population.Add(CalcFitness(individual));
            }

            return population.ToList();
        }

        private Individual CalcFitness(Individual individual)
        {
            var substitution = new Substitution(Alphabet, individual.Key);
            string decryptedMessage = substitution.Decrypt(EncryptedText);

            individual.Fitness = //OneLetterFittingQuotientCoef * _singleByteXorAttacker.CalcOneLetterFittingQuotient(decryptedMessage) +
                    //TwoLettersFittingQuotientCoef * CalcTwoLettersFittingQuotient(decryptedMessage) +
                    ThreeLettersFittingQuotientCoef * CalcThreeLettersFittingQuotient(decryptedMessage);

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

            return new List<Individual>
            {
                CalcFitness(new Individual(new string(firstChildKey), Alphabet.Length)),
                CalcFitness(new Individual(new string(secondChildKey), Alphabet.Length))
            };

            void Swap(char[] childKey, int firstLetterIndex, int secondLetterIndex)
            {
                char temp = childKey[firstLetterIndex];
                childKey[firstLetterIndex] = childKey[secondLetterIndex];
                childKey[secondLetterIndex] = temp;
            }
        }

        private float CalcTwoLettersFittingQuotient(string decryptedMessage)
        {
            List<string> bigrams = decryptedMessage.Select(
                (letter, index) => index != decryptedMessage.Length - 1 ? $"{letter}{decryptedMessage[index + 1]}" : letter.ToString())
                .Take(decryptedMessage.Length - 1)
                .ToList();

            return CalcFittingQuotient(bigrams, _twoLettersFrequencies);
        }

        private float CalcThreeLettersFittingQuotient(string decryptedMessage)
        {
            List<string> trigrams = decryptedMessage.Select((letter, index) => 
                index != decryptedMessage.Length - 2 ? $"{letter}{decryptedMessage[index + 1]}{decryptedMessage[index + 2]}" : letter.ToString())
                .Take(decryptedMessage.Length - 2)
                .ToList();

            return CalcFittingQuotient(trigrams, _threeLettersFrequencies);
        }

        private float CalcFittingQuotient(List<string> splittedMessage, Dictionary<string, float> etalon)
        {
            float currentFrequency;
            float tempDeviationSum = 0;
            foreach (var frequency in etalon)
            {
                currentFrequency =
                    splittedMessage.Count(val => val.Equals(frequency.Key)) * 100 /
                    (float)splittedMessage.Count;

                tempDeviationSum += Math.Abs(frequency.Value - currentFrequency);
            }

            return tempDeviationSum / etalon.Count;
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
