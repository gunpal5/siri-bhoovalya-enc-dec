namespace SiriBhuvalyaExtractor.NLP;

/// <summary>
/// Sanskrit sandhi rule
/// </summary>
public class SandhiRule
{
    public string First { get; }
    public string Second { get; }
    public string Result { get; }
        
    public SandhiRule(string first, string second, string result)
    {
        First = first;
        Second = second;
        Result = result;
    }
}