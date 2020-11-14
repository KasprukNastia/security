using Lab1.Task1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public int IndividualSetMembersCount { get; }
        public int MinPopulationSize { get; }
        public int MaxPopulationSize { get; }
        public int IterationsCount { get; }
        public double MutationPercentage { get; }
        public int BestPercentage { get; }
        public double TwoLettersFittingQuotientCoef { get; }
        public double ThreeLettersFittingQuotientCoef { get; }

        public SubstitutionAttacker(
            string encryptedText,
            int individualSetMembersCount,
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

            if(individualSetMembersCount <= 0)
                throw new ArgumentException($"{nameof(individualSetMembersCount)} must be  greather than 0");
            IndividualSetMembersCount = individualSetMembersCount;

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

        public async Task<List<IndividualSet>> Evaluate()
        {
            List<Task<IndividualSet>> population =
                GeneratePoputation(MaxPopulationSize).Select(i => CalcFitness(i)).ToList();
            await Task.WhenAll(population);

            var random = new Random();
            IOrderedEnumerable<Task<IndividualSet>> orderedPopulation;
            IEnumerable<Task<IndividualSet>> best;
            int currentBestCount;
            int totalGenerated = 0;
            int mutatedCount = 0;
            bool needsMutation = false;
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < IterationsCount; i++)
            {
                Console.WriteLine($"Iteration: {i}");
                orderedPopulation = population.OrderByDescending(i => i.Result.Fitness);
                Console.WriteLine("Best keys:");
                foreach(Individual individual in orderedPopulation.First().Result)
                    Console.WriteLine(individual.Key);

                currentBestCount = orderedPopulation.Count() * BestPercentage / 100;                
                best = orderedPopulation.Take(currentBestCount);

                population = new List<Task<IndividualSet>>(best);
                totalGenerated = population.Count;
                mutatedCount = 0;
                needsMutation = false;

                sw.Start();
                while (totalGenerated < MaxPopulationSize || population.Count < MinPopulationSize)
                {
                    needsMutation = mutatedCount * 100 / (double)(totalGenerated - population.Count) < MutationPercentage;
                    if (needsMutation)
                        mutatedCount += 2;
                    Crossover(best.ElementAt(random.Next(0, currentBestCount)).Result,
                        best.ElementAt(random.Next(0, currentBestCount)).Result,
                        needsMutation)
                        .ForEach(i =>
                        {
                            population.Add(CalcFitness(i));
                        });
                    totalGenerated += 2;
                }
                await Task.WhenAll(population);
                sw.Stop();
                Console.WriteLine($"Population generation time: {sw.ElapsedMilliseconds} ms");

                population = population.Distinct(new IndividualSetTasksComparer()).ToList();
                
                sw.Reset();
                Console.WriteLine();
            }

            return population.Select(val => val.Result).ToList();
        }

        private IEnumerable<IndividualSet> GeneratePoputation(int size)
        {
            var random = new Random();
            var population = new HashSet<IndividualSet>(size, new IndividualSetsComparer());
            IndividualSet individualSet;
            Individual individual;
            while (population.Count != size)
            {
                individualSet = new IndividualSet(IndividualSetMembersCount);
                while(individualSet.Count != IndividualSetMembersCount)
                {
                    individual = new Individual(new string(Alphabet.ToCharArray().OrderBy(s => (random.Next(2) % 2) == 0).ToArray()), Alphabet.Length);
                    individualSet.Add(individual);
                }
                population.Add(individualSet);
            }

            return population;
        }

        private async Task<IndividualSet> CalcFitness(IndividualSet individualSet)
        {
            var substitution = new Substitution(Alphabet, individualSet.Select(i => i.Key).ToList());
            string decryptedMessage = substitution.Decrypt(EncryptedText);

            IEnumerable<double> results = await Task.WhenAll(
                CalcFittingQuotient(decryptedMessage, _twoLettersFrequencies),
                CalcFittingQuotient(decryptedMessage, _threeLettersFrequencies));

            individualSet.Fitness = TwoLettersFittingQuotientCoef * results.ElementAt(0) +
                ThreeLettersFittingQuotientCoef * results.ElementAt(1);

            return individualSet;
        }

        private List<IndividualSet> Crossover(IndividualSet firstSet, IndividualSet secondSet, bool needsMutation = false)
        {
            IndividualSet firstChildSet = new IndividualSet(IndividualSetMembersCount);
            IndividualSet secondChildSet = new IndividualSet(IndividualSetMembersCount);

            var random = new Random();
            char[] firstChildKey, secondChildKey;
            LinkedList<char> firstKeyCopy, secondKeyCopy;
            List<(int index, int indicator)> positions;
            for (int keyIndex = 0; keyIndex < firstSet.Count; keyIndex++)
            {
                firstChildKey = new char[Alphabet.Length];
                secondChildKey = new char[Alphabet.Length];

                firstKeyCopy = new LinkedList<char>(firstSet[keyIndex].Key);
                secondKeyCopy = new LinkedList<char>(secondSet[keyIndex].Key);               

                positions = Enumerable.Range(0, Alphabet.Length)
                    .Select(i => (i, random.Next(0, 2))).ToList();
                foreach ((int letterIndex, int indicator) in positions)
                {
                    if (indicator == 0)
                    {
                        firstChildKey[letterIndex] = firstSet[keyIndex].Key[letterIndex];
                        secondKeyCopy.Remove(firstSet[keyIndex].Key[letterIndex]);
                    }
                    else
                    {
                        secondChildKey[letterIndex] = secondSet[keyIndex].Key[letterIndex];
                        firstKeyCopy.Remove(secondSet[keyIndex].Key[letterIndex]);
                    }
                }

                foreach ((int index, int indicator) in positions)
                {
                    if (indicator == 1)
                    {
                        firstChildKey[index] = secondKeyCopy.First.Value;
                        secondKeyCopy.RemoveFirst();
                    }
                    else
                    {
                        secondChildKey[index] = firstKeyCopy.First.Value;
                        firstKeyCopy.RemoveFirst();
                    }
                }

                if (needsMutation)
                {
                    int randomPos = random.Next(0, Alphabet.Length);
                    int firstLetterIndex = random.Next(0, randomPos);
                    int secondLetterIndex = random.Next(randomPos, Alphabet.Length);

                    Swap(firstChildKey, firstLetterIndex, secondLetterIndex);
                    Swap(secondChildKey, firstLetterIndex, secondLetterIndex);
                }

                firstChildSet.Add(new Individual(new string(firstChildKey), Alphabet.Length));
                secondChildSet.Add(new Individual(new string(secondChildKey), Alphabet.Length));
            }

            return new List<IndividualSet> { firstChildSet, secondChildSet };

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

                currentFrequency = numberOfMatches * 100 / (double)(decryptedMessage.Length - etalonMemberKeyLength - 1);
                tempDeviationSum += Math.Abs(frequency.Frequency - currentFrequency);
            }

            return Task.FromResult(tempDeviationSum / etalons.Count);
        }
    }
}