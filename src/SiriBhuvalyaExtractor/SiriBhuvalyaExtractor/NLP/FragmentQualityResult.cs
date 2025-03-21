namespace SiriBhuvalyaExtractor.NLP;

/// <summary>
/// Fragment quality evaluation result
/// </summary>
public class FragmentQualityResult
{
    public string Fragment { get; set; }
    public double Quality { get; set; }
    public bool IsValid { get; set; }
}