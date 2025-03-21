using System.Diagnostics;

namespace SiriBhuvalyaExtractor.Extractor;

public class GeneticAlgorithm
{
    private readonly ToroidalGridHologram hologram;
    private readonly int[,] gridValues;
    private readonly int rows;
    private readonly int cols;
    private readonly Random random = new Random(42);

    // Updated constraints based on your requirements
    private int maxConsecutiveVowels = 2;
    private int maxConsecutiveConsonants = 2;
    private readonly int maxAttempts = 100000;
    int numberOfConsonants => maxConsecutiveConsonants;

    public GeneticAlgorithm(ToroidalGridHologram hologram, int[,] gridValues, int maxVowels = 2, int maxConsonants = 2)
    {
        this.hologram = hologram;
        this.gridValues = gridValues;
        this.rows = gridValues.GetLength(0);
        this.cols = gridValues.GetLength(1);
        this.maxConsecutiveVowels = maxVowels;
        this.maxConsecutiveConsonants = maxConsonants;
    }

    public List<Vertex> FindHamiltonianPathGenetic(int timeoutSeconds = 120, int populationSize = 100,
        int generations = 1000)
    {
        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Starting genetic algorithm search for Hamiltonian path...");


        // Pre-compute and cache neighbor information and vertex types
        var neighborCache = new Dictionary<Vertex, List<Vertex>>();
        var isVowelCache = new Dictionary<Vertex, bool>();
        var isConsonantCache = new Dictionary<Vertex, bool>();

        foreach (var vertex in hologram.Vertices)
        {
            neighborCache[vertex] = GetAllDiagonalNeighbors(vertex).ToList();
            isVowelCache[vertex] = IsVowel(vertex);
            isConsonantCache[vertex] = IsConsonant(vertex);
        }

        var random = new Random();
        var totalVertices = hologram.Vertices.Count;
        var vertexList = hologram.Vertices.ToList();

        // Create initial population with greedy path construction
        var population = new List<List<Vertex>>(populationSize);
        var bestPath = new List<Vertex>();
        var bestFitness = 0;

        // Create initial population using greedy path construction from different starting points
        Parallel.For(0, populationSize, i =>
        {
            if (stopwatch.Elapsed.TotalSeconds >= timeoutSeconds)
                return;

            var localRandom = new Random(random.Next());
            var startVertex = vertexList[localRandom.Next(totalVertices)];
            var individual = ConstructGreedyPath(startVertex, neighborCache, isVowelCache, isConsonantCache);

            lock (population)
            {
                population.Add(individual);

                // Update best path if this one is better
                if (individual.Count > bestFitness)
                {
                    bestFitness = individual.Count;
                    bestPath = new List<Vertex>(individual);
                    Console.WriteLine($"New best path found: {bestFitness}/{totalVertices} vertices");

                    if (bestFitness == totalVertices)
                    {
                        Console.WriteLine(
                            $"Found complete Hamiltonian path in {stopwatch.Elapsed.TotalSeconds} seconds!");
                    }
                }
            }
        });

        // Main genetic algorithm loop
        for (int gen = 0; gen < generations && stopwatch.Elapsed.TotalSeconds < timeoutSeconds; gen++)
        {
            // If we've found a complete path, we can stop
            if (bestFitness == totalVertices)
                break;

            var newPopulation = new List<List<Vertex>>(populationSize);

            // Elitism - keep best individuals
            var eliteCount = Math.Max(1, populationSize / 10); // 10% elites
            var sortedPopulation = population
                .OrderByDescending(p => p.Count)
                .ThenByDescending(p => p.Count > 0 ? CalculatePathPotential(p, neighborCache) : 0)
                .ToList();

            newPopulation.AddRange(sortedPopulation.Take(eliteCount));

            // Fill rest of population with crossovers and mutations
            Parallel.For(eliteCount, populationSize, i =>
            {
                if (stopwatch.Elapsed.TotalSeconds >= timeoutSeconds)
                    return;

                var localRandom = new Random(random.Next());
                List<Vertex> newIndividual;

                if (localRandom.NextDouble() < 0.7) // 70% chance of crossover
                {
                    // Tournament selection
                    var parent1 = TournamentSelect(sortedPopulation, localRandom);
                    var parent2 = TournamentSelect(sortedPopulation, localRandom);

                    // Path-preserving crossover
                    newIndividual = CrossoverPaths(parent1, parent2, neighborCache, isVowelCache, isConsonantCache,
                        localRandom);
                }
                else
                {
                    // Mutation
                    var parent = TournamentSelect(sortedPopulation, localRandom);
                    newIndividual = MutatePath(parent, neighborCache, isVowelCache, isConsonantCache, localRandom);
                }

                // Apply local search to improve path
                newIndividual = LocalSearch(newIndividual, neighborCache, isVowelCache, isConsonantCache, localRandom);

                lock (newPopulation)
                {
                    newPopulation.Add(newIndividual);

                    // Update best path if this one is better
                    if (newIndividual.Count > bestFitness)
                    {
                        bestFitness = newIndividual.Count;
                        bestPath = new List<Vertex>(newIndividual);
                        Console.WriteLine(
                            $"Generation {gen}: New best path found with {bestFitness}/{totalVertices} vertices");

                        if (bestFitness == totalVertices)
                        {
                            Console.WriteLine(
                                $"Found complete Hamiltonian path in {stopwatch.Elapsed.TotalSeconds} seconds!");
                        }
                    }
                }
            });

            population = newPopulation;

            if (gen % 10 == 0)
            {
                var averageLength = population.Average(p => p.Count);
                Console.WriteLine($"Generation {gen}: Best = {bestFitness}, Avg = {averageLength:F1}");
            }

            // Introduce fresh blood occasionally to maintain diversity
            if (gen % 50 == 49)
            {
                int refreshCount = populationSize / 10; // Refresh 10% of population
                for (int i = 0; i < refreshCount; i++)
                {
                    var startVertex = vertexList[random.Next(totalVertices)];
                    population[populationSize - 1 - i] =
                        ConstructGreedyPath(startVertex, neighborCache, isVowelCache, isConsonantCache);
                }
            }
        }

        stopwatch.Stop();
        Console.WriteLine($"Search completed in {stopwatch.Elapsed.TotalSeconds} seconds");
        Console.WriteLine($"Best path found: {bestPath.Count}/{totalVertices} vertices");

        return bestPath;

        // Helper methods for the genetic algorithm

        // Construct a path greedily from a start vertex
        List<Vertex> ConstructGreedyPath(Vertex start, Dictionary<Vertex, List<Vertex>> neighborCache,
            Dictionary<Vertex, bool> isVowelCache, Dictionary<Vertex, bool> isConsonantCache)
        {
            var path = new List<Vertex> { start };
            var visited = new HashSet<Vertex> { start };

            int consecutiveVowels = isVowelCache[start] ? 1 : 0;
            int consecutiveConsonants = isConsonantCache[start] ? 1 : 0;

            while (path.Count < totalVertices)
            {
                var current = path[path.Count - 1];
                var validNeighbors = new List<Vertex>();

                foreach (var neighbor in neighborCache[current])
                {
                    if (visited.Contains(neighbor))
                        continue;

                    int newConsecutiveVowels =
                        isVowelCache[neighbor] ? (isVowelCache[current] ? consecutiveVowels + 1 : 1) : 0;

                    int newConsecutiveConsonants = isConsonantCache[neighbor]
                        ? (isConsonantCache[current] ? consecutiveConsonants + 1 : 1)
                        : 0;

                    if (newConsecutiveVowels <= maxConsecutiveVowels &&
                        newConsecutiveConsonants <= numberOfConsonants)
                    {
                        validNeighbors.Add(neighbor);
                    }
                }

                if (validNeighbors.Count == 0)
                    break;

                // Add some randomness to greedy selection
                var localRandom = new Random();
                Vertex next;

                if (localRandom.NextDouble() < 0.9) // 90% greedy selection
                {
                    // Use Warnsdorff's heuristic
                    next = validNeighbors.OrderBy(n =>
                        neighborCache[n].Count(nn => !visited.Contains(nn))
                    ).First();
                }
                else // 10% random selection
                {
                    next = validNeighbors[localRandom.Next(validNeighbors.Count)];
                }

                path.Add(next);
                visited.Add(next);

                // Update consecutive counts
                consecutiveVowels = isVowelCache[next] ? (isVowelCache[current] ? consecutiveVowels + 1 : 1) : 0;

                consecutiveConsonants = isConsonantCache[next]
                    ? (isConsonantCache[current] ? consecutiveConsonants + 1 : 1)
                    : 0;
            }

            return path;
        }

        // Tournament selection
        List<Vertex> TournamentSelect(List<List<Vertex>> population, Random random)
        {
            int tournamentSize = 5;
            var bestCandidate = population[random.Next(population.Count)];

            for (int i = 1; i < tournamentSize; i++)
            {
                var candidate = population[random.Next(population.Count)];
                if (candidate.Count > bestCandidate.Count ||
                    (candidate.Count == bestCandidate.Count &&
                     CalculatePathPotential(candidate, neighborCache) >
                     CalculatePathPotential(bestCandidate, neighborCache)))
                {
                    bestCandidate = candidate;
                }
            }

            return bestCandidate;
        }

        // Calculate path potential (how likely it is to be extended)
        int CalculatePathPotential(List<Vertex> path, Dictionary<Vertex, List<Vertex>> neighborCache)
        {
            if (path.Count == 0)
                return 0;

            var lastVertex = path[path.Count - 1];
            var visited = new HashSet<Vertex>(path);

            return neighborCache[lastVertex].Count(n => !visited.Contains(n));
        }

        // Path-preserving crossover
        List<Vertex> CrossoverPaths(List<Vertex> parent1, List<Vertex> parent2,
            Dictionary<Vertex, List<Vertex>> neighborCache,
            Dictionary<Vertex, bool> isVowelCache,
            Dictionary<Vertex, bool> isConsonantCache,
            Random random)
        {
            // If either parent is empty, return the other
            if (parent1.Count == 0) return new List<Vertex>(parent2);
            if (parent2.Count == 0) return new List<Vertex>(parent1);

            // Find common vertices between paths
            var commonVertices = parent1.Intersect(parent2).ToList();
            if (commonVertices.Count == 0)
            {
                // No common vertices, return the better parent
                return parent1.Count >= parent2.Count ? new List<Vertex>(parent1) : new List<Vertex>(parent2);
            }

            // Choose a random common vertex as crossover point
            var crossoverVertex = commonVertices[random.Next(commonVertices.Count)];
            var index1 = parent1.IndexOf(crossoverVertex);
            var index2 = parent2.IndexOf(crossoverVertex);

            // Create child by combining first part of parent1 with second part of parent2
            var child = new List<Vertex>();
            var visited = new HashSet<Vertex>();

            // Add first part from parent1
            for (int i = 0; i <= index1; i++)
            {
                if (!visited.Contains(parent1[i]))
                {
                    child.Add(parent1[i]);
                    visited.Add(parent1[i]);
                }
            }

            // Try to add second part from parent2
            for (int i = index2 + 1; i < parent2.Count; i++)
            {
                if (!visited.Contains(parent2[i]))
                {
                    // Check constraints
                    if (child.Count > 0)
                    {
                        var prev = child[child.Count - 1];
                        var next = parent2[i];

                        // Check if they're neighbors
                        if (!neighborCache[prev].Contains(next))
                            continue;

                        // Check vowel/consonant constraints
                        int consecutiveVowels = 0;
                        int consecutiveConsonants = 0;

                        // Calculate current consecutive counts at the end of child
                        for (int j = Math.Max(0, child.Count - maxConsecutiveVowels); j < child.Count; j++)
                        {
                            if (isVowelCache[child[j]])
                            {
                                consecutiveVowels++;
                            }
                            else
                            {
                                consecutiveVowels = 0;
                            }
                        }

                        for (int j = Math.Max(0, child.Count - numberOfConsonants); j < child.Count; j++)
                        {
                            if (isConsonantCache[child[j]])
                            {
                                consecutiveConsonants++;
                            }
                            else
                            {
                                consecutiveConsonants = 0;
                            }
                        }

                        // Check if adding next would violate constraints
                        if ((isVowelCache[next] && consecutiveVowels + 1 > maxConsecutiveVowels) ||
                            (isConsonantCache[next] && consecutiveConsonants + 1 > numberOfConsonants))
                        {
                            continue;
                        }
                    }

                    child.Add(parent2[i]);
                    visited.Add(parent2[i]);
                }
            }

            return child;
        }

        // Mutate path
        List<Vertex> MutatePath(List<Vertex> path, Dictionary<Vertex, List<Vertex>> neighborCache,
            Dictionary<Vertex, bool> isVowelCache, Dictionary<Vertex, bool> isConsonantCache,
            Random random)
        {
            if (path.Count <= 1)
                return path;

            var result = new List<Vertex>(path);

            // Choose mutation type based on path length
            double r = random.NextDouble();

            if (r < 0.4) // 40% chance: Try to extend the path
            {
                var lastVertex = result[result.Count - 1];
                var visited = new HashSet<Vertex>(result);

                // Find valid extensions
                var validExtensions = neighborCache[lastVertex]
                    .Where(n => !visited.Contains(n))
                    .ToList();

                if (validExtensions.Count > 0)
                {
                    // Apply constraint checks
                    var current = lastVertex;
                    int consecutiveVowels = 0;
                    int consecutiveConsonants = 0;

                    // Calculate current consecutive counts at the end
                    for (int i = Math.Max(0, result.Count - maxConsecutiveVowels); i < result.Count; i++)
                    {
                        if (isVowelCache[result[i]])
                        {
                            if (i > 0 && isVowelCache[result[i - 1]])
                                consecutiveVowels++;
                            else
                                consecutiveVowels = 1;
                        }
                        else
                        {
                            consecutiveVowels = 0;
                        }
                    }

                    for (int i = Math.Max(0, result.Count - numberOfConsonants); i < result.Count; i++)
                    {
                        if (isConsonantCache[result[i]])
                        {
                            if (i > 0 && isConsonantCache[result[i - 1]])
                                consecutiveConsonants++;
                            else
                                consecutiveConsonants = 1;
                        }
                        else
                        {
                            consecutiveConsonants = 0;
                        }
                    }

                    // Filter by constraints
                    var constraintValidExtensions = validExtensions.Where(v =>
                    {
                        int newConsecutiveVowels =
                            isVowelCache[v] ? (isVowelCache[current] ? consecutiveVowels + 1 : 1) : 0;

                        int newConsecutiveConsonants = isConsonantCache[v]
                            ? (isConsonantCache[current] ? consecutiveConsonants + 1 : 1)
                            : 0;

                        return newConsecutiveVowels <= maxConsecutiveVowels &&
                               newConsecutiveConsonants <= numberOfConsonants;
                    }).ToList();

                    if (constraintValidExtensions.Count > 0)
                    {
                        var extension = constraintValidExtensions[random.Next(constraintValidExtensions.Count)];
                        result.Add(extension);
                    }
                }
            }
            else if (r < 0.7) // 30% chance: Truncate and regrow
            {
                // Cut off some portion of the path and regrow
                int cutPoint = Math.Max(1, random.Next(result.Count / 2, result.Count));
                result = result.Take(cutPoint).ToList();

                // Regrow from the truncation point
                var visited = new HashSet<Vertex>(result);
                int consecutiveVowels = 0;
                int consecutiveConsonants = 0;

                // Calculate current consecutive counts at the end
                var current = result[result.Count - 1];
                for (int i = Math.Max(0, result.Count - maxConsecutiveVowels); i < result.Count; i++)
                {
                    if (isVowelCache[result[i]])
                    {
                        if (i > 0 && isVowelCache[result[i - 1]])
                            consecutiveVowels++;
                        else
                            consecutiveVowels = 1;
                    }
                    else
                    {
                        consecutiveVowels = 0;
                    }
                }

                for (int i = Math.Max(0, result.Count - numberOfConsonants); i < result.Count; i++)
                {
                    if (isConsonantCache[result[i]])
                    {
                        if (i > 0 && isConsonantCache[result[i - 1]])
                            consecutiveConsonants++;
                        else
                            consecutiveConsonants = 1;
                    }
                    else
                    {
                        consecutiveConsonants = 0;
                    }
                }

                // Greedily extend path with warnsdorff's heuristic
                while (result.Count < totalVertices && visited.Count < totalVertices)
                {
                    current = result[result.Count - 1];
                    var validNeighbors = neighborCache[current]
                        .Where(n => !visited.Contains(n))
                        .ToList();

                    // Filter by constraints
                    var constraintValidNeighbors = validNeighbors.Where(v =>
                    {
                        int newConsecutiveVowels =
                            isVowelCache[v] ? (isVowelCache[current] ? consecutiveVowels + 1 : 1) : 0;

                        int newConsecutiveConsonants = isConsonantCache[v]
                            ? (isConsonantCache[current] ? consecutiveConsonants + 1 : 1)
                            : 0;

                        return newConsecutiveVowels <= maxConsecutiveVowels &&
                               newConsecutiveConsonants <= numberOfConsonants;
                    }).ToList();

                    if (constraintValidNeighbors.Count == 0)
                        break;

                    // Apply Warnsdorff's heuristic with some randomization
                    Vertex next;
                    if (random.NextDouble() < 0.8) // 80% use heuristic
                    {
                        next = constraintValidNeighbors
                            .OrderBy(n => neighborCache[n].Count(nn => !visited.Contains(nn)))
                            .First();
                    }
                    else // 20% random choice
                    {
                        next = constraintValidNeighbors[random.Next(constraintValidNeighbors.Count)];
                    }

                    result.Add(next);
                    visited.Add(next);

                    // Update consecutive counts
                    consecutiveVowels = isVowelCache[next] ? (isVowelCache[current] ? consecutiveVowels + 1 : 1) : 0;

                    consecutiveConsonants = isConsonantCache[next]
                        ? (isConsonantCache[current] ? consecutiveConsonants + 1 : 1)
                        : 0;
                }
            }
            else // 30% chance: Change starting point
            {
                // Start from a different random vertex
                var startVertex = vertexList[random.Next(vertexList.Count)];
                result = ConstructGreedyPath(startVertex, neighborCache, isVowelCache, isConsonantCache);
            }

            return result;
        }

        // Local search to try to improve path
        List<Vertex> LocalSearch(List<Vertex> path, Dictionary<Vertex, List<Vertex>> neighborCache,
            Dictionary<Vertex, bool> isVowelCache, Dictionary<Vertex, bool> isConsonantCache,
            Random random)
        {
            if (path.Count <= 1)
                return path;

            var result = new List<Vertex>(path);
            var visited = new HashSet<Vertex>(path);

            // Try to extend path if it's not complete
            if (path.Count < totalVertices)
            {
                var current = result[result.Count - 1];
                int consecutiveVowels = 0;
                int consecutiveConsonants = 0;

                // Calculate current consecutive counts at the end
                for (int i = Math.Max(0, result.Count - maxConsecutiveVowels); i < result.Count; i++)
                {
                    if (isVowelCache[result[i]])
                    {
                        if (i > 0 && isVowelCache[result[i - 1]])
                            consecutiveVowels++;
                        else
                            consecutiveVowels = 1;
                    }
                    else
                    {
                        consecutiveVowels = 0;
                    }
                }

                for (int i = Math.Max(0, result.Count - numberOfConsonants); i < result.Count; i++)
                {
                    if (isConsonantCache[result[i]])
                    {
                        if (i > 0 && isConsonantCache[result[i - 1]])
                            consecutiveConsonants++;
                        else
                            consecutiveConsonants = 1;
                    }
                    else
                    {
                        consecutiveConsonants = 0;
                    }
                }

                // Try to extend with backtracking when hitting dead ends
                var extensionAttempts =
                    Math.Min(20, totalVertices - path.Count); // Limit attempts to avoid excessive time

                for (int attempt = 0; attempt < extensionAttempts; attempt++)
                {
                    current = result[result.Count - 1];
                    var validNeighbors = neighborCache[current]
                        .Where(n => !visited.Contains(n))
                        .ToList();

                    // Filter by constraints
                    var constraintValidNeighbors = validNeighbors.Where(v =>
                    {
                        int newConsecutiveVowels =
                            isVowelCache[v] ? (isVowelCache[current] ? consecutiveVowels + 1 : 1) : 0;

                        int newConsecutiveConsonants = isConsonantCache[v]
                            ? (isConsonantCache[current] ? consecutiveConsonants + 1 : 1)
                            : 0;

                        return newConsecutiveVowels <= maxConsecutiveVowels &&
                               newConsecutiveConsonants <= numberOfConsonants;
                    }).ToList();

                    if (constraintValidNeighbors.Count == 0)
                    {
                        // Dead end - backtrack if possible
                        if (result.Count <= 1)
                            break;

                        visited.Remove(result[result.Count - 1]);
                        result.RemoveAt(result.Count - 1);

                        // Recalculate consecutive counts
                        if (result.Count > 0)
                        {
                            current = result[result.Count - 1];
                            consecutiveVowels = 0;
                            consecutiveConsonants = 0;

                            for (int i = Math.Max(0, result.Count - maxConsecutiveVowels); i < result.Count; i++)
                            {
                                if (isVowelCache[result[i]])
                                {
                                    if (i > 0 && isVowelCache[result[i - 1]])
                                        consecutiveVowels++;
                                    else
                                        consecutiveVowels = 1;
                                }
                                else
                                {
                                    consecutiveVowels = 0;
                                }
                            }

                            for (int i = Math.Max(0, result.Count - numberOfConsonants); i < result.Count; i++)
                            {
                                if (isConsonantCache[result[i]])
                                {
                                    if (i > 0 && isConsonantCache[result[i - 1]])
                                        consecutiveConsonants++;
                                    else
                                        consecutiveConsonants = 1;
                                }
                                else
                                {
                                    consecutiveConsonants = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Choose next vertex with Warnsdorff's heuristic
                        Vertex next = constraintValidNeighbors
                            .OrderBy(n => neighborCache[n].Count(nn => !visited.Contains(nn)))
                            .First();

                        result.Add(next);
                        visited.Add(next);

                        // Update consecutive counts
                        consecutiveVowels =
                            isVowelCache[next] ? (isVowelCache[current] ? consecutiveVowels + 1 : 1) : 0;

                        consecutiveConsonants = isConsonantCache[next]
                            ? (isConsonantCache[current] ? consecutiveConsonants + 1 : 1)
                            : 0;
                    }
                }
            }

            return result;
        }
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