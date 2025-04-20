using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder
{
    public List<List<BoardTile>> FindPaths(BoardTile startTile, int steps)
    {
        List<List<BoardTile>> allPathsFound = new List<List<BoardTile>>();

        if (startTile == null)
        {
            Debug.LogError("Start tile is null. Cannot find paths.");
            return allPathsFound; // Return empty list
        }

        if (steps < 0)
        {
            Debug.LogWarning("Cannot find paths with negative steps. Returning empty list.");
            return allPathsFound; // Return empty list
        }

        if (steps == 0)
        {
            // A path of 0 steps is just the starting tile itself.
            allPathsFound.Add(new List<BoardTile> { startTile });
            return allPathsFound;
        }

        // --- Initiate the Recursive Search ---
        // The initial path contains only the starting tile.
        List<BoardTile> initialPath = new List<BoardTile> { startTile };
        FindPathsRecursive(startTile, initialPath, steps, allPathsFound);

        return allPathsFound;
    }

    private void FindPathsRecursive(BoardTile currentTile, List<BoardTile> currentPath, int stepsRemaining, List<List<BoardTile>> allPathsFound)
    {
        // --- Base Case: Path Completed ---
        // If no more steps are needed, this path is complete.
        if (stepsRemaining == 0)
        {
            // Add a *copy* of the current path to the results.
            // Important to copy, otherwise subsequent modifications would affect the stored path.
            allPathsFound.Add(new List<BoardTile>(currentPath));
            return; // Stop exploring this branch further
        }

        // --- Determine the tile we just came from (to prevent immediate U-turns) ---
        // If the path has 2 or more tiles, the previous tile is the second-to-last one.
        BoardTile previousTile = (currentPath.Count >= 2) ? currentPath[currentPath.Count - 2] : null;

        // --- Gather Potential Next Moves (Neighbors) ---
        List<BoardTile> potentialNeighbors = new List<BoardTile>();

        // Add forward connections (nextTiles)
        if (currentTile.nextTiles != null)
        {
            potentialNeighbors.AddRange(currentTile.nextTiles);
        }

        // Add backward connections (prevTiles)
        // Use the ReadOnly Property or iterate through GetPrevTile if you implemented that
        // Assuming 'PreviousTiles' property exists as suggested before:
        if (currentTile.PreviousTiles != null) // Check the property exists
        {
            potentialNeighbors.AddRange(currentTile.PreviousTiles);
        }
        // --- If you only have GetPrevTile(index), you'd need a way to know the count ---
        // Example: Assuming a hypothetical GetPrevTileCount() method exists
        // int prevCount = currentTile.GetPrevTileCount();
        // for(int i = 0; i < prevCount; i++) {
        //     BoardTile prev = currentTile.GetPrevTile(i);
        //     if (prev != null) potentialNeighbors.Add(prev);
        // }


        // --- Recursive Step: Explore Each Valid Neighbor ---
        foreach (BoardTile neighbor in potentialNeighbors)
        {
            // Skip null connections in the lists
            if (neighbor == null)
            {
                continue;
            }

            // *** The "No Doubling Back" Rule ***
            // If the potential neighbor is the tile we just came from, skip it.
            if (neighbor == previousTile)
            {
                continue; // Don't immediately go back
            }

            // If the move is valid:
            // 1. Create a *new* list representing the path extended with the neighbor.
            List<BoardTile> newPath = new List<BoardTile>(currentPath);
            newPath.Add(neighbor);

            // 2. Recursively call the function for the neighbor, with one less step remaining.
            FindPathsRecursive(neighbor, newPath, stepsRemaining - 1, allPathsFound);
        }
    }
}
