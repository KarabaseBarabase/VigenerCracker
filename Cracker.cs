using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Crypto
{
    public class VigenereCracker
    {
        public static void Main()
        {
            Console.WriteLine("Введите текст: ");
            string encryptedText = Console.ReadLine();
            string key;
            string cleanedText = CleanText(encryptedText);
            Console.WriteLine("Очищенный текст:");
            Console.WriteLine(cleanedText);
            Console.WriteLine();

            List<int> possibleKeyLengths = FindKeyLengths(cleanedText);
            foreach (int keyLength in possibleKeyLengths)
            {
                Console.WriteLine($"\nПробуем длину ключа: {keyLength}");
                key = FindKey(cleanedText, keyLength);
                Console.WriteLine($"Найденный ключ: {key}");
            }
            Console.WriteLine("Введите отредактированный ключ: ");
            key = Console.ReadLine();
            string decryptedText = Decrypt(cleanedText, key);
            Console.WriteLine("\nРасшифрованный текст:");
            Console.WriteLine(decryptedText);
        }

        public static string CleanText(string text)
        {
            Regex regex = new Regex("[^а-яёА-ЯЁ]");
            string cleaned = regex.Replace(text, "").ToLower();
            return cleaned;
        }

        public static List<int> FindKeyLengths(string text)
        {
            Dictionary<string, List<int>> sequences = new Dictionary<string, List<int>>();
            int minSequenceLength = 3;
            int maxSequenceLength = 40;

            for (int len = minSequenceLength; len <= maxSequenceLength; len++)
                for (int i = 0; i <= text.Length - len; i++)
                {
                    string sequence = text.Substring(i, len);
                    if (!sequences.ContainsKey(sequence))
                        sequences[sequence] = new List<int>();
                    sequences[sequence].Add(i);
                }
            var repeatedSequences = sequences.Where(kv => kv.Value.Count > 1).ToDictionary(kv => kv.Key, kv => kv.Value);

            List<int> distances = new List<int>();
            foreach (var seq in repeatedSequences)
                for (int i = 1; i < seq.Value.Count; i++)
                    distances.Add(seq.Value[i] - seq.Value[i - 1]);

            Dictionary<int, int> divisorCounts = new Dictionary<int, int>();
            foreach (int distance in distances)
                for (int d = 2; d <= 40; d++)
                    if (distance % d == 0)
                    {
                        if (!divisorCounts.ContainsKey(d))
                            divisorCounts[d] = 0;
                        divisorCounts[d]++;
                    }

            if (divisorCounts.Count == 0)
                return new List<int> { 3, 4, 5, 6, 7, 8, 9, 10 }; 
            return divisorCounts.Keys.ToList();
        }

        public static string FindKey(string text, int keyLength)
        {
            StringBuilder keyBuilder = new StringBuilder();
            for (int i = 0; i < keyLength; i++)
            {
                StringBuilder groupBuilder = new StringBuilder();
                for (int j = i; j < text.Length; j += keyLength) 
                    groupBuilder.Append(text[j]);
                string group = groupBuilder.ToString();
                char likelyKeyChar = FindLikelyKeyChar(group);
                keyBuilder.Append(likelyKeyChar);
            }
            return keyBuilder.ToString();
        }

        public static char FindLikelyKeyChar(string group)
        {
            Dictionary<char, double> frequencies = new Dictionary<char, double>()
            {
            {'о', 0.1097}, {'е', 0.0845}, {'а', 0.0801}, {'и', 0.0735}, {'н', 0.0670},
            {'т', 0.0626}, {'с', 0.0547}, {'р', 0.0473}, {'в', 0.0454}, {'л', 0.0440},
            {'к', 0.0349}, {'м', 0.0321}, {'д', 0.0298}, {'п', 0.0281}, {'у', 0.0262},
            {'я', 0.0201}, {'ы', 0.0190}, {'ь', 0.0174}, {'г', 0.0170}, {'з', 0.0165},
            {'б', 0.0159}, {'ч', 0.0144}, {'й', 0.0121}, {'х', 0.0097}, {'ж', 0.0094},
            {'ш', 0.0073}, {'ю', 0.0064}, {'ц', 0.0048}, {'щ', 0.0036}, {'э', 0.0032},
            {'ф', 0.0026}, {'ъ', 0.0004}, {'ё', 0.0004}
            };
            double maxScore = double.MinValue;
            char bestKeyChar = 'а';
            for (char keyChar = 'а'; keyChar <= 'я'; keyChar++)
            {
                double score = 0;
                foreach (char c in group)
                {
                    char decrypted = DecryptChar(c, keyChar);
                    if (frequencies.ContainsKey(decrypted))
                        score += frequencies[decrypted];
                }
                if (score > maxScore)
                {
                    maxScore = score;
                    bestKeyChar = keyChar;
                }
            }
            return bestKeyChar;
        }

        public static char DecryptChar(char encrypted, char keyChar)
        {
            int alphabetSize = 32;
            int encryptedPos = encrypted - 'а';
            int keyPos = keyChar - 'а';
            int decryptedPos = (encryptedPos - keyPos + alphabetSize) % alphabetSize;
            return (char)('а' + decryptedPos);
        }

        public static string Decrypt(string encryptedText, string key)
        {
            StringBuilder result = new StringBuilder();
            int keyIndex = 0;
            char[] alphabet = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя".ToCharArray();
            foreach (char c in encryptedText)
            {
                int textCharPos = Array.IndexOf(alphabet, c);
                int keyCharPos = Array.IndexOf(alphabet, key[keyIndex % key.Length]);
                int decryptedPos = (textCharPos - keyCharPos + alphabet.Length) % alphabet.Length;
                char decryptedChar = alphabet[decryptedPos];
                result.Append(decryptedChar);
                keyIndex++;
            }
            return result.ToString();
        }
    }
}