using System.Text;

namespace SiriBhuvalyaExtractor.NLP
{
    /// <summary>
    /// Main program - Entry point of the application
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: SanskritFragmentProcessor <dictionary_path> <fragments_directory> <output_path>");
                Console.WriteLine("Or: SanskritFragmentProcessor <dictionary_path> --fragments \"comma,separated,fragments\" <output_path>");
                return;
            }
            
            string dictionaryPath = args[0];
            string outputPath = args[args.Length - 1];
            Directory.CreateDirectory(outputPath);
            try
            {
                // Load dictionary
                Console.WriteLine("Loading Sanskrit dictionary...");
                var dictionary = new SanskritDictionary(dictionaryPath);
                Console.WriteLine($"Loaded {dictionary.WordCount} dictionary entries");
                
                // Load fragment arrays
                List<FragmentArray> fragmentArrays = new List<FragmentArray>();
                
                if (args[1] == "--fragments")
                {
                    // Process fragments from command line
                    string fragmentText = args[2];
                    var fragments = fragmentText
                        .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim())
                        .Where(f => !string.IsNullOrWhiteSpace(f))
                        .ToArray();
                        
                    fragmentArrays.Add(new FragmentArray 
                    { 
                        Id = "command_line_input", 
                        Fragments = fragments 
                    });
                }
                else
                {
                    // Process fragments from directory
                    string fragmentsDirectory = args[1];
                    Console.WriteLine("Loading fragment arrays...");
                    fragmentArrays = LoadFragmentArrays(fragmentsDirectory);
                    Console.WriteLine($"Loaded {fragmentArrays.Count} fragment arrays");
                }
                
                // Create grader
                var grader = new SanskritCorpusGrader(dictionary);
                
                // Grade corpus
                Console.WriteLine("Grading corpus...");
                foreach (var fragment in fragmentArrays)
                {
                    var gradingResult = grader.GradeFragmentArray(fragment);
                    if (gradingResult.UsefulFragmentPercentage > 60)
                    {
                        File.WriteAllText(Path.Combine(outputPath, $"{fragment.Id}_grading_{Math.Round(gradingResult.UsefulFragmentPercentage)}.txt"), $"{string.Join(",",fragment.Fragments)}");
                    }
                }
               
                
                // // Display results
                // Console.WriteLine("\nCorpus Grading Results:");
                // Console.WriteLine($"Total Fragment Arrays: {gradingResult.TotalFragmentArrays}");
                // Console.WriteLine($"Valid Arrays: {gradingResult.ValidFragmentArrays} ({gradingResult.ValidArrayPercentage:F1}%)");
                // Console.WriteLine($"Total Fragments: {gradingResult.TotalFragments}");
                // Console.WriteLine($"Valid Fragments: {gradingResult.ValidFragments} ({gradingResult.ValidFragmentPercentage:F1}%)");
                // Console.WriteLine($"Useful Fragments: {gradingResult.UsefulFragments} ({gradingResult.UsefulFragmentPercentage:F1}%)");
                // Console.WriteLine($"Corpus Grade: {gradingResult.CorpusGrade}");
                //
                // // Save detailed results
                // SaveGradingResults(gradingResult, outputPath);
                //
                // Console.WriteLine($"\nDetailed results saved to: {outputPath}");
                // Console.WriteLine($"Cleanup guide created at: {Path.Combine(outputPath, "corpus_cleanup_guide.txt")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        
        /// <summary>
        /// Load fragment arrays from a directory
        /// </summary>
        private static List<FragmentArray> LoadFragmentArrays(string directory)
        {
            var arrays = new List<FragmentArray>();
            
            foreach (var file in Directory.GetFiles(directory, "*.txt"))
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var lines = File.ReadAllLines(file, Encoding.UTF8);
                    var content = lines[5];

                    var fragments = content
                        .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim())
                        .Where(f => !string.IsNullOrWhiteSpace(f))
                        .ToArray();

                    arrays.Add(new FragmentArray
                    {
                        Id = name,
                        Fragments = fragments
                    });

                    content = lines[7];

                    fragments = content
                        .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim())
                        .Where(f => !string.IsNullOrWhiteSpace(f))
                        .ToArray();

                    arrays.Add(new FragmentArray
                    {
                        Id = name,
                        Fragments = fragments
                    });
                }catch(Exception ex)
                {
                    Console.WriteLine($"Error loading file: {file}");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
            
            return arrays;
        }
        
        /// <summary>
        /// Save grading results to files
        /// </summary>
        private static void SaveGradingResults(CorpusGradingResult result, string outputPath)
        {
            Directory.CreateDirectory(outputPath);
            
            // Save summary
            string summaryPath = Path.Combine(outputPath, "corpus_grading_summary.txt");
            using (var writer = new StreamWriter(summaryPath, false, Encoding.UTF8))
            {
                writer.WriteLine("Sanskrit Corpus Grading Summary");
                writer.WriteLine("==============================");
                writer.WriteLine();
                writer.WriteLine($"Total Fragment Arrays: {result.TotalFragmentArrays}");
                writer.WriteLine($"Valid Arrays: {result.ValidFragmentArrays} ({result.ValidArrayPercentage:F1}%)");
                writer.WriteLine($"Total Fragments: {result.TotalFragments}");
                writer.WriteLine($"Valid Fragments: {result.ValidFragments} ({result.ValidFragmentPercentage:F1}%)");
                writer.WriteLine($"Useful Fragments: {result.UsefulFragments} ({result.UsefulFragmentPercentage:F1}%)");
                writer.WriteLine($"Corpus Grade: {result.CorpusGrade}");
                writer.WriteLine();
                writer.WriteLine("Array Results Overview:");
                writer.WriteLine("----------------------");
                
                foreach (var arrayResult in result.ArrayResults.OrderByDescending(r => r.OverallQualityScore))
                {
                    writer.WriteLine($"Array: {arrayResult.ArrayId}");
                    writer.WriteLine($"  Grade: {arrayResult.ArrayGrade} (Score: {arrayResult.OverallQualityScore:F2})");
                    writer.WriteLine($"  Valid Fragments: {arrayResult.ValidFragmentPercentage:F1}%");
                    writer.WriteLine($"  Useful Fragments: {arrayResult.UsefulFragmentPercentage:F1}%");
                    
                    if (!string.IsNullOrEmpty(arrayResult.BestCombination))
                    {
                        writer.WriteLine($"  Best Combination: {arrayResult.BestCombination}");
                        if (!string.IsNullOrEmpty(arrayResult.BestCombinationTranslation))
                        {
                            writer.WriteLine($"  Translation: {arrayResult.BestCombinationTranslation}");
                        }
                        writer.WriteLine($"  Grammatically Valid: {arrayResult.IsGrammaticallyValid}");
                    }
                    
                    writer.WriteLine();
                }
            }
            
            // Save detailed array results
            foreach (var arrayResult in result.ArrayResults)
            {
                string arrayPath = Path.Combine(outputPath, $"{arrayResult.ArrayId}_grading.txt");
                using (var writer = new StreamWriter(arrayPath, false, Encoding.UTF8))
                {
                    writer.WriteLine($"Grading Results for Array: {arrayResult.ArrayId}");
                    writer.WriteLine("=".PadRight(50, '='));
                    writer.WriteLine();
                    writer.WriteLine($"Total Fragments: {arrayResult.TotalFragments}");
                    writer.WriteLine($"Valid Fragments: {arrayResult.ValidFragments} ({arrayResult.ValidFragmentPercentage:F1}%)");
                    writer.WriteLine($"Useful Fragments: {arrayResult.UsefulFragments} ({arrayResult.UsefulFragmentPercentage:F1}%)");
                    writer.WriteLine($"Overall Quality Score: {arrayResult.OverallQualityScore:F3}");
                    writer.WriteLine($"Array Grade: {arrayResult.ArrayGrade}");
                    writer.WriteLine();
                    
                    if (!string.IsNullOrEmpty(arrayResult.BestCombination))
                    {
                        writer.WriteLine("Best Combination:");
                        writer.WriteLine("-----------------");
                        writer.WriteLine($"Text: {arrayResult.BestCombination}");
                        writer.WriteLine($"Score: {arrayResult.BestCombinationScore:F3}");
                        
                        if (!string.IsNullOrEmpty(arrayResult.BestCombinationTranslation))
                        {
                            writer.WriteLine($"Translation: {arrayResult.BestCombinationTranslation}");
                        }
                        
                        writer.WriteLine($"Grammatically Valid: {arrayResult.IsGrammaticallyValid}");
                        
                        if (arrayResult.GrammaticalIssues.Count > 0)
                        {
                            writer.WriteLine("\nGrammatical Issues:");
                            foreach (var issue in arrayResult.GrammaticalIssues)
                            {
                                writer.WriteLine($"  - {issue}");
                            }
                        }
                        
                        writer.WriteLine();
                    }
                    
                    writer.WriteLine("Fragment Analysis:");
                    writer.WriteLine("-----------------");
                    foreach (var fragment in arrayResult.FragmentQualities.OrderByDescending(f => f.Quality))
                    {
                        writer.WriteLine($"  • \"{fragment.Fragment}\"");
                        writer.WriteLine($"    - Valid: {fragment.IsValid}");
                        writer.WriteLine($"    - Quality: {fragment.Quality:F3}");
                        writer.WriteLine($"    - Used in Best Combination: {arrayResult.BestCombination?.Contains(fragment.Fragment) ?? false}");
                        writer.WriteLine();
                    }
                }
            }
            
            // Save CSV summary for analysis
            string csvPath = Path.Combine(outputPath, "corpus_grading_summary.csv");
            using (var writer = new StreamWriter(csvPath, false, Encoding.UTF8))
            {
                writer.WriteLine("ArrayId,TotalFragments,ValidFragments,ValidFragmentPercentage,UsefulFragments,UsefulFragmentPercentage,OverallQualityScore,ArrayGrade,IsGrammaticallyValid");
                
                foreach (var arrayResult in result.ArrayResults)
                {
                    writer.WriteLine($"{arrayResult.ArrayId},{arrayResult.TotalFragments},{arrayResult.ValidFragments},{arrayResult.ValidFragmentPercentage:F1},{arrayResult.UsefulFragments},{arrayResult.UsefulFragmentPercentage:F1},{arrayResult.OverallQualityScore:F3},{arrayResult.ArrayGrade},{arrayResult.IsGrammaticallyValid}");
                }
            }
            
            // Save cleanup guide
            string cleanupPath = Path.Combine(outputPath, "corpus_cleanup_guide.txt");
            using (var writer = new StreamWriter(cleanupPath, false, Encoding.UTF8))
            {
                writer.WriteLine("Sanskrit Corpus Cleanup Guide");
                writer.WriteLine("============================");
                writer.WriteLine();
                writer.WriteLine("This report identifies problematic fragments that should be removed or corrected.");
                writer.WriteLine();
                
                foreach (var arrayResult in result.ArrayResults)
                {
                    // Only include arrays with issues
                    if (arrayResult.ValidFragmentPercentage < 100 || arrayResult.UsefulFragmentPercentage < 70)
                    {
                        writer.WriteLine($"Array: {arrayResult.ArrayId} (Grade: {arrayResult.ArrayGrade})");
                        
                        // List invalid fragments
                        var invalidFragments = arrayResult.FragmentQualities.Where(f => !f.IsValid).ToList();
                        if (invalidFragments.Any())
                        {
                            writer.WriteLine("  Invalid fragments to remove or correct:");
                            foreach (var fragment in invalidFragments)
                            {
                                writer.WriteLine($"    • \"{fragment.Fragment}\" - Invalid Devanagari structure");
                            }
                        }
                        
                        // List low-quality fragments
                        var lowQualityFragments = arrayResult.FragmentQualities
                            .Where(f => f.IsValid && f.Quality < 0.5)
                            .ToList();
                            
                        if (lowQualityFragments.Any())
                        {
                            writer.WriteLine("  Low quality fragments to review:");
                            foreach (var fragment in lowQualityFragments)
                            {
                                writer.WriteLine($"    • \"{fragment.Fragment}\" - Quality score: {fragment.Quality:F3}");
                            }
                        }
                        
                        // List unused fragments in best combination
                        if (!string.IsNullOrEmpty(arrayResult.BestCombination))
                        {
                            var unusedFragments = arrayResult.FragmentQualities
                                .Where(f => f.IsValid && !arrayResult.BestCombination.Contains(f.Fragment))
                                .ToList();
                                
                            if (unusedFragments.Any())
                            {
                                writer.WriteLine("  Fragments not used in best combination:");
                                foreach (var fragment in unusedFragments)
                                {
                                    writer.WriteLine($"    • \"{fragment.Fragment}\"");
                                }
                            }
                        }
                        
                        writer.WriteLine();
                    }
                }
                
                writer.WriteLine("\nRecommendations for Corpus Cleaning:");
                writer.WriteLine("----------------------------------");
                writer.WriteLine("1. Remove all invalid fragments identified above");
                writer.WriteLine("2. Review low quality fragments and correct if possible");
                writer.WriteLine("3. For arrays with grade D or F, consider removing or splitting the fragment set");
                writer.WriteLine("4. After cleaning, run the grading tool again to verify improvement");
            }
        }
    }
    
    #region Data Models

    #endregion
    
    #region Core Components

    #endregion
}