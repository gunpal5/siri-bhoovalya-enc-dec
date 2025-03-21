namespace SiriBhuvalyaExtractor.Extractor;

using System.Diagnostics;
using System.Text;

public class WarnsdorffHamiltonianPathFinder
{
    private readonly ToroidalGridHologram hologram;
    private readonly int[,] gridValues;
    private readonly int rows;
    private readonly int cols;
    private readonly Random random = new Random(42);

    // Updated constraints based on your requirements
    private  int maxConsecutiveVowels = 2;
    private  int maxConsecutiveConsonants = 2;
    private readonly int maxAttempts = 100000;
    int numberOfConsonants => maxConsecutiveConsonants;

    public WarnsdorffHamiltonianPathFinder(ToroidalGridHologram hologram, int[,] gridValues,int maxVowels = 2, int maxConsonants = 2)
    {
        this.hologram = hologram;
        this.gridValues = gridValues;
        this.rows = gridValues.GetLength(0);
        this.cols = gridValues.GetLength(1);
        this.maxConsecutiveVowels = maxVowels;
        this.maxConsecutiveConsonants = maxConsonants;
    }

    public List<Vertex> FindHamiltonianPathWarnsdorff(int timeoutSeconds = 120)
    {
        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Starting Warnsdorff's algorithm search...");

        // Try multiple starting positions
        var random = new Random();
        for (int attempt = 0; attempt < maxAttempts && stopwatch.Elapsed.TotalSeconds < timeoutSeconds; attempt++)
        {
            // Pick a random starting vertex
            Vertex startVertex = hologram.Vertices[random.Next(hologram.Vertices.Count)];

            var path = new List<Vertex> { startVertex };
            var visited = new HashSet<Vertex> { startVertex };

            int consecutiveVowels = IsVowel(startVertex) ? 1 : 0;
            int consecutiveConsonants = IsConsonant(startVertex) ? 1 : 0;

            // Try to build a complete path
            while (path.Count < hologram.Vertices.Count)
            {
                Vertex current = path[path.Count - 1];

                // Get all unvisited neighbors that don't violate constraints
                var validNeighbors = GetAllDiagonalNeighbors(current)
                    .Where(n => !visited.Contains(n))
                    .ToList();

                // Filter out neighbors that would violate consecutive constraints
                validNeighbors = validNeighbors.Where(n =>
                {
                    int newConsecutiveVowels = IsVowel(n) ? (IsVowel(current) ? consecutiveVowels + 1 : 1) : 0;

                    int newConsecutiveConsonants =
                        IsConsonant(n) ? (IsConsonant(current) ? consecutiveConsonants + 1 : 1) : 0;

                    return newConsecutiveVowels <= maxConsecutiveVowels &&
                           newConsecutiveConsonants <= this.numberOfConsonants;
                }).ToList();

                if (validNeighbors.Count == 0)
                {
                    // Dead end, this attempt failed
                    break;
                }

                // Sort neighbors by degree (number of their unvisited neighbors)
                validNeighbors = validNeighbors
                    .OrderBy(n => GetAllDiagonalNeighbors(n).Count(nn => !visited.Contains(nn)))
                    .ToList();

                // Pick the neighbor with fewest unvisited neighbors (Warnsdorff's rule)
                Vertex next = validNeighbors[0];

                // Update path and state
                path.Add(next);
                visited.Add(next);

                // Update consecutive counts
                consecutiveVowels = IsVowel(next) ? (IsVowel(current) ? consecutiveVowels + 1 : 1) : 0;

                consecutiveConsonants = IsConsonant(next) ? (IsConsonant(current) ? consecutiveConsonants + 1 : 1) : 0;

                if (path.Count % 100 == 0)
                {
                    Console.WriteLine(
                        $"Attempt {attempt + 1}: Path length so far: {path.Count}/{hologram.Vertices.Count}");
                }
            }

            // Check if we found a complete path
            if (path.Count == hologram.Vertices.Count)
            {
                Console.WriteLine($"Found Hamiltonian path in {stopwatch.Elapsed.TotalSeconds} seconds!");
                return path;
            }

            Console.WriteLine($"Attempt {attempt + 1} failed, reached {path.Count}/{hologram.Vertices.Count} vertices");
        }

        stopwatch.Stop();
        Console.WriteLine($"Search completed in {stopwatch.Elapsed.TotalSeconds} seconds");
        Console.WriteLine("No Hamiltonian path found");

        return new List<Vertex>();
    }
    
