namespace SiriBhuvalyaExtractor.Extractor;

using System.Text;
using SiriBhuvalyaExtractor.Databases;
using SiriBhuvalyaExtractor.WordMatcher;

public class HelagandaWordLookup
{
    private readonly Dictionary<string, string> _vowelToMatraMap;
    private HelagandaWordTree _wordTree;

    public HelagandaWordLookup(HelagandaWordTree tree)
    {
        _wordTree = tree;
        _vowelToMatraMap = InitializeVowelToMatraMap();
    }

    public List<string> ExtractWords(List<string> helagandaLetters)
    {
        List<string> extractedWords = new List<string>();
        int startIndex = 0;

        while (startIndex < helagandaLetters.Count)
        {
            string longestWord = FindLongestWordStartingAt(helagandaLetters, startIndex);
            
            if (!string.IsNullOrEmpty(longestWord))
            {
                extractedWords.Add(longestWord);

                // Calculate how many letters were consumed to form this word
                int lettersConsumed = CountLettersInWord(helagandaLetters, startIndex, longestWord);
                startIndex += lettersConsumed;
            }
            else
            {
                // No word found starting at this position, skip one character
                extractedWords.Add(helagandaLetters[startIndex]);
                startIndex++;
            }
        }

        return extractedWords;
    }

    private string FindLongestWordStartingAt(List<string> letters, int startIndex)
    {
        string longestWord = string.Empty;
        List<string> currentSequence = new List<string>();

        for (int endIndex = startIndex; endIndex < letters.Count; endIndex++)
        {
            // Add the current letter to our sequence
            currentSequence.Add(letters[endIndex]);

            var contains = ContainsLetterSequence(currentSequence.ToArray());
            // Check if this letter sequence exists in dictionary
            if (contains &&
                (currentSequence.Count > longestWord.Length || string.IsNullOrEmpty(longestWord)))
            {
                // Store the longest matching sequence
                StringBuilder sb = new StringBuilder();

                string? previousLetter = null;
                for (int i = 0; i < currentSequence.Count; i++)
                {
                    var letter = currentSequence[i];
                    ProcessLetter(sb, letter, previousLetter ?? "");
                    previousLetter = letter;
                }

                longestWord = sb.ToString();
            }
            else if (!contains)
            {
                break;
            }
        }

        return longestWord;
    }

    private bool ContainsLetterSequence(string[] toArray)
    {
        return _wordTree.ContainsLetterSequence(toArray);
    }

    private void ProcessLetter(StringBuilder builder, string letter, string previousLetter = "")
    {
        // Handle special cases for Kannada half consonants and matras
        
        // If the letter is a matra, it may need special handling
        if (!string.IsNullOrEmpty(previousLetter) && (IsHalfConsonant(previousLetter)) &&
            _vowelToMatraMap.TryGetValue(letter, out string matra))
        {
            // Apply the matra to the previous consonant
            // Special handling for Kannada i-matra (ಿ) which comes before the consonant
            if (matra == "ಿ")
            {
                if (builder.Length > 0)
                    builder.Remove(builder.Length - 1, 1).Insert(builder.Length, matra);
                else
                {
                    builder.Append(letter);
                }
            }
            else
            {
                if (builder.Length > 0)
                    builder.Remove(builder.Length - 1, 1).Append(matra);
                else
                {
                    builder.Append(letter);
                }
            }
        }
        // Handle half consonants (consonants with virama)
        else if (IsHalfConsonant(letter))
        {
            builder.Append(letter);
        }
        else
        {
            // Regular letter
            builder.Append(letter);
        }
    }

    private bool IsHalfConsonant(string letter)
    {
        // In Kannada, half consonant (or consonant with virama) ends with ್
        return letter.EndsWith("್");
    }

    private int CountLettersInWord(List<string> letters, int startIndex, string word)
    {
        // Count how many letters from the input list were consumed to form this word
        int letterCount = 0;
        StringBuilder builder = new StringBuilder();

        for (int i = startIndex; i < letters.Count; i++)
        {
            ProcessLetter(builder, letters[i], i > 0 ? letters[i - 1] : "");
            letterCount++;

            if (builder.ToString() == word)
                break;
        }

        return letterCount;
    }
    private Dictionary<string, string> InitializeVowelToMatraMap()
    {
        // Initialize Helaganda vowels to corresponding matras mapping based on Kannada script
        return new Dictionary<string, string>
        {
            // Independent vowels -> Dependent vowel marks (matras)
            { "ಅ", "" },      // a -> no visible matra
            { "ಆ", "ಾ" },     // ā -> ā-matra
            { "ಆಾ", "ಾ" },    // duplicate ā
            { "ಇ", "ಿ" },     // i -> i-matra
            { "ಈ", "ೀ" },     // ī -> ī-matra
            { "ಈೀ", "ೀ" },    // duplicate ī
            { "ಉ", "ು" },     // u -> u-matra
            { "ಊ", "ೂ" },     // ū -> ū-matra
            { "ಊೂ", "ೂ" },    // duplicate ū
            { "ಋ", "ೃ" },     // ṛ -> ṛ-matra
            { "ೠ", "ೄ" },     // ṝ -> ṝ-matra
            { "ೠೄ", "ೄ" },    // duplicate ṝ
            { "ಳ್", "" },      // ḷ (half consonant)
            { "ಳು", "ು" },     // ḷu -> dependent vowel mark
            { "ಳೂ", "ೂ" },     // ḷū -> dependent vowel mark
            { "ಎ", "ೆ" },     // e -> e-matra
            { "ಏ", "ೇ" },     // ē -> ē-matra
            { "ಏೋ", "ೇ" },    // extended ē
            { "ಐ", "ೈ" },     // ai -> ai-matra
            { "ಐೖ", "ೈ" },    // extended ai
            { "ಐೖೖ", "ೈ" },   // further extended ai
            { "ಒ", "ೊ" },     // o -> o-matra
            { "ಓ", "ೋ" },     // ō -> ō-matra
            { "ಓೋ", "ೋ" },    // extended ō
            { "ಔ", "ೌ" },     // au -> au-matra
            { "ಔೌ", "ೌ" },    // extended au
            { "ಔೌೌ", "ೌ" },   // further extended au

            // Special cases
            { "ಂ", "ಂ" },      // anusvāra
            { "ಃ", "ಃ" },      // visarga
            { "...", "..." },  // horizontal ellipsis
            { "::", "::" },    // double colon
            { ":", ":" }       // colon
        };
    }

    // Helper methods
    public bool IsVowel(string letter)
    {
        return _vowelToMatraMap.ContainsKey(letter);
    }

    public bool IsMatra(string letter)
    {
        return _vowelToMatraMap.ContainsValue(letter);
    }

    public string GetMatraForVowel(string vowel)
    {
        if (_vowelToMatraMap.TryGetValue(vowel, out string matra))
        {
            return matra;
        }

        return string.Empty;
    }
}