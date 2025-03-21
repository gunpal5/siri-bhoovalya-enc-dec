namespace SiriBhuvalyaExtractor.NLP;

/// <summary>
/// Corpus grading result
/// </summary>
public class CorpusGradingResult
{
    public int TotalFragmentArrays { get; set; }
    public int ValidFragmentArrays { get; set; }
    public double ValidArrayPercentage { get; set; }
    public int TotalFragments { get; set; }
    public int ValidFragments { get; set; }
    public double ValidFragmentPercentage { get; set; }
    public int UsefulFragments { get; set; }
    public double UsefulFragmentPercentage { get; set; }
    public string CorpusGrade { get; set; }
    public List<ArrayGradingResult> ArrayResults { get; set; } = new List<ArrayGradingResult>();
}