using System.Text;
using System.Text.RegularExpressions;

namespace SiriBhuvalyaExtractor.NLP;

/// <summary>
/// Dictionary loader for Sanskrit
/// </summary>
public class SanskritDictionary
{
    private Dictionary<string, List<DictionaryEntry>> _dictionary = new Dictionary<string, List<DictionaryEntry>>();
    private HashSet<string> _knownWords = new HashSet<string>();
    private HashSet<string> _partialWords = new HashSet<string>();
        
    // Common Sanskrit prefixes (उपसर्ग)
    private static readonly HashSet<string> _commonPrefixes = new HashSet<string> 
    {
        "अ", "अन्", "अनु", "अप", "अपि", "अभि", "अव", "आ", 
        "उत्", "उप", "दुर्", "दुस्", "निर्", "निस्", "परा", "परि", 
        "प्र", "प्रति", "वि", "सम्", "सु", "अति", "अधि", "अनु", "अन्तर्"
    };
        
    // Common Sanskrit suffixes (प्रत्यय)
    private static readonly HashSet<string> _commonSuffixes = new HashSet<string>
    {
        "क", "कार", "ता", "त्व", "त्र", "इक", "इन्", "ईय", "अन", "ल", "वत्", "मत्", "तम", "तर", "मय"
    };
        
    public int WordCount => _knownWords.Count;
        
    public SanskritDictionary(string path)
    {
        LoadDictionary(path);
    }
        
    private void LoadDictionary(string path)
    {
        // Check if path is a directory or file
        // if (Directory.Exists(path))
        // {
        //     foreach (var file in Directory.GetFiles(path, "*.txt"))
        //     {
                ParseDictionaryFile(path);
        //     }
        // }
        // else if (File.Exists(path))
        // {
        //     ParseDictionaryFile(path);
        // }
        // else
        // {
        //     throw new FileNotFoundException($"Dictionary path not found: {path}");
        // }
    }
        
    private void ParseDictionaryFile(string filePath)
    {
        string content = File.ReadAllText(filePath, Encoding.UTF8);
            
        // Try to detect if it's a LEND format dictionary
        if (content.Contains("<LEND>"))
        {
            ParseLENDFormat(content);
        }
        else
        {
            ParseSimpleFormat(filePath);
        }
    }
        
