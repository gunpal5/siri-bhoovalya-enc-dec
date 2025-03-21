using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using SiriBhuvalyaExtractor.Extractor;


var rootCommand = new RootCommand(
    "Create Codes for OpenRouter Models")
{
    new Option<string>(
        ["--input-file", "-i"], "Input Chakra File (comma separated or tab separated)"),
    new Option<string>(
        ["--output", "-o"], getDefaultValue: () => "output", "Output directory"),
    new Option<int>(
        ["--vowel-count", "-v"], getDefaultValue: () => 2, "optional: number of vowels in the path: default: 2"),
    new Option<int>(
        ["--consonants-count", "-c"], getDefaultValue: () => 3, "optional: number of vowels in the path. default: 3"),
    new Option<string>(
        ["--file-prefix", "-f"], getDefaultValue: () => "sample", "optional: output file prefix. default: \"sample\""),
    new Option<bool>(
    ["--cycle", "-s"], getDefaultValue: () => false, "optional: Return Hamiltonian Cycle. default: false")
};

rootCommand.Handler = CommandHandler.Create(async (string inputFile, string output,int vowelCount,int consonantsCount,string filePrefix, bool cycle) =>
{
    //var d = new DiagonalExtractor(inputFile);
     var chaktraPathFinder = new ChakraPathFinder();
    //
     await chaktraPathFinder.FindPath(inputFile,output,vowelCount,consonantsCount,filePrefix,cycle).ConfigureAwait(false);
});

return await rootCommand.InvokeAsync(args).ConfigureAwait(false);