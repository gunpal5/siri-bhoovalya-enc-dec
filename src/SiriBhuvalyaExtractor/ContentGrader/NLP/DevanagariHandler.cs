using System.Text.RegularExpressions;

namespace SiriBhuvalyaExtractor.NLP;

/// <summary>
/// Devanagari character handler for Sanskrit
/// </summary>
public static class DevanagariHandler
{
    // All Devanagari vowels (independent forms)
    private static readonly HashSet<string> _vowels = new HashSet<string>
    {
        "अ", "आ", "इ", "ई", "उ", "ऊ", "ऋ", "ॠ", "ऌ", "ॡ", "ए", "ऐ", "ओ", "औ",
        "ऍ", "ऑ", "ऎ", "ऒ"
    };
        
    // All Devanagari vowel signs (dependent forms)
    private static readonly HashSet<string> _vowelSigns = new HashSet<string>
    {
        "ा", "ि", "ी", "ु", "ू", "ृ", "ॄ", "ॢ", "ॣ", "े", "ै", "ो", "ौ",
        "आा", "ईी", "ऊू", "ॠॄ", "एा", "एाा", "ऐो", "ऐोो", "ओो", "ओोो", "औौ", "औौौ"
    };
        
    // All Devanagari consonants
    private static readonly HashSet<string> _consonants = new HashSet<string>
    {
        "क", "ख", "ग", "घ", "ङ", 
        "च", "छ", "ज", "झ", "ञ", 
        "ट", "ठ", "ड", "ढ", "ण", 
        "त", "थ", "द", "ध", "न", 
        "प", "फ", "ब", "भ", "म", 
        "य", "र", "ल", "व", "श", 
        "ष", "स", "ह",
        // Additional consonants
        "ळ", "क़", "ख़", "ग़", "ज़", "ड़", "ढ़", "फ़", "य़", "ऴ", "ऩ", "ऱ"
    };
        
    // Consonants with virāma (halanta)
    private static readonly HashSet<string> _consonantsWithVirāma = new HashSet<string>
    {
        "क्", "ख्", "ग्", "घ्", "ङ्", 
        "च्", "छ्", "ज्", "झ्", "ञ्", 
        "ट्", "ठ्", "ड्", "ढ्", "ण्", 
        "त्", "थ्", "द्", "ध्", "न्", 
        "प्", "फ्", "ब्", "भ्", "म्", 
        "य्", "र्", "ल्", "व्", "श्", 
        "ष्", "स्", "ह्",
        // Additional
        "ळ्", "क़्", "ख़्", "ग़्", "ज़्", "ड़्", "ढ़्", "फ़्", "य़्", "ऴ्", "ऩ्", "ऱ्"
    };
        
    // Special marks and signs
    private static readonly HashSet<string> _specialSigns = new HashSet<string>
    {
        "ं", "ः", "ँ", "ऽ", "।", "॥", "॰",
        "…", "::"
    };
        
    /// <summary>
    /// Check if a string is a vowel (independent form)
    /// </summary>
    public static bool IsVowel(string character)
    {
        return _vowels.Contains(character);
    }
        
    /// <summary>
    /// Check if a string is a vowel sign (dependent form)
    /// </summary>
    public static bool IsVowelSign(string character)
    {
        return _vowelSigns.Contains(character);
    }
        
    /// <summary>
    /// Check if a string is a consonant
    /// </summary>
    public static bool IsConsonant(string character)
    {
        return _consonants.Contains(character);
    }
        
    /// <summary>
    /// Check if a string is a consonant with virāma
    /// </summary>
    public static bool IsConsonantWithVirāma(string character)
    {
        return _consonantsWithVirāma.Contains(character);
    }
        
    /// <summary>
    /// Check if a string is a special sign
    /// </summary>
    public static bool IsSpecialSign(string character)
    {
        return _specialSigns.Contains(character);
    }
        
    /// <summary>
    /// Check if a character is a vowel
    /// </summary>
    public static bool IsVowel(char c)
    {
        return "अआइईउऊऋॠऌॡएऐओऔऍऑऎऒ".Contains(c);
    }
        
    /// <summary>
    /// Check if a character is a consonant
    /// </summary>
    public static bool IsConsonant(char c)
    {
        return "कखगघङचछजझञटठडढणतथदधनपफबभमयरलवशषसहळक़ख़ग़ज़ड़ढ़फ़य़ऴऩऱ".Contains(c);
    }
        
    /// <summary>
    /// Check if a character is a vowel sign
    /// </summary>
    public static bool IsVowelSign(char c)
    {
        return "ािीुूृॄॢॣेैोौ".Contains(c);
    }
        
    /// <summary>
    /// Validate a Sanskrit fragment
    /// </summary>
    public static bool IsValidFragment(string fragment)
    {
        if (string.IsNullOrEmpty(fragment))
            return false;
                
        // Check for common invalid patterns
        if (fragment.Contains("््") || // Double virāma
            Regex.IsMatch(fragment, "([ािीुूृॄॢॣेैोौ]){2,}")) // Two vowel signs in sequence
            return false;
            
        // More checks can be added based on specific requirements
            
        return true;
    }
        
    /// <summary>
    /// Normalize a Sanskrit fragment
    /// </summary>
    public static string NormalizeFragment(string fragment)
    {
        // Replace common incorrect combinations
        fragment = fragment.Replace("ाी", "ी")
            .Replace("ीि", "ी")
            .Replace("ुू", "ू")
            .Replace("ूु", "ू")
            .Replace("ृॄ", "ॄ")
            .Replace("ेै", "ै")
            .Replace("ोौ", "ौ");
            
        return fragment;
    }
        
    /// <summary>
    /// Calculate quality score for a fragment
    /// </summary>
    public static double CalculateFragmentQuality(string fragment)
    {
        // Normalize first
        fragment = NormalizeFragment(fragment);
            
        // Invalid fragments get 0.0
        if (!IsValidFragment(fragment))
            return 0.0;
                
        // Start with base score
        double score = 0.6;
            
        // Check for good patterns (simplified for this consolidated file)
        int goodPatterns = 0;
        int badPatterns = 0;
            
        for (int i = 0; i < fragment.Length - 1; i++)
        {
            // Good pattern: consonant followed by vowel sign
            if (IsConsonant(fragment[i]) && IsVowelSign(fragment[i+1]))
            {
                goodPatterns++;
            }
                
            // Bad pattern: two vowels in sequence
            if (IsVowel(fragment[i]) && IsVowel(fragment[i+1]))
            {
                badPatterns++;
            }
        }
            
        // Adjust score based on patterns
        if (fragment.Length > 1)
        {
            score += 0.2 * goodPatterns / (fragment.Length - 1);
            score -= 0.3 * badPatterns / (fragment.Length - 1);
        }
            
        return Math.Max(0.0, Math.Min(1.0, score));
    }
}