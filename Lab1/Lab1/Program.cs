using Lab1.Task1;
using Lab1.Task2;
using Lab1.Task3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lab1
{
    class Program
    {
        static void Main(string[] args)
        {
            RunFourthTask();
        }

        public static void RunFirstTask()
        {
            string firstTaskEncryptedMessage = "Yx`7cen7v7ergrvc~yp:|rn7OXE7t~g.re97R9p97~c7d.xb{s7cv|r7v7dce~yp75.r{{x7`xe{s57vys;7p~ary7c.r7|rn7~d75|rn5;7oxe7c.r7q~edc7{rccre75.57`~c.75|5;7c.ry7oxe75r57`~c.75r5;7c.ry75{57`~c.75n5;7vys7c.ry7oxe7yroc7t.ve75{57`~c.75|57vpv~y;7c.ry75x57`~c.75r57vys7dx7xy97Nxb7zvn7bdr7vy7~ysro7xq7tx~yt~srytr;7_vzz~yp7s~dcvytr;7\vd~d|~7rovz~yvc~xy;7dcvc~dc~tv{7crdcd7xe7`.vcrare7zrc.xs7nxb7qrr{7`xb{s7d.x`7c.r7urdc7erdb{c9";
            foreach (byte firstTaskKey in new SingleByteXorAttacker().GetSingleByteXorPossibleKeys(firstTaskEncryptedMessage))
            {
                Console.WriteLine($"First task key: {firstTaskKey}");
                Console.WriteLine("First task decrypted message");
                Console.WriteLine(new SingleByteXor().Decrypt(firstTaskEncryptedMessage, firstTaskKey));
                Console.WriteLine();
            }
        }

        public static void RunSecondTask()
        {
            string secondTaskEncryptedMessage = "1c41023f564b2a130824570e6b47046b521f3f5208201318245e0e6b40022643072e13183e51183f5a1f3e4702245d4b285a1b23561965133f2413192e571e28564b3f5b0e6b50042643072e4b023f4a4b24554b3f5b0238130425564b3c564b3c5a0727131e38564b245d0732131e3b430e39500a38564b27561f3f5619381f4b385c4b3f5b0e6b580e32401b2a500e6b5a186b5c05274a4b79054a6b67046b540e3f131f235a186b5c052e13192254033f130a3e470426521f22500a275f126b4a043e131c225f076b431924510a295f126b5d0e2e574b3f5c4b3e400e6b400426564b385c193f13042d130c2e5d0e3f5a086b52072c5c192247032613433c5b02285b4b3c5c1920560f6b47032e13092e401f6b5f0a38474b32560a391a476b40022646072a470e2f130a255d0e2a5f0225544b24414b2c410a2f5a0e25474b2f56182856053f1d4b185619225c1e385f1267131c395a1f2e13023f13192254033f13052444476b4a043e131c225f076b5d0e2e574b22474b3f5c4b2f56082243032e414b3f5b0e6b5d0e33474b245d0e6b52186b440e275f456b710e2a414b225d4b265a052f1f4b3f5b0e395689cbaa186b5d046b401b2a500e381d4b23471f3b4051641c0f2450186554042454072e1d08245e442f5c083e5e0e2547442f1c5a0a64123c503e027e040c413428592406521a21420e184a2a32492072000228622e7f64467d512f0e7f0d1a";
            foreach (string secondTaskKey in new RepeatingKeyXorAttacker().GetRepeatingKeyXorPossibleKeys(secondTaskEncryptedMessage.HexStringToString()))
            {
                Console.WriteLine($"Second task key: {secondTaskKey}");
                Console.WriteLine("Second task decrypted message");
                Console.WriteLine(new RepeatingKeyXor().Decrypt(secondTaskEncryptedMessage.HexStringToString(), secondTaskKey));
                Console.WriteLine();
            }
        }

        public static void RunThirdTask()
        {
            var basePath = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent}\\Task3\\Data";

            string allText = File.ReadAllText($"{basePath}\\bigrams_percentages.json");
            var bigrams = JsonConvert.DeserializeObject<Dictionary<string, float>>(allText)
                .Select(t => new EtalonMember(t.Key, t.Value)).ToList();

            allText = File.ReadAllText($"{basePath}\\trigrams_percentages.json");
            var trigrams = JsonConvert.DeserializeObject<Dictionary<string, float>>(allText)
                .Select(t => new EtalonMember(t.Key, t.Value)).ToList();

            string thirdTaskEncryptedMessage = "EFFPQLEKVTVPCPYFLMVHQLUEWCNVWFYGHYTCETHQEKLPVMSAKSPVPAPVYWMVHQLUSPQLYWLASLFVWPQLMVHQLUPLRPSQLULQESPBLWPCSVRVWFLHLWFLWPUEWFYOTCMQYSLWOYWYETHQEKLPVMSAKSPVPAPVYWHEPPLUWSGYULEMQTLPPLUGUYOLWDTVSQETHQEKLPVPVSMTLEUPQEPCYAMEWWYTYWDLUULTCYWPQLSEOLSVOHTLUYAPVWLYGDALSSVWDPQLNLCKCLRQEASPVILSLEUMQBQVMQCYAHUYKEKTCASLFPYFLMVHQLUPQLHULIVYASHEUEDUEHQBVTTPQLVWFLRYGMYVWMVFLWMLSPVTTBYUNESESADDLSPVYWCYAMEWPUCPYFVIVFLPQLOLSSEDLVWHEUPSKCPQLWAOKLUYGMQEUEMPLUSVWENLCEWFEHHTCGULXALWMCEWETCSVSPYLEMQYGPQLOMEWCYAGVWFEBECPYASLQVDQLUYUFLUGULXALWMCSPEPVSPVMSBVPQPQVSPCHLYGMVHQLUPQLWLRPOEDVMETBYUFBVTTPENLPYPQLWLRPTEKLWZYCKVPTCSTESQPQULLGYAUMEHVPETFWMEHVPETBZMEHVPETB";            
            
            string alphabet = new string(SingleByteXorAttacker.OneLetterEnglishFrequency.Select(c => c.Key).ToArray());
            string thirdTaskKey = "EKMFLGDQVZNTOWYHXUSPAIBRCJ".ToLower();
            var substitution = new Substitution(alphabet, new List<string> { thirdTaskKey });            
            Console.WriteLine(substitution.Decrypt(thirdTaskEncryptedMessage.ToLower()));

            //var substitutionAttacker = new SubstitutionAttacker(
            //    encryptedText: thirdTaskEncryptedMessage,
            //    individualSetMembersCount: 1,
            //    minPopulationSize: 40,
            //    maxPopulationSize: 100,
            //    iterationsCount: 1000,
            //    mutationPercentage: 0.02F,
            //    bestPercentage: 30,
            //    twoLettersFrequencies: bigrams,
            //    threeLettersFrequencies: trigrams,
            //    twoLettersFittingQuotientCoef: 0.5F,
            //    threeLettersFittingQuotientCoef: 1.5F);

            //List<IndividualSet> keySets = substitutionAttacker.Evaluate().Result;
            //for(int i = 0; i < 10; i++)
            //{
            //    Console.WriteLine($"Key set {i}:");
            //    keySets[i].ForEach(Console.WriteLine);
            //    Console.WriteLine();
            //}
        }

        public static void RunFourthTask()
        {
            string fourthTaskEncryptedMessage = "KZBWPFHRAFHMFSNYSMNOZYBYLLLYJFBGZYYYZYEKCJVSACAEFLMAJZQAZYHIJFUNHLCGCINWFIHHHTLNVZLSHSVOZDPYSMNYJXHMNODNHPATXFWGHZPGHCVRWYSNFUSPPETRJSIIZSAAOYLNEENGHYAMAZBYSMNSJRNGZGSEZLNGHTSTJMNSJRESFRPGQPSYFGSWZMBGQFBCCEZTTPOYNIVUJRVSZSCYSEYJWYHUJRVSZSCRNECPFHHZJBUHDHSNNZQKADMGFBPGBZUNVFIGNWLGCWSATVSSWWPGZHNETEBEJFBCZDPYJWOSFDVWOTANCZIHCYIMJSIGFQLYNZZSETSYSEUMHRLAAGSEFUSKBZUEJQVTDZVCFHLAAJSFJSCNFSJKCFBCFSPITQHZJLBMHECNHFHGNZIEWBLGNFMHNMHMFSVPVHSGGMBGCWSEZSZGSEPFQEIMQEZZJIOGPIOMNSSOFWSKCRLAAGSKNEAHBBSKKEVTZSSOHEUTTQYMCPHZJFHGPZQOZHLCFSVYNFYYSEZGNTVRAJVTEMPADZDSVHVYJWHGQFWKTSNYHTSZFYHMAEJMNLNGFQNFZWSKCCJHPEHZZSZGDZDSVHVYJWHGQFWKTSNYHTSZFYHMAEDNJZQAZSCHPYSKXLHMQZNKOIOKHYMKKEIKCGSGYBPHPECKCJJKNISTJJZMHTVRHQSGQMBWHTSPTHSNFQZKPRLYSZDYPEMGZILSDIOGGMNYZVSNHTAYGFBZZYJKQELSJXHGCJLSDTLNEHLYZHVRCJHZTYWAFGSHBZDTNRSESZVNJIVWFIVYSEJHFSLSHTLNQEIKQEASQJVYSEVYSEUYSMBWNSVYXEIKWYSYSEYKPESKNCGRHGSEZLNGHTSIZHSZZHCUJWARNEHZZIWHZDZMADNGPNSYFZUWZSLXJFBCGEANWHSYSEGGNIVPFLUGCEUWTENKCJNVTDPNXEIKWYSYSFHESFPAJSWGTYVSJIOKHRSKPEZMADLSDIVKKWSFHZBGEEATJLBOTDPMCPHHVZNYVZBGZSCHCEZZTWOOJMBYJSCYFRLSZSCYSEVYSEUNHZVHRFBCCZZYSEUGZDCGZDGMHDYNAFNZHTUGJJOEZBLYZDHYSHSGJMWZHWAFTIAAY";

            var basePath = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent}\\Task3\\Data";
            string allText = File.ReadAllText($"{basePath}\\bigrams_percentages.json");
            var bigrams = JsonConvert.DeserializeObject<Dictionary<string, float>>(allText)
                .Select(t => new EtalonMember(t.Key, t.Value)).ToList();
            allText = File.ReadAllText($"{basePath}\\trigrams_percentages.json");
            var trigrams = JsonConvert.DeserializeObject<Dictionary<string, float>>(allText)
                .Select(t => new EtalonMember(t.Key, t.Value)).ToList();

            //int individualSetMembersCount = new RepeatingKeyXorAttacker().GetKeyLength(fourthTaskEncryptedMessage);
            int individualSetMembersCount = 4;

            var substitutionAttacker = new SubstitutionAttacker(
                encryptedText: fourthTaskEncryptedMessage,
                individualSetMembersCount: individualSetMembersCount,
                minPopulationSize: 40,
                maxPopulationSize: 100,
                iterationsCount: 1000,
                mutationPercentage: 0.02F,
                bestPercentage: 30,
                twoLettersFrequencies: bigrams,
                threeLettersFrequencies: trigrams,
                twoLettersFittingQuotientCoef: 0.5F,
                threeLettersFittingQuotientCoef: 1.5F);

            List<IndividualSet> keySets = substitutionAttacker.Evaluate().Result;
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Key set {i}:");
                keySets[i].ForEach(Console.WriteLine);
                Console.WriteLine();
            }
        }
    }
}
