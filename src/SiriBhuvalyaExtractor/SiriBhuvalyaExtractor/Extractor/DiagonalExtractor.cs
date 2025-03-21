using System.Text;
using SiriBhuvalyaExtractor.Extensions;

namespace SiriBhuvalyaExtractor.Extractor;

public class DiagonalExtractor
{
    public DiagonalExtractor(string inputFile)
    {
        var chakra = File.ReadAllText(inputFile)
            .Split("\r\n, \t".ToArray(), StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Select(int.Parse).ToArray();

        var grid = chakra.To2DArray();

        var dimentions =(uint) grid.GetLength(0);
        List<string> words = new();

        uint k = 5;
        for (uint i = 0; i < dimentions; i++)
        {
            for (uint j = 0; j < dimentions; j++)
            {
                var row = (dimentions - (i + j)) % dimentions;
                var col = (j+k) % dimentions;
                Console.WriteLine($"{row},{col}");
                var val = Constants.devnagri[grid[row, col]];
                words.Add(val);
            }
        }
        
        File.WriteAllText("output/words.txt", string.Join(",", words));
    }
}