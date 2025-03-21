namespace SiriBhuvalyaExtractor.NLP;

/// <summary>
/// Sanskrit result with additional properties
/// </summary>
public class SanskritResult
{
    public string Text { get; set; }
    public double Score { get; set; }
    public string Method { get; set; }
    public List<SanskritWord> Words { get; set; } = new List<SanskritWord>();
    public string Translation { get; set; }
    public bool IsGrammaticallyValid { get; set; }
    public List<string> GrammarIssues { get; set; } = new List<string>();
}