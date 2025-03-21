namespace SiriBhuvalyaExtractor.NLP;

/// <summary>
/// Dictionary entry
/// </summary>
public class DictionaryEntry
{
    public string Headword { get; set; }
    public string Type { get; set; }
    public string Definition { get; set; }
    public List<string> Examples { get; set; } = new List<string>();
}