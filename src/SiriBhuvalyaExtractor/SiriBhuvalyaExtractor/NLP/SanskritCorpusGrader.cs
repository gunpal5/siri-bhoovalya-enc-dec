namespace SiriBhuvalyaExtractor.NLP;

/// <summary>
/// Sanskrit corpus grader
/// </summary>
public class SanskritCorpusGrader
{
    private readonly SanskritDictionary _dictionary;
    private readonly SanskritProcessor _processor;
        
    public SanskritCorpusGrader(SanskritDictionary dictionary)
    {
        _dictionary = dictionary;
        _processor = new SanskritProcessor(dictionary);
    }
        
    /// <summary>
    /// Grade a collection of fragment arrays
    /// </summary>
    public CorpusGradingResult GradeCorpus(List<FragmentArray> fragmentArrays)
    {
        var result = new CorpusGradingResult();
            
        int totalArrays = fragmentArrays.Count;
        int validArrays = 0;
        int totalFragments = 0;
        int validFragments = 0;
        int usefulFragments = 0;
        List<ArrayGradingResult> arrayResults = new List<ArrayGradingResult>();
            
        // Analyze each fragment array
        foreach (var array in fragmentArrays)
        {
            var arrayResult = GradeFragmentArray(array);
            arrayResults.Add(arrayResult);
                
            // Update corpus statistics
            totalFragments += array.Fragments.Length;
            // validFragments += arrayResult.ValidFragments;
            // usefulFragments += arrayResult.UsefulFragments;
                
            if (arrayResult.OverallQualityScore >= 0.5)
            {
                validArrays++;
            }
        }
            
        // Calculate corpus statistics
        result.TotalFragmentArrays = totalArrays;
        result.ValidFragmentArrays = validArrays;
        result.ValidArrayPercentage = totalArrays > 0 ? (double)validArrays / totalArrays * 100 : 0;
            
        result.TotalFragments = totalFragments;
        result.ValidFragments = validFragments;
        result.ValidFragmentPercentage = totalFragments > 0 ? (double)validFragments / totalFragments * 100 : 0;
            
        result.UsefulFragments = usefulFragments;
        result.UsefulFragmentPercentage = totalFragments > 0 ? (double)usefulFragments / totalFragments * 100 : 0;
            
        result.ArrayResults = arrayResults;
            
        // Assign corpus grade
        result.CorpusGrade = AssignGrade(result.UsefulFragmentPercentage);
            
        return result;
    }
        
    /// <summary>
    /// Grade a single fragment array
    /// </summary>
    public ArrayGradingResult GradeFragmentArray(FragmentArray array)
    {
        var result = new ArrayGradingResult
        {
            ArrayId = array.Id,
            TotalFragments = array.Fragments.Length
        };
            
        // // Validate individual fragments first
        // int validFragmentCount = 0;
        // List<FragmentQualityResult> fragmentQualities = new List<FragmentQualityResult>();
        //     
        // foreach (var fragment in array.Fragments)
        // {
        //     double quality = DevanagariHandler.CalculateFragmentQuality(fragment);
        //     bool isValid = DevanagariHandler.IsValidFragment(fragment);
        //         
        //     fragmentQualities.Add(new FragmentQualityResult
        //     {
        //         Fragment = fragment,
        //         Quality = quality,
        //         IsValid = isValid
        //     });
        //         
        //     if (isValid)
        //     {
        //         validFragmentCount++;
        //     }
        // }
        //     
        // result.ValidFragments = validFragmentCount;
        // result.ValidFragmentPercentage = array.Fragments.Length > 0 ? 
        //     (double)validFragmentCount / array.Fragments.Length * 100 : 0;
        // result.FragmentQualities = fragmentQualities;
        //     
        // Process valid fragments to find combinations
        var validFragments = array.Fragments
            .Where(f => DevanagariHandler.IsValidFragment(f))
            .ToArray();
                
        var validArray = new FragmentArray
        {
            Id = array.Id,
            Fragments = validFragments
        };
            
        // Process if we have valid fragments
        if (validFragments.Length > 0)
        {
            var processedResults = _processor.ProcessFragments(validArray);
                
            // // Sort by score
            // processedResults = processedResults.OrderByDescending(r => r.Score).ToList();
            //     
            // // Calculate useful fragment percentage
            // int usefulFragmentCount = 0;
            // string bestCombination = "";
            //     
            // if (processedResults.Any())
            // {
            //     var bestResult = processedResults.First();
            //     bestCombination = bestResult.Text;
            //         
            //     // Count fragments used in best combination
            //     foreach (var fragment in validFragments)
            //     {
            //         if (bestCombination.Contains(fragment))
            //         {
            //             usefulFragmentCount++;
            //         }
            //     }
            //         
            //     // Save best result info
            //     result.BestCombination = bestResult.Text;
            //     result.BestCombinationTranslation = bestResult.Translation;
            //     result.IsGrammaticallyValid = bestResult.IsGrammaticallyValid;
            //     result.BestCombinationScore = bestResult.Score;
            //         
            //     if (bestResult.GrammarIssues.Any())
            //     {
            //         result.GrammaticalIssues = bestResult.GrammarIssues;
            //     }
            // }
                
           // result.UsefulFragments = usefulFragmentCount;
            // result.UsefulFragmentPercentage = validFragments.Length > 0 ? 
            //     (double)usefulFragmentCount / array.Fragments.Length * 100 : 0;

            result.UsefulFragmentPercentage = processedResults[0].Score;
        }
        else
        {
            // No valid fragments
            result.UsefulFragments = 0;
            result.UsefulFragmentPercentage = 0;
            result.BestCombinationScore = 0;
            result.GrammaticalIssues = new List<string> { "No valid fragments found" };
        }
            
        // // Calculate overall quality score (weighted average)
        // result.OverallQualityScore = 0.3 * (result.ValidFragmentPercentage / 100) + 
        //                              0.7 * (result.UsefulFragmentPercentage / 100);
        //     
        // // Grade based on useful fragments percentage
        // result.ArrayGrade = AssignGrade(result.UsefulFragmentPercentage);
            
        return result;
    }
        
    /// <summary>
    /// Assign a letter grade based on percentage
    /// </summary>
    private string AssignGrade(double percentage)
    {
        if (percentage >= 90) return "A+";
        else if (percentage >= 85) return "A";
        else if (percentage >= 80) return "A-";
        else if (percentage >= 75) return "B+";
        else if (percentage >= 70) return "B";
        else if (percentage >= 65) return "B-";
        else if (percentage >= 60) return "C+";
        else if (percentage >= 55) return "C";
        else if (percentage >= 50) return "C-";
        else if (percentage >= 45) return "D+";
        else if (percentage >= 40) return "D";
        else if (percentage >= 35) return "D-";
        else return "F";
    }
}