    public List<Vertex> FindHamiltonianCycleWarnsdorff(int timeoutSeconds = 120)
    {
        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Starting Warnsdorff's algorithm search for Hamiltonian cycle...");

        // Try multiple starting positions
        var random = new Random();
        for (int attempt = 0; attempt < maxAttempts && stopwatch.Elapsed.TotalSeconds < timeoutSeconds; attempt++)
        {
            // Pick a random starting vertex
            Vertex startVertex = hologram.Vertices[random.Next(hologram.Vertices.Count)];

            var path = new List<Vertex> { startVertex };
            var visited = new HashSet<Vertex> { startVertex };

            int consecutiveVowels = IsVowel(startVertex) ? 1 : 0;
            int consecutiveConsonants = IsConsonant(startVertex) ? 1 : 0;

            // Try to build a complete path
            while (path.Count < hologram.Vertices.Count)
            {
                Vertex current = path[path.Count - 1];

                // Get all unvisited neighbors that don't violate constraints
                var validNeighbors = GetAllDiagonalNeighbors(current)
                    .Where(n => !visited.Contains(n))
                    .ToList();

                // Filter out neighbors that would violate consecutive constraints
                validNeighbors = validNeighbors.Where(n =>
                {
                    int newConsecutiveVowels = IsVowel(n) ? (IsVowel(current) ? consecutiveVowels + 1 : 1) : 0;

                    int newConsecutiveConsonants =
                        IsConsonant(n) ? (IsConsonant(current) ? consecutiveConsonants + 1 : 1) : 0;

                    return newConsecutiveVowels <= maxConsecutiveVowels &&
                           newConsecutiveConsonants <= this.numberOfConsonants;
                }).ToList();

                // If we're at the second-to-last vertex, we need to check if the last vertex can connect back to start
                if (path.Count == hologram.Vertices.Count - 1)
                {
                    validNeighbors = validNeighbors.Where(n => {
                        // Check if this neighbor can connect back to the starting vertex
                        bool canCompleteCircuit = GetAllDiagonalNeighbors(n).Contains(startVertex);
        
                        // Also check if connecting back won't violate constraints
                        if (canCompleteCircuit)
                        {
                            int nextConsecutiveVowels = IsVowel(n) ? (IsVowel(current) ? consecutiveVowels + 1 : 1) : 0;
                            int nextConsecutiveConsonants = IsConsonant(n) ? (IsConsonant(current) ? consecutiveConsonants + 1 : 1) : 0;
            
                            // Now check if going back to start would maintain constraints
                            int finalConsecutiveVowels = IsVowel(startVertex) ? 
                                (IsVowel(n) ? nextConsecutiveVowels + 1 : 1) : 0;
            
                            int finalConsecutiveConsonants = IsConsonant(startVertex) ? 
                                (IsConsonant(n) ? nextConsecutiveConsonants + 1 : 1) : 0;
            
                            return finalConsecutiveVowels <= maxConsecutiveVowels &&
                                   finalConsecutiveConsonants <= numberOfConsonants;
                        }
        
                        return canCompleteCircuit;
                    }).ToList();
                }


                if (validNeighbors.Count == 0)
                {
                    // Dead end, this attempt failed
                    break;
                }

                // Sort neighbors by degree (number of their unvisited neighbors)
                validNeighbors = validNeighbors
                    .OrderBy(n => GetAllDiagonalNeighbors(n).Count(nn => !visited.Contains(nn)))
                    .ToList();

                // Pick the neighbor with fewest unvisited neighbors (Warnsdorff's rule)
                Vertex next = validNeighbors[0];

                // Update path and state
                path.Add(next);
                visited.Add(next);

                // Update consecutive counts
                consecutiveVowels = IsVowel(next) ? (IsVowel(current) ? consecutiveVowels + 1 : 1) : 0;

                consecutiveConsonants = IsConsonant(next) ? (IsConsonant(current) ? consecutiveConsonants + 1 : 1) : 0;

                if (path.Count % 100 == 0)
                {
                    Console.WriteLine(
                        $"Attempt {attempt + 1}: Path length so far: {path.Count}/{hologram.Vertices.Count}");
                }
            }

            // Check if we found a complete path AND can form a cycle back to the start
            if (path.Count == hologram.Vertices.Count)
            {
                Vertex last = path[path.Count - 1];
                
                // Check if the last vertex can connect back to the first vertex to form a cycle
                if (GetAllDiagonalNeighbors(last).Contains(startVertex))
                {
                    // Also check if constraints are maintained when connecting back to start
                    bool constraintsOk = true;
                    
                    // Check vowel constraint
                    if (IsVowel(startVertex) && IsVowel(last))
                    {
                        int cycleVowels = consecutiveVowels + 1;
                        if (cycleVowels > maxConsecutiveVowels)
                            constraintsOk = false;
                    }
                    
                    // Check consonant constraint
                    if (IsConsonant(startVertex) && IsConsonant(last))
                    {
                        int cycleConsonants = consecutiveConsonants + 1;
                        if (cycleConsonants > maxConsecutiveConsonants)
                            constraintsOk = false;
                    }
                    
                    if (constraintsOk)
                    {
                        Console.WriteLine($"Found Hamiltonian cycle in {stopwatch.Elapsed.TotalSeconds} seconds!");
                        return path;
                    }
                }
                
                Console.WriteLine($"Found complete path but it doesn't form a valid cycle.");
            }

            Console.WriteLine($"Attempt {attempt + 1} failed, reached {path.Count}/{hologram.Vertices.Count} vertices");
        }

        stopwatch.Stop();
        Console.WriteLine($"Search completed in {stopwatch.Elapsed.TotalSeconds} seconds");
        Console.WriteLine("No Hamiltonian cycle found");

        return new List<Vertex>();
    }



