using System.Text.RegularExpressions;

namespace SiriBhuvalyaExtractor.Databases;

public class SanskritDictionaryExtractor
{
    public static List<string> Extract(string fileName)
    {

        List<string> words = new();
        try
        {
            // Read the entire file
            string content = File.ReadAllText(fileName);

            // Extract entries between <LEND> tags
            string[] entries = content.Split(new[] { "<LEND>" }, StringSplitOptions.RemoveEmptyEntries);

            // Create a list to store extracted words
            List<string> extractedWords = new List<string>();

            // Regular expressions to match k1 and k2 values
            Regex k1Regex = new Regex(@"<k1>(.*?)<k2>", RegexOptions.Singleline);
            Regex k2Regex = new Regex(@"<k2>(.*?)<e>", RegexOptions.Singleline);

            foreach (string entry in entries)
            {
                // Extract k1 value
                Match k1Match = k1Regex.Match(entry);
                if (k1Match.Success)
                {
                    string k1Value = k1Match.Groups[1].Value.Trim();
                    if (!string.IsNullOrEmpty(k1Value))
                    {
                        extractedWords.Add($"{k1Value}");
                    }
                }

                // // Extract k2 value
                // Match k2Match = k2Regex.Match(entry);
                // if (k2Match.Success)
                // {
                //     string k2Value = k2Match.Groups[1].Value.Trim();
                //     if (!string.IsNullOrEmpty(k2Value))
                //     {
                //         extractedWords.Add($"{k2Value}");
                //     }
                // }
            }

            words= extractedWords.Distinct().ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        return words;
    }
}