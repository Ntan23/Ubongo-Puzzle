using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "NewUbongoLevel", menuName = "Ubongo/Level Configuration")]
public class LevelData : ScriptableObject
{
    [Header("Board Dimensions")]
    public int boardWidth = 5;
    public int boardHeight = 5;

    [Header("Solution Map (Rows Y, Columns X)")]
    [Tooltip("Enter the board layout as rows of 0s (empty) and 1s (required). Top row is the last element in the list.")]
    public List<string> solutionLayoutStrings; 

    public int[,] GetSolutionMap()
    {
        if (solutionLayoutStrings == null || solutionLayoutStrings.Count == 0)
        {
            return new int[boardWidth, boardHeight];
        }

        int[,] map = new int[boardWidth, boardHeight];
        
        if (solutionLayoutStrings.Count != boardHeight || (boardHeight > 0 && solutionLayoutStrings[0].Length != boardWidth))
        {
             Debug.LogError("Layout dimensions do not match specified width/height in LevelData: " + this.name);
             return map;
        }

        // Map the string rows (index 0 is top row) to the grid map (Y=0 is bottom row)
        for (int y = 0; y < boardHeight; y++)
        {
            // Read the string row corresponding to the correct Y position (bottom-up index)
            string row = solutionLayoutStrings[boardHeight - 1 - y];
            for (int x = 0; x < boardWidth; x++)
            {
                if (x < row.Length && row[x] == '1')
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = 0;
                }
            }
        }
        return map;
    }
}