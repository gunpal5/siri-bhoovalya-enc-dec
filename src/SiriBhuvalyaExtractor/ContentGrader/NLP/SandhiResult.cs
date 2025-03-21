namespace SiriBhuvalyaExtractor.NLP;

/// <summary>
/// Result of sandhi combination
/// </summary>
public class SandhiResult
{
    public string Combined { get; set; }
    public string Rule { get; set; }
    public List<string> Components { get; set; } = new List<string>();
}