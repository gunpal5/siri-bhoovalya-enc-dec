

namespace SiriBhuvalyaExtractor.NLP;

/// <summary>
/// Sanskrit processor for combining and analyzing fragments
/// </summary>
public class SanskritProcessor
{
    private readonly SanskritDictionary _dictionary;
    private readonly HashSet<string> _processedCombinations = new HashSet<string>();
    private readonly int _maxCombinations = 1000;
        
    // Sanskrit sandhi rules
    private readonly List<SandhiRule> _sandhiRules = new List<SandhiRule>
    {
        // Vowel sandhi rules
        new SandhiRule("अ", "अ", "आ"),
        new SandhiRule("अ", "आ", "आ"),
        new SandhiRule("आ", "अ", "आ"),
        new SandhiRule("आ", "आ", "आ"),
        new SandhiRule("इ", "इ", "ई"),
        new SandhiRule("उ", "उ", "ऊ"),
        new SandhiRule("अ", "इ", "ए"),
        new SandhiRule("अ", "ई", "ए"),
        new SandhiRule("आ", "इ", "े"),
        new SandhiRule("आ", "ई", "े"),
        new SandhiRule("अ", "उ", "ओ"),
        new SandhiRule("अ", "ऊ", "ओ"),
        new SandhiRule("आ", "उ", "ो"),
        new SandhiRule("आ", "ऊ", "ो"),
        new SandhiRule("इ", "अ", "य"),
        new SandhiRule("ई", "अ", "य"),
        new SandhiRule("उ", "अ", "व"),
        new SandhiRule("ऊ", "अ", "व"),
            
        // Visarga sandhi
        new SandhiRule("ः", "अ", "ोऽ"),
        new SandhiRule("ः", "आ", "ा"),
            
        // Consonant sandhi
        new SandhiRule("त्", "त", "त्त"),
        new SandhiRule("द्", "द", "द्द"),
        new SandhiRule("न्", "न", "न्न")
    };
        
    public SanskritProcessor(SanskritDictionary dictionary)
    {
        _dictionary = dictionary;
    }
        
    /// <summary>
    /// Process a fragment array to find valid combinations
    /// </summary>
    public List<SanskritResult> ProcessFragments(FragmentArray array)
    {
        var results = new List<SanskritResult>();
        _processedCombinations.Clear();
            
        // Clean and normalize fragments
        var fragments = array.Fragments
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(f => DevanagariHandler.NormalizeFragment(f))
            .ToArray();
                
        if (fragments.Length == 0)
            return results;
                
        // Count matched words
        double matchCount = 0;
        
        foreach (var fragment in fragments)
        {
            // Check if fragment matches exactly with a dictionary word
            if (_dictionary.IsKnownWord(fragment))
            {
                if (DevanagariHandler.IsConsonantWithVirāma(fragment))
                {
                    matchCount += 0.1;
                    continue;
                }
                matchCount++;
                continue;
            }
            else
            { 
                // // Check if fragment is one Devanagari character shorter than a dictionary word
                // foreach (var dictWord in _dictionary.GetAllWords())
                // {
                //     if (IsOneCharacterDifference(fragment, dictWord))
                //     {
                //         matchCount+=0.5;
                //         break;
                //     }
                // }

                if (_dictionary.IsPartialMatch(fragment))
                {
                    matchCount+=0.5;
                    continue;
                }
            
                // Check for Devanagari letters in Constants file - count as 0.5
                if (IsDevanagariInConstants(fragment))
                {
                    matchCount += 0.25;
                }
            }
        }
        
        // Calculate percentage
        double percentage = fragments.Length > 0 
            ? (double)matchCount / fragments.Length * 100 
            : 0;
        
        results.Add(new SanskritResult()
        {
            Score = percentage,
        });
        return results;

        // // Try direct concatenation first
        // string directText = string.Join("", fragments);
        // TryAddResult(directText, "Direct concatenation", results);
        //     
        // int combinationCount = 0;
        //     
        // // Try adjacent fragment combinations
        // TryCombiningAdjacentFragments(fragments, results, ref combinationCount);
        //     
        // // Try applying sandhi rules
        // if (combinationCount < _maxCombinations)
        // {
        //     TryApplyingSandhiRules(fragments, results, ref combinationCount);
        // }
        //     
        // // Try specific affix combinations
        // if (combinationCount < _maxCombinations)
        // {
        //     TryAffixCombinations(fragments, results, ref combinationCount);
        // }
        //     
        // // Sort results by score
        // results = results.OrderByDescending(r => r.Score).ToList();
            
        return results;
    }
    /// <summary>
    /// Check if fragment contains Devanagari letters defined in Constants
    /// </summary>
    private bool IsDevanagariInConstants(string fragment)
    {
        // This would need to check against the Devanagari letters defined in Constants.devnagri
        // Since we don't have direct access to the Constants class content, 
        // this is a placeholder implementation
        if (DevanagariHandler.IsVowel(fragment))
            return true;
        
        return false;
    }

    /// <summary>
    /// Check if fragment is one Devanagari character shorter than a dictionary word
    /// </summary>
    private bool IsOneCharacterDifference(string fragment, string dictionaryWord)
    {
        // // Check if the fragment is exactly one character shorter
        // if (fragment.Length + 1 != dictionaryWord.Length)
        //     return false;
        //
        // // Try removing one character from the dictionary word at each position
        // // to see if it matches the fragment
        // for (int i = 0; i < dictionaryWord.Length; i++)
        // {
        //     string modified = dictionaryWord.Remove(i, 1);
        //     if (modified == fragment)
        //         return true;
        // }
        
        return false;
    }

        
    /// <summary>
    /// Try to add a result to the list
    /// </summary>
    private void TryAddResult(string text, string method, List<SanskritResult> results)
    {
        double score = CalculateScore(text);
        var words = AnalyzeWords(text);
        string translation = GenerateTranslation(words);
        bool isGrammaticallyValid = ValidateGrammar(words);
        var grammarIssues = new List<string>();
            
        if (!isGrammaticallyValid)
        {
            grammarIssues.Add("Sentence does not follow standard Sanskrit grammar");
        }
            
        results.Add(new SanskritResult
        {
            Text = text,
            Score = score,
            Words = words,
            Method = method,
            Translation = translation,
            IsGrammaticallyValid = isGrammaticallyValid,
            GrammarIssues = grammarIssues
        });
    }
        
    /// <summary>
    /// Calculate score for a Sanskrit text
    /// </summary>
    private double CalculateScore(string text)
    {
        // Start with a base score
        double score = 0.3;
            
        // Check if the whole text is a known word
        if (_dictionary.IsKnownWord(text))
        {
            return 0.95;
        }
            
        // Try to find words in the text
        var words = IdentifyWords(text);
            
        if (words.Count > 0)
        {
            // Calculate what percentage of words are known
            int knownWords = words.Count(w => _dictionary.IsKnownWord(w));
            score = Math.Max(score, 0.5 + (0.45 * knownWords / words.Count));
        }
            
        // Add a bonus for good character patterns
        score += AnalyzeDevanagariPatterns(text) * 0.1;
            
        // Ensure score is between 0 and 1
        return Math.Max(0.0, Math.Min(1.0, score));
    }
        
    /// <summary>
    /// Identify words in a text
    /// </summary>
    private List<string> IdentifyWords(string text)
    {
        var words = new List<string>();
            
        // Try sliding window approach to find known words
        for (int len = Math.Min(15, text.Length); len >= 2; len--)
        {
            for (int start = 0; start <= text.Length - len; start++)
            {
                string potentialWord = text.Substring(start, len);
                if (_dictionary.IsKnownWord(potentialWord))
                {
                    words.Add(potentialWord);
                }
            }
        }
            
        return words;
    }
        
    /// <summary>
    /// Analyze Devanagari patterns in text
    /// </summary>
    private double AnalyzeDevanagariPatterns(string text)
    {
        int goodPatterns = 0;
        int badPatterns = 0;
            
        for (int i = 0; i < text.Length - 1; i++)
        {
            // Good pattern: consonant followed by vowel sign
            if (DevanagariHandler.IsConsonant(text[i]) && DevanagariHandler.IsVowelSign(text[i+1]))
            {
                goodPatterns++;
            }
                
            // Bad pattern: two vowels in sequence
            if (DevanagariHandler.IsVowel(text[i]) && DevanagariHandler.IsVowel(text[i+1]))
            {
                badPatterns++;
            }
        }
            
        // Calculate pattern quality
        double total = goodPatterns + badPatterns;
        if (total > 0)
        {
            return goodPatterns / total;
        }
            
        return 0.5; // Neutral score if no patterns found
    }
        
    /// <summary>
    /// Analyze words in a text
    /// </summary>
    private List<SanskritWord> AnalyzeWords(string text)
    {
        var result = new List<SanskritWord>();
        var identifiedWords = IdentifyWords(text);
            
        foreach (var word in identifiedWords)
        {
            if (_dictionary.TryGetEntries(word, out var entries))
            {
                var entry = entries.First();
                result.Add(new SanskritWord
                {
                    Text = word,
                    Type = entry.Type,
                    Meaning = entry.Definition
                });
            }
        }
            
        // If no words identified, add the whole text as an unknown word
        if (result.Count == 0)
        {
            result.Add(new SanskritWord
            {
                Text = text,
                Type = "unknown",
                Meaning = "[unknown]"
            });
        }
            
        return result;
    }
        
    /// <summary>
    /// Generate a translation for the words
    /// </summary>
    private string GenerateTranslation(List<SanskritWord> words)
    {
        if (words.Count == 0)
            return string.Empty;
                
        var translations = words.Select(w => w.Meaning).ToList();
        return string.Join(" ", translations);
    }
        
    /// <summary>
    /// Validate grammar of the words
    /// </summary>
    private bool ValidateGrammar(List<SanskritWord> words)
    {
        // Basic grammar validation
        if (words.Count == 0)
            return false;
                
        // If all words are unknown, we can't validate
        if (words.All(w => w.Type == "unknown"))
            return false;
                
        // For simplicity, consider it valid if we have at least one known word
        return words.Any(w => w.Type != "unknown");
    }
        
    /// <summary>
    /// Try combining adjacent fragments
    /// </summary>
    private void TryCombiningAdjacentFragments(string[] fragments, List<SanskritResult> results, ref int combinationCount)
    {
        // Try combining different numbers of adjacent fragments
        for (int numMerges = 1; numMerges < fragments.Length && combinationCount < _maxCombinations; numMerges++)
        {
            TryMergingAdjacentFragments(fragments, numMerges, 0, new List<int>(), results, ref combinationCount);
        }
    }
        
    /// <summary>
    /// Recursively try merging adjacent fragments
    /// </summary>
    private void TryMergingAdjacentFragments(
        string[] fragments, int remainingMerges, int startIdx, 
        List<int> selectedMerges, List<SanskritResult> results, 
        ref int combinationCount)
    {
        if (remainingMerges == 0 || startIdx >= fragments.Length - 1 || combinationCount >= _maxCombinations)
        {
            if (remainingMerges == 0 && selectedMerges.Count > 0)
            {
                // Apply the selected merges
                var mergedFragments = new List<string>(fragments);
                    
                // Apply merges in reverse order to avoid index shifting
                foreach (var idx in selectedMerges.OrderByDescending(i => i))
                {
                    mergedFragments[idx] = mergedFragments[idx] + mergedFragments[idx + 1];
                    mergedFragments.RemoveAt(idx + 1);
                }
                    
                string combinedText = string.Join("", mergedFragments);
                    
                if (!_processedCombinations.Contains(combinedText))
                {
                    _processedCombinations.Add(combinedText);
                    TryAddResult(combinedText, $"Combined {selectedMerges.Count} adjacent pairs", results);
                    combinationCount++;
                }
            }
            return;
        }
            
        // Try merging at current position
        selectedMerges.Add(startIdx);
        if (selectedMerges.Count <= fragments.Length) // Prevent overly deep recursion
        {
            TryMergingAdjacentFragments(fragments, remainingMerges - 1, startIdx + 2, 
                selectedMerges, results, ref combinationCount);
        }
        selectedMerges.RemoveAt(selectedMerges.Count - 1);
            
        // Skip merging at current position
        if (selectedMerges.Count <= fragments.Length) // Prevent overly deep recursion
        {
            TryMergingAdjacentFragments(fragments, remainingMerges, startIdx + 1, 
                selectedMerges, results, ref combinationCount);
        }
    }
        
    /// <summary>
    /// Try applying Sanskrit sandhi rules
    /// </summary>
    private void TryApplyingSandhiRules(string[] fragments, List<SanskritResult> results, ref int combinationCount)
    {
        // Try applying sandhi rules to adjacent pairs
        for (int i = 0; i < fragments.Length - 1 && combinationCount < _maxCombinations; i++)
        {
            string first = fragments[i];
            string second = fragments[i + 1];
                
            foreach (var rule in _sandhiRules)
            {
                // Check if this rule applies
                if (first.EndsWith(rule.First) && second.StartsWith(rule.Second))
                {
                    // Apply the rule
                    string combined = first.Substring(0, first.Length - rule.First.Length) + 
                                      rule.Result + 
                                      second.Substring(rule.Second.Length);
                        
                    // Create a new fragment array with the combined fragments
                    var combinedFragments = new List<string>(fragments);
                    combinedFragments[i] = combined;
                    combinedFragments.RemoveAt(i + 1);
                        
                    string combinedText = string.Join("", combinedFragments);
                        
                    if (!_processedCombinations.Contains(combinedText))
                    {
                        _processedCombinations.Add(combinedText);
                        TryAddResult(combinedText, $"Sandhi rule: {rule.First}+{rule.Second}→{rule.Result}", results);
                        combinationCount++;
                            
                        // Recursively try more combinations
                        if (combinedFragments.Count > 1 && combinationCount < _maxCombinations)
                        {
                            TryApplyingSandhiRules(combinedFragments.ToArray(), results, ref combinationCount);
                        }
                    }
                }
            }
        }
    }
        
    /// <summary>
    /// Try combining using known Sanskrit affixes
    /// </summary>
    private void TryAffixCombinations(string[] fragments, List<SanskritResult> results, ref int combinationCount)
    {
        // Check for prefixes
        for (int i = 0; i < fragments.Length - 1 && combinationCount < _maxCombinations; i++)
        {
            if (_dictionary.IsPrefix(fragments[i]))
            {
                // Try this as a prefix
                var combinedFragments = new List<string>(fragments);
                combinedFragments[i] = fragments[i] + fragments[i + 1];
                combinedFragments.RemoveAt(i + 1);
                    
                string combinedText = string.Join("", combinedFragments);
                    
                if (!_processedCombinations.Contains(combinedText))
                {
                    _processedCombinations.Add(combinedText);
                    TryAddResult(combinedText, $"Prefix: {fragments[i]}", results);
                    combinationCount++;
                        
                    // Recursively try more combinations
                    if (combinedFragments.Count > 1 && combinationCount < _maxCombinations)
                    {
                        TryCombiningAdjacentFragments(combinedFragments.ToArray(), results, ref combinationCount);
                    }
                }
            }
        }
            
        // Check for suffixes
        for (int i = 0; i < fragments.Length - 1 && combinationCount < _maxCombinations; i++)
        {
            if (_dictionary.IsSuffix(fragments[i + 1]))
            {
                // Try this as a suffix
                var combinedFragments = new List<string>(fragments);
                combinedFragments[i] = fragments[i] + fragments[i + 1];
                combinedFragments.RemoveAt(i + 1);
                    
                string combinedText = string.Join("", combinedFragments);
                    
                if (!_processedCombinations.Contains(combinedText))
                {
                    _processedCombinations.Add(combinedText);
                    TryAddResult(combinedText, $"Suffix: {fragments[i + 1]}", results);
                    combinationCount++;
                        
                    // Recursively try more combinations
                    if (combinedFragments.Count > 1 && combinationCount < _maxCombinations)
                    {
                        TryCombiningAdjacentFragments(combinedFragments.ToArray(), results, ref combinationCount);
                    }
                }
            }
        }
    }
}