    // Get all diagonal neighbors in a toroidal grid
    private List<Vertex> GetAllDiagonalNeighbors(Vertex vertex)
    {
        var neighbors = new List<Vertex>();

        // Assuming the grid dimensions are known
        int width = (int)Math.Sqrt(hologram.Vertices.Count);
        int height = width;

        // Get diagonal neighbors (NE, SE, SW, NW)
        // All 8 directions: N, NE, E, SE, S, SW, W, NW
        int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
        int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

        for (int i = 0; i < 8; i++)
        {
            // Calculate neighbor coordinates with wrapping for toroidal grid
            int newRow = (vertex.Row + dy[i] + height) % height;
            int newCol = (vertex.Col + dx[i] + width) % width;

            // Find the vertex at these coordinates
            var neighbor = hologram.Vertices.FirstOrDefault(v => v.Row == newRow && v.Col == newCol);

            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    // Helper methods for vowel/consonant checking
    private bool IsVowel(Vertex vertex)
    {
        int value = vertex.Value;
        return value >= 1 && value <= 27;
    }

    private bool IsConsonant(Vertex vertex)
    {
        int value = vertex.Value;
        return value >= 28 && value <= 60;
    }

    private bool IsSpecial(Vertex vertex)
    {
        int value = vertex.Value;
        return value >= 61 && value <= 64;
    }

    // Helper method to get path hash
    public string GetPathHash(List<Vertex> path)
    {
        return string.Join("-", path.Select(v => $"{v.Row},{v.Col}"));
    }
}