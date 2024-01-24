using Rummisolver;
using System;
using System.Runtime.CompilerServices;

class Program
{
    public static List<Tile>? AllTiles;
    
    public static bool unlocked = true;
    static void Main()
    {
        AllTiles = new List<Tile>();
        InitializeRummikubStartBoard();

        if (!unlocked)
        {
            FindOpeningPlay(AllTiles);
        }
        else
        {
            List<List<Tile>> legalPlays = FindCombinations(FindGroups(AllTiles), FindSequences(AllTiles), AllTiles);

            // Sort the valid combinations by the count of non-wild tiles in descending order
            legalPlays.Sort((c1, c2) =>
                c2.Count(tile => !tile.IsWild).CompareTo(c1.Count(tile => !tile.IsWild))
            );

            // Get the first combination in the sorted list (if any)
            var firstCombination = legalPlays.FirstOrDefault();

            // Output the valid combination
            if (firstCombination != null)
            {
                Console.WriteLine("Valid Combination:");
                foreach (var tile in firstCombination)
                {
                    Console.WriteLine($"Value: {tile.Value}, Color: {tile.Color}, IsWild: {tile.IsWild}, Id: {tile.Id}");
                    Console.WriteLine("______________________________________________________________________________________________");
                    Console.WriteLine("Do u want to use this? (Y or N)");
                    string answer =  Convert.ToString(Console.ReadKey()).ToLower();
                    if (answer == "y"||  answer == "n")
                    {
                        if(answer == "y")
                        {
                            AllTiles.Clear();
                            foreach (var usedTile in firstCombination)
                            {
                                usedTile.IsWild = true;
                                AllTiles.Add(usedTile);
                                
                            }
                        }
                        else
                        {
                            
                        }
                       
                    } 
                }
            }
            else
            {
                Console.WriteLine("No valid combination found");
            }
        }
    }



    public static List<Tile> FindOpeningPlay(List<Tile> AvalibleTiles)
    {
        List<List<Tile>> possibleCombinations = new List<List<Tile>>();

        // Sort the available tiles by value, then by color
        var sortedTiles = AvalibleTiles.OrderBy(t => t.Value).ThenBy(t => t.Color).ToList();

        // Iterate over unique values to find groups of the same value with a diffrent color foreach tile
        foreach (var uniqueValue in sortedTiles.Select(t => t.Value).Distinct())
        {
            List<Tile> matchingNumbers = new List<Tile>();

            // Add tiles with the current value, each with a unique color
            foreach (var tile in sortedTiles.Where(t => t.Value == uniqueValue))
            {
                if (!matchingNumbers.Any(t => t.Color == tile.Color))
                {
                    matchingNumbers.Add(tile);
                }
            }

            // Check if the sum of values in the list is above 30 and more than 3 tiles, speeds up the logic by preventing unnecessary checks
            if (matchingNumbers.Count >= 3 || matchingNumbers.Sum(t => t.Value) >= 30)
            {
                possibleCombinations.Add(matchingNumbers.ToList());
            }
        }
        possibleCombinations.AddRange(FindSequences(AvalibleTiles));


        // Find the combination with the highest count
        var bestCombination = possibleCombinations
            .OrderByDescending(combination => combination.Count)
            .FirstOrDefault();

        if (bestCombination != null)
        {
            Console.WriteLine("Best Combination:");
            foreach (var tile in bestCombination)
            {
                Console.WriteLine($"Value: {tile.Value}, Color: {tile.Color}, Joker: {tile.Joker}");
            }
        }
        else
        {
            Console.WriteLine("No possible opening play");
        }

        // Return the list of tiles from the best combination
        return bestCombination ?? new List<Tile>();
    }

    public static List<List<Tile>> FindGroups(List<Tile> AvalibleTiles)
    {
        List<List<Tile>> possibleCombinations = new List<List<Tile>>();

        // Sort the available tiles by value, then by color
        var sortedTiles = AvalibleTiles.OrderBy(t => t.Value).ThenBy(t => t.Color).ToList();

        // Iterate over unique values to find groups of the same value with a diffrent color foreach tile
        foreach (var uniqueValue in sortedTiles.Select(t => t.Value).Distinct())
        {
            List<Tile> matchingNumbers = new List<Tile>();

            // Add tiles with the current value, each with a unique color
            foreach (var tile in sortedTiles.Where(t => t.Value == uniqueValue))
            {
                if (!matchingNumbers.Any(t => t.Color == tile.Color))
                {
                    matchingNumbers.Add(tile);
                }
            }

            // Check if the sum of values in the list is above 30 and more than 3 tiles, speeds up the logic by preventing unnecessary checks
            if (matchingNumbers.Count >= 3)
            {
                possibleCombinations.Add(matchingNumbers.ToList());
            }
        }
        possibleCombinations.AddRange(FindSequences(AvalibleTiles));


        // Find the combination with the highest count
       possibleCombinations
            .OrderByDescending(combination => combination.Count);




        // Return the list of tiles from the best combination
        return possibleCombinations;
    }






