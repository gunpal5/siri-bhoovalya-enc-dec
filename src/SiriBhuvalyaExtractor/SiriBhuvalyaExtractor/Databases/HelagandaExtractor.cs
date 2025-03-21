namespace SiriBhuvalyaExtractor.Databases;

public class HelagandaExtractor
{
    public static List<string> ExtractHelagandaWords(string fileName)
    {
        List<string> words = new List<string>();
        var lines = File.ReadAllLines(fileName);
        HashSet<string> uniqueWords = new HashSet<string>();
        foreach (var content in lines) 
        {
            // In the provided document, dictionary entries appear to follow this pattern:
            // Word - Definition (with examples and citations)
            
            // This regex matches Kannada words at the beginning of a line followed by " - "
            // The \u0C80-\u0CFF range covers Kannada Unicode characters
            // We include some special characters like "‌" (zero-width non-joiner) that might be part of words
            
            // if(!content.Contains("-"))
            //     continue;
            
            var word = content.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
            
            
            
            // Filter out any entries that don't look like proper Kannada words
            if (!string.IsNullOrWhiteSpace(word))
            {
                uniqueWords.Add(word);
            }
            
            // Convert the unique words to a list and sort them
            
            
        }
        words.AddRange(uniqueWords);
        words.Sort();
        
            
        return words;
    }
}
