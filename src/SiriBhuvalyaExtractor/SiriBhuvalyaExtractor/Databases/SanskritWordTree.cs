using SiriBhuvalyaExtractor.Extractor;

namespace SiriBhuvalyaExtractor.Databases;

public class SanskritWordTree
{
    private class TrieNode
    {
        public Dictionary<string, TrieNode> Children { get; } = new Dictionary<string, TrieNode>();
        public bool IsEndOfWord { get; set; }
    }

    private readonly TrieNode _root = new TrieNode();

    // Complete set of Devanagari characters as provided
    public static readonly string[] DevanagariChars =
    {
        "अ", "आ", "आा", "इ", "ई", "ईी", "उ", "ऊ", "ऊू", "ऋ", "ॠ", "ॠॄ", "ळ्", "ळु", "ळू", "ए", "एा", "एाा", "ऐ", "ऐो",
        "ऐोो", "ओ", "ओो", "ओोो", "औ", "औौ", "औौौ",
        "क्", "ख्", "ग्", "घ्", "ङ्", "च्", "छ्", "ज्", "झ्", "ञ्", "ट्", "ठ्", "ड्", "ढ्", "ण्", "त्", "थ्", "द्",
        "ध्", "न्", "प्", "फ्", "ब्", "भ्", "म्", "य्", "र्", "ल्", "व्", "श्",
        "ष्", "स्", "ह्", "ं", "ः", "…", "::",
        "क़्", "ख़्", "ग़्", "ज़्", "ड़्", "ढ़्", "फ़्", "य़्", "ऴ्", "ऩ्", "ऴ", "ऱ्", "ऍ", "ऑ", "ऎ", "ऒ", "ऌ", "ॡ"
    };

    // Set of vowels (first 27 entries in the array)
    private static readonly HashSet<string> Vowels = new HashSet<string>(DevanagariChars.Take(27));

    // Set of half consonants (entries 27-62 in the array)
    private static readonly HashSet<string> HalfConsonants = new HashSet<string>(DevanagariChars.Skip(27).Take(36));

    // Updated comprehensive mapping of matras to corresponding vowels
    private static readonly Dictionary<char, string> MatraToVowelMap = new Dictionary<char, string>
    {
        // Basic vowel marks
        { 'ा', "आ" }, // aa
        { 'ि', "इ" }, // i
        { 'ी', "ई" }, // ii
        { 'ु', "उ" }, // u
        { 'ू', "ऊ" }, // uu
        { 'ृ', "ऋ" }, // ri
        { 'ॄ', "ॠ" }, // rri
        { 'े', "ए" }, // e
        { 'ै', "ऐ" }, // ai
        { 'ो', "ओ" }, // o
        { 'ौ', "औ" }, // au

        // Special vowel marks for some dialects and extensions
        { 'ॅ', "ऍ" }, // candra e
        { 'ॉ', "ऑ" }, // candra o
        { 'ॆ', "ऎ" }, // short e
        { 'ॊ', "ऒ" }, // short o
        { 'ॢ', "ऌ" }, // vocalic l
        { 'ॣ', "ॡ" }, // vocalic ll

        // Special marks
        { 'ं', "ं" }, // anusvara
        { 'ः', "ः" }, // visarga
        { '़', "़" }, // nukta
        { '्', "्" }, // virama/halant
    };

    // Special characters
    private const char VIRAMA = '\u094D'; // Halant

    /// <summary>
    /// Load Sanskrit words from file and build the tree
    /// </summary>
    public void LoadDictionary(string filePath)
    {
        var words = SanskritDictionaryExtractor.Extract(filePath);
        foreach (var word in words)
        {
            AddWord(word);
        }

        Console.WriteLine($"Loaded {words.Count} Sanskrit words into the tree");
    }

    /// <summary>
    /// Add a collection of Sanskrit words to the tree
    /// </summary>
    public void AddWords(IEnumerable<string> words)
    {
        int count = 0;
        foreach (var word in words)
        {
            AddWord(word);
            count++;
        }

        Console.WriteLine($"Added {count} Sanskrit words to the tree");
    }

    /// <summary>
    /// Add a single word to the tree
    /// </summary>
    public void AddWord(string word)
    {
        if (string.IsNullOrEmpty(word))
            return;

        var characters = SplitIntoSanskritCharacters(word);
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
    /// Split a Sanskrit word into individual characters based on Devanagari rules
    /// </summary>
    public List<string> SplitIntoSanskritCharacters(string word)
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
                        result.Add("अ");
                        continue;
                    }
                }
                else
                {
                    // Final consonant with implicit 'a'
                    result.Add(current + VIRAMA);
                    result.Add("अ");
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
        return (c >= '\u0915' && c <= '\u0939') || // Basic consonants
               (c >= '\u0958' && c <= '\u095F'); // Extended consonants
    }

    private bool IsIndependentVowel(char c)
    {
        return (c >= '\u0904' && c <= '\u0914') || // Basic vowels
               (c >= '\u0960' && c <= '\u0963') || // Additional vowels
               c == '\u0972' || // Additional vowel forms
               c == '\u0905' || c == '\u0906' || // a, aa
               c == '\u0907' || c == '\u0908' || // i, ii
               c == '\u0909' || c == '\u090A' || // u, uu
               c == '\u090B' || c == '\u0960' || // ri, rii
               c == '\u090C' || c == '\u0961' || // li, lii 
               c == '\u090D' || c == '\u090E' || c == '\u090F' || // e variants
               c == '\u0910' || // ai
               c == '\u0911' || c == '\u0912' || c == '\u0913' || // o variants
               c == '\u0914'; // au
    }

    private bool IsVowelMark(char c)
    {
        return (c >= '\u093E' && c <= '\u094C') || // Vowel marks
               c == '\u094E' || c == '\u094F' || // Additional vowel marks
               c == '\u0955' || c == '\u0956' || // Additional vowel marks
               c == '\u0957'; // Additional vowel mark
    }

    private bool IsSpecialMark(char c)
    {
        return c == '\u0901' || c == '\u0902' || // Chandrabindu, Anusvara
               c == '\u0903' || // Visarga
               c == '\u093C' || // Nukta
               c == '\u094D'; // Virama
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
    /// Check if the given Sanskrit text appears in any dictionary word
    /// </summary>
    public bool ContainsLetterSequence(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var letters = SplitIntoSanskritCharacters(text).ToArray();
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

        // // Try all children recursively
        // foreach (var child in node.Children.Values)
        // {
        //     if (SearchSequenceInTrie(child, sequence, index))
        //         return true;
        // }

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
    /// Get words that contain the given Sanskrit text sequence
    /// </summary>
    public List<string> GetWordsWithLetterSequence(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new List<string>();

        var letters = SplitIntoSanskritCharacters(text).ToArray();
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