    private static List<List<Tile>> FindSequences(List<Tile> tiles)
    {
        // Sort the tiles by color and then by value for better grouping
        var sortedTiles = tiles.OrderBy(t => t.Color).ThenBy(t => t.Value).ToList();

        List<List<Tile>> sequences = new List<List<Tile>>();
        List<Tile> currentSequence = new List<Tile>();

        foreach (var tile in sortedTiles)
        {
            if (currentSequence.Count == 0 || (tile.Value == currentSequence.Last().Value + 1 && tile.Color == currentSequence.Last().Color))
            {
                currentSequence.Add(tile);
            }
            else
            {
                // Check if the current sequence forms a valid sequence
                if (currentSequence.Count >= 3)
                {
                    sequences.Add(currentSequence.ToList());
                }

                currentSequence.Clear();
                currentSequence.Add(tile);
            }
        }

        // Check if the last sequence should be added
        if (currentSequence.Count >= 3)
        {
            sequences.Add(currentSequence.ToList());
        }

        return sequences;
    }


    public static List<List<Tile>> FindCombinations(List<List<Tile>> sequences, List<List<Tile>> groups, List<Tile> allTiles)
    {
        List<List<Tile>> allPossibleSets = new List<List<Tile>>();
        allPossibleSets.AddRange(sequences);
        allPossibleSets.AddRange(groups);

        List<List<Tile>> validCombinations = new List<List<Tile>>();

        FindCombinationsRecursive(allPossibleSets, 0, new List<Tile>(), allTiles, validCombinations);

        return validCombinations;
    }

    private static void FindCombinationsRecursive(List<List<Tile>> possibleSets, int index, List<Tile> currentCombination, List<Tile> allTiles, List<List<Tile>> validCombinations)
    {
        if (index == possibleSets.Count)
        {
            // Check if all tiles that are marked as wild are used
            if (allTiles.Where(tile => tile.IsWild).All(tile => currentCombination.Contains(tile)) &&
                currentCombination.Distinct().Count() == currentCombination.Count)
            {
                validCombinations.Add(new List<Tile>(currentCombination));
            }
            return;
        }

        // Explore including the current set (both wild and non-wild tiles)
        FindCombinationsRecursive(
            possibleSets,
            index + 1,
            currentCombination.Concat(possibleSets[index]).ToList(),
            allTiles,
            validCombinations
        );

        // Explore excluding the current set
        FindCombinationsRecursive(
            possibleSets,
            index + 1,
            currentCombination,
            allTiles,
            validCombinations
        );
    }

    public static void grabTile()
    {
        Console.WriteLine("Grab a tile from the pile...");
        Console.WriteLine("Enter the tiles color:");
        string color = Console.ReadLine().ToLower();

    }



    public static void InitializeRummikubStartBoard()
    {
        //wild
        AllTiles.Add(new Tile { Value = 5, Color = "red", IsWild = true, Id = 1 });
        AllTiles.Add(new Tile { Value = 5, Color = "blue", IsWild = true, Id = 2 });
        AllTiles.Add(new Tile { Value = 5, Color = "green", IsWild = true, Id = 3 });

        AllTiles.Add(new Tile { Value = 8, Color = "red", IsWild = true, Id = 4 });
        AllTiles.Add(new Tile { Value = 8, Color = "blue", IsWild = true, Id = 5 });
        AllTiles.Add(new Tile { Value = 8, Color = "green", IsWild = true, Id = 6  });

        // Board tiles
        AllTiles.Add(new Tile { Value = 7, Color = "red", IsWild = false, Id = 7 });
        AllTiles.Add(new Tile { Value = 8, Color = "red", IsWild = false, Id = 8 });
        AllTiles.Add(new Tile { Value = 9, Color = "red", IsWild = false, Id = 9 });

        AllTiles.Add(new Tile { Value = 9, Color = "red", IsWild = false, Id = 10 });
        AllTiles.Add(new Tile { Value = 9, Color = "blue", IsWild = false, Id = 11 });

        AllTiles.Add(new Tile { Value = 10, Color = "red", IsWild = false, Id = 12 });
        AllTiles.Add(new Tile { Value = 10, Color = "blue", IsWild = false, Id = 13 });
        AllTiles.Add(new Tile { Value = 10, Color = "green", IsWild = false, Id = 14 });

    }
}