    private void ParseLENDFormat(string content)
    {
        // Split into entries
        string[] entries = Regex.Split(content, @"<LEND>\s*");
            
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry))
                continue;
                
            try
            {
                ParseLENDEntry(entry.Trim());
            }
            catch (Exception ex)
            {
                // Skip problematic entries
                Console.WriteLine($"Warning: Skipping problematic entry: {ex.Message}");
            }
        }
    }
        
    private void ParseLENDEntry(string entryText)
    {
        // Check if this is a valid entry
        if (!entryText.StartsWith("<L>"))
            return;
                
        // Extract headword
        string k1 = ExtractField(entryText, "<k1>", "</k1>");
        if (string.IsNullOrEmpty(k1))
        {
            // Try alternate format
            int k1Start = entryText.IndexOf("<k1>");
            if (k1Start == -1) return;
                
            k1Start += 4; // Length of "<k1>"
                
            int k1End = entryText.IndexOf("<k2>", k1Start);
            if (k1End == -1) return;
                
            k1 = entryText.Substring(k1Start, k1End - k1Start).Trim();
        }
            
        // Extract type information
        string typeCode = ExtractField(entryText, "<e>", "\n");
        if (string.IsNullOrEmpty(typeCode))
        {
            typeCode = ExtractField(entryText, "<e>", " ");
        }
            
        // Extract definition
        string definition = "";
        int defStart = entryText.IndexOf("¦");
        if (defStart != -1)
        {
            definition = entryText.Substring(defStart + 1).Trim();
                
            // Clean up the definition
            definition = Regex.Replace(definition, @"<[^>]+>", "");
            definition = Regex.Replace(definition, @"\([^)]+\)", "");
            definition = definition.Replace("\n", " ").Replace("\r", " ");
                
            // Limit length
            if (definition.Length > 100)
            {
                definition = definition.Substring(0, 97) + "...";
            }
        }
            
        // Extract examples
        var examples = new List<string>();
        var matches = Regex.Matches(entryText, @"<s>([^<]+)</s>");
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                examples.Add(match.Groups[1].Value);
            }
        }
            
        // Create entry
        var entry = new DictionaryEntry
        {
            Headword = k1.Trim(),
            Type = DetermineType(typeCode),
            Definition = definition,
            Examples = examples
        };
            
        // Add to dictionary
        if (!_dictionary.ContainsKey(entry.Headword))
        {
            _dictionary[entry.Headword] = new List<DictionaryEntry>();
        }
            
        _dictionary[entry.Headword].Add(entry);
        _knownWords.Add(entry.Headword);
            
        // Check if the Headword ends with specific characters
        var specialEndings = new[] { "ं", "ः", "ँ", "ऽ", "।", "॥", "॰", "…", "::", "अ" };
        if (specialEndings.Any(suffix => entry.Headword.EndsWith(suffix)))
        {
            var modifiedHeadword = entry.Headword.Substring(0, entry.Headword.Length - 1);
            _partialWords.Add(modifiedHeadword);
        }
        
        // Add examples to known words
        foreach (var example in examples)
        {
            _knownWords.Add(example);
        }
    }
        
    private string ExtractField(string text, string startTag, string endTag)
    {
        int start = text.IndexOf(startTag);
        if (start == -1) return null;
            
        start += startTag.Length;
        int end = text.IndexOf(endTag, start);
        if (end == -1) return null;
            
        return text.Substring(start, end - start);
    }
        
    private void ParseSimpleFormat(string filePath)
    {
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;
                    
            try
            {
                var parts = line.Split('|');
                if (parts.Length >= 3)
                {
                    var word = parts[0].Trim();
                    var entry = new DictionaryEntry
                    {
                        Headword = word,
                        Type = parts[1].Trim(),
                        Definition = parts[2].Trim()
                    };
                        
                    if (parts.Length >= 4)
                    {
                        entry.Examples = parts[3].Split(',').Select(f => f.Trim()).ToList();
                    }
                        
                    if (!_dictionary.ContainsKey(word))
                    {
                        _dictionary[word] = new List<DictionaryEntry>();
                    }
                        
                    _dictionary[word].Add(entry);
                    _knownWords.Add(word);
                        
                    // Add examples too
                    foreach (var example in entry.Examples)
                    {
                        _knownWords.Add(example);
                    }
                }
            }
            catch
            {
                // Skip problematic lines
            }
        }
    }
        
    private string DetermineType(string typeCode)
    {
        if (string.IsNullOrEmpty(typeCode))
            return "unknown";
                
        // Check type code for common patterns
        if (typeCode.Contains("A") || typeCode.Contains("P") || 
            typeCode.Contains("verb") || typeCode.Contains("v."))
        {
            return "verb";
        }
        else if (typeCode.Contains("m") || typeCode.Contains("f") || 
                 typeCode.Contains("n") || typeCode.Contains("noun") || 
                 typeCode.Contains("subst"))
        {
            return "noun";
        }
        else if (typeCode.Contains("adj") || typeCode.Contains("a."))
        {
            return "adjective";
        }
        else if (typeCode.Contains("adv") || typeCode.Contains("ind") || 
                 typeCode.Contains("indecl"))
        {
            return "indeclinable";
        }
            
        return "unknown";
    }
        
    /// <summary>
    /// Check if a word exists in the dictionary
    /// </summary>
    public bool IsKnownWord(string word)
    {
        return _knownWords.Contains(word);
    }
        
    /// <summary>
    /// Try to get entries for a word
    /// </summary>
    public bool TryGetEntries(string word, out List<DictionaryEntry> entries)
    {
        return _dictionary.TryGetValue(word, out entries);
    }
        
    /// <summary>
    /// Check if a string is a known prefix
    /// </summary>
    public bool IsPrefix(string text)
    {
        return _commonPrefixes.Contains(text);
    }
        
    /// <summary>
    /// Check if a string is a known suffix
    /// </summary>
    public bool IsSuffix(string text)
    {
        return _commonSuffixes.Contains(text);
    }

    public IEnumerable<string> GetAllWords()
    {
        return _dictionary.Keys;
    }

    public bool IsPartialMatch(string fragment)
    {
       return _partialWords.Contains(fragment);
    }
}