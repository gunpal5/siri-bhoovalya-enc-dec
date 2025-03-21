namespace SiriBhuvalyaExtractor.NLP;

/// <summary>
/// Grading result for a fragment array
/// </summary>
public class ArrayGradingResult
{
    public string ArrayId { get; set; }
    public int TotalFragments { get; set; }
    public int ValidFragments { get; set; }
    public double ValidFragmentPercentage { get; set; }
    public int UsefulFragments { get; set; }
    public double UsefulFragmentPercentage { get; set; }
    public double OverallQualityScore { get; set; }
    public string ArrayGrade { get; set; }
    public string BestCombination { get; set; }
    public double BestCombinationScore { get; set; }
    public string BestCombinationTranslation { get; set; }
    public bool IsGrammaticallyValid { get; set; }
    public List<string> GrammaticalIssues { get; set; } = new List<string>();
    public List<FragmentQualityResult> FragmentQualities { get; set; } = new List<FragmentQualityResult>();
}