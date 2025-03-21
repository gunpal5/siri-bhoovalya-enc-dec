using SiriBhuvalyaExtractor.Extractor;

namespace SiriBhuvalyaExtractor.Databases;

public class HelagandaWordTree
{
    private class TrieNode
    {
        public Dictionary<string, TrieNode> Children { get; } = new Dictionary<string, TrieNode>();
        public bool IsEndOfWord { get; set; }
    }

    private readonly TrieNode _root = new TrieNode();

    // Complete set of Kannada characters for Helaganda as provided
    public static readonly string[] HelagandaChars =
    {
        "ಅ", "ಆ", "ಆಾ", "ಇ", "ಈ", "ಈೀ", "ಉ", "ಊ", "ಊೂ", "ಋ", "ೠ", "ೠೄ",
        "ಳ್", "ಳು", "ಳೂ", "ಎ", "ಏ", "ಏೋ", "ಐ", "ಐೖ", "ಐೖೖ", "ಒ", "ಓ", "ಓೋ",
        "ಔ", "ಔೌ", "ಔೌೌ", "ಕ್", "ಖ್", "ಗ್", "ಘ್", "ಙ್", "ಚ್", "ಛ್", "ಜ್",
        "ಝ್", "ಞ್", "ಟ್", "ಠ್", "ಡ್", "ಢ್", "ಣ್", "ತ್", "ಥ್", "ದ್", "ಧ್", "ನ್",
        "ಪ್", "ಫ್", "ಬ್", "ಭ್", "ಮ್", "ಯ್", "ರ್", "ಲ್", "ವ್", "ಶ್", "ಷ್", "ಸ್",
        "ಹ್", "ಂ", "ಃ", "...", "::", ":", "ೞ್", "ಱ್"
    };

    // Set of vowels (first 27 entries in the array)
    private static readonly HashSet<string> Vowels = new HashSet<string>(HelagandaChars.Take(27));

    // Set of half consonants (entries 27-62 in the array)
    private static readonly HashSet<string> HalfConsonants = new HashSet<string>(HelagandaChars.Skip(27).Take(36));

    // Updated comprehensive mapping of matras to corresponding vowels for Kannada script
    private static readonly Dictionary<char, string> MatraToVowelMap = new Dictionary<char, string>
    {
        // Basic vowel marks
        { 'ಾ', "ಆ" }, // aa
        { 'ಿ', "ಇ" }, // i
        { 'ೀ', "ಈ" }, // ii
        { 'ು', "ಉ" }, // u
        { 'ೂ', "ಊ" }, // uu
        { 'ೃ', "ಋ" }, // ri
        { 'ೄ', "ೠ" }, // rri
        { 'ೆ', "ಎ" }, // e
        { 'ೇ', "ಏ" }, // ee
        { 'ೈ', "ಐ" }, // ai
        { 'ೊ', "ಒ" }, // o
        { 'ೋ', "ಓ" }, // oo
        { 'ೌ', "ಔ" }, // au

        // Special marks
        { 'ಂ', "ಂ" }, // anusvara
        { 'ಃ', "ಃ" }, // visarga
        { '್', "್" }, // virama/halant
    };

    // Special characters
    private const char VIRAMA = '\u0CCD'; // Kannada Halant

    /// <summary>
    /// Load Helaganda words from file and build the tree
    /// </summary>
    public void LoadDictionary(string filePath)
    {
        var words = HelagandaExtractor.ExtractHelagandaWords(filePath);
        foreach (var word in words)
        {
            AddWord(word);
        }

        Console.WriteLine($"Loaded {words.Count} Helaganda words into the tree");
    }

    /// <summary>
    /// Add a collection of Helaganda words to the tree
    /// </summary>
    public void AddWords(IEnumerable<string> words)
    {
        int count = 0;
        foreach (var word in words)
        {
            AddWord(word);
            count++;
        }

        Console.WriteLine($"Added {count} Helaganda words to the tree");
    }

    /// <summary>
    /// Add a single word to the tree
    /// </summary>
    public void AddWord(string word)
    {
        if (string.IsNullOrEmpty(word))
            return;

        var characters = SplitIntoHelagandaCharacters(word);
        var current = _root;

        foreach (var character in characters)
        {
            if (!current.Children.TryGetValue(character, out var node))
            {
                node = new TrieNode();
                current.Children[character] = node;
            }

            current = node;
        }

        current.IsEndOfWord = true;
    }

    /// <summary>
    /// Split a Helaganda word into individual characters based on Kannada script rules
    /// </summary>
    public List<string> SplitIntoHelagandaCharacters(string word)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(word))
            return result;

        int i = 0;
        while (i < word.Length)
        {
            // Handle consonants
            if (IsConsonant(word[i]))
            {
                string current = word[i].ToString();
                i++;

                // Check for virama or matra
                if (i < word.Length)
                {
                    if (word[i] == VIRAMA)
                    {
                        // Half consonant with explicit virama
                        current += VIRAMA;
                        i++;
                        result.Add(current);
                        continue;
                    }
                    else if (IsVowelMark(word[i]))
                    {
                        // Add the consonant with virama
                        result.Add(current + VIRAMA);

                        // Then add the vowel as a separate character
                        if (MatraToVowelMap.TryGetValue(word[i], out string vowel))
                        {
                            result.Add(vowel);
                        }
                        else
                        {
                            result.Add(word[i].ToString());
                        }

                        i++;
                        continue;
                    }
                    else
                    {
                        // Consonant with implicit 'a'
                        result.Add(current + VIRAMA);
                        result.Add("ಅ");
                        continue;
                    }
                }
                else
                {
                    // Final consonant with implicit 'a'
                    result.Add(current + VIRAMA);
                    result.Add("ಅ");
                    continue;
                }
            }
            // Handle independent vowels
            else if (IsIndependentVowel(word[i]))
            {
                result.Add(word[i].ToString());
                i++;
            }
            // Special marks like anusvara, visarga
            else if (IsSpecialMark(word[i]))
            {
                result.Add(word[i].ToString());
                i++;
            }
            // Skip other characters
            else
            {
                i++;
            }
        }

        return result;
    }


    private bool IsConsonant(char c)
    {
        return (c >= '\u0C95' && c <= '\u0CB9'); // Kannada consonants range
    }

    private bool IsIndependentVowel(char c)
    {
        return (c >= '\u0C85' && c <= '\u0C94'); // Kannada vowels range
    }

    private bool IsVowelMark(char c)
    {
        return (c >= '\u0CBE' && c <= '\u0CCC') || // Kannada vowel marks
               c == '\u0CD5' || c == '\u0CD6'; // Additional vowel marks
    }

    private bool IsSpecialMark(char c)
    {
        return c == '\u0C82' || // Kannada Anusvara
               c == '\u0C83' || // Kannada Visarga
               c == '\u0CCD'; // Kannada Virama
    }

    /// <summary>
    /// Check if the given letter sequence appears in any dictionary word
    /// </summary>
    public bool ContainsLetterSequence(string[] letters)
    {
        if (letters == null || letters.Length == 0)
            return false;

        return SearchSequenceInTrie(_root, letters);
    }

    /// <summary>
    /// Check if the given Helaganda text appears in any dictionary word
    /// </summary>
    public bool ContainsLetterSequence(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var letters = SplitIntoHelagandaCharacters(text).ToArray();
        return SearchSequenceInTrie(_root, letters);
    }

    private bool SearchSequenceInTrie(TrieNode node, string[] sequence, int index = 0)
    {
        if (index >= sequence.Length)
            return true;

        // Try to match the current sequence from this node
        var current = node;
        for (int i = index; i < sequence.Length; i++)
        {
            if (!current.Children.TryGetValue(sequence[i], out var child))
            {
                // Sequence doesn't match from current path
                break;
            }

            current = child;

            // If we reached the end of the sequence, we found a match
            if (i == sequence.Length - 1)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Get words that contain the given letter sequence
    /// </summary>
    public List<string> GetWordsWithLetterSequence(string[] letters)
    {
        var results = new List<string>();
        if (letters == null || letters.Length == 0)
            return results;

        CollectWordsWithSequence(_root, letters, new List<string>(), results);
        return results;
    }

    /// <summary>
    /// Get words that contain the given Helaganda text sequence
    /// </summary>
    public List<string> GetWordsWithLetterSequence(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new List<string>();

        var letters = SplitIntoHelagandaCharacters(text).ToArray();
        return GetWordsWithLetterSequence(letters);
    }

    private void CollectWordsWithSequence(TrieNode node, string[] sequence, List<string> currentPath,
        List<string> results, int limit = 100)
    {
        // Limit results to prevent excessive processing
        if (results.Count >= limit)
            return;

        // Check if the current path contains our sequence
        if (node.IsEndOfWord && ContainsSubsequence(currentPath, sequence))
        {
            results.Add(string.Join("", currentPath));
        }

        // Continue exploring all children
        foreach (var pair in node.Children)
        {
            currentPath.Add(pair.Key);
            CollectWordsWithSequence(pair.Value, sequence, currentPath, results, limit);
            currentPath.RemoveAt(currentPath.Count - 1);
        }
    }

    private bool ContainsSubsequence(List<string> list, string[] subsequence)
    {
        if (list.Count < subsequence.Length)
            return false;

        for (int i = 0; i <= list.Count - subsequence.Length; i++)
        {
            bool found = true;
            for (int j = 0; j < subsequence.Length; j++)
            {
                if (list[i + j] != subsequence[j])
                {
                    found = false;
                    break;
                }
            }

            if (found)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Get all words in the dictionary
    /// </summary>
    public List<string> GetAllWords(int limit = 1000)
    {
        var results = new List<string>();
        CollectWords(_root, new List<string>(), results, limit);
        return results;
    }

    private void CollectWords(TrieNode node, List<string> currentPath, List<string> results, int limit = 1000)
    {
        if (results.Count >= limit)
            return;

        if (node.IsEndOfWord)
        {
            results.Add(string.Join("", currentPath));
        }

        foreach (var pair in node.Children)
        {
            currentPath.Add(pair.Key);
            CollectWords(pair.Value, currentPath, results, limit);
            currentPath.RemoveAt(currentPath.Count - 1);
        }
    }
}