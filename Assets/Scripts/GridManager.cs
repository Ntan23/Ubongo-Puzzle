using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    [Header("Current Level")]
    public LevelData currentLevelData; 

    [Header("Visual Reference")]
    public Transform boardVisualTransform; 

    // Internal state
    [SerializeField] private List<PieceUsingGrid> allScenePieces; 
    private int boardWidth;
    private int boardHeight;
    private int[,] solutionMap;
    private Vector3 gridOriginOffset; 
    
    private Dictionary<PieceUsingGrid, Vector3> placedPiecesMap = new Dictionary<PieceUsingGrid, Vector3>(); 
    private int[,] coverageGrid; 

    void Start()
    {
        if (currentLevelData == null) { Debug.LogError("No LevelData assigned!"); return; }

        boardWidth = currentLevelData.boardWidth;
        boardHeight = currentLevelData.boardHeight;
        solutionMap = currentLevelData.GetSolutionMap();
        coverageGrid = new int[boardWidth, boardHeight];
        
        CalculateGridOffset();
        //allScenePieces = FindObjectsOfType<PieceUsingGrid>().ToList();
    }

    private void CalculateGridOffset()
    {
        if (boardVisualTransform == null) { Debug.LogError("Board Visual Transform is not assigned!"); return; }

        float offsetX = boardVisualTransform.position.x - (boardWidth / 2f);
        float offsetY = boardVisualTransform.position.y - (boardHeight / 2f);
        
        gridOriginOffset = new Vector3(offsetX, offsetY, 0); 
    }

    // --- Coordinate Transformation ---

    private bool WorldToGridIndex(Vector3 worldPos, out int x, out int y)
    {
        Vector3 relativePosition = worldPos - gridOriginOffset;
        
        x = Mathf.RoundToInt(relativePosition.x);
        y = Mathf.RoundToInt(relativePosition.y);
        
        return x >= 0 && x < boardWidth && y >= 0 && y < boardHeight;
    }
    
    public Vector3 CalculateSnapTarget(Vector3 blockWorldPos, Vector3 blockLocalPos, float correctionX, float correctionY)
    {
        Vector3 targetWorldPos = blockWorldPos - blockLocalPos;
        
        targetWorldPos.x += correctionX;
        targetWorldPos.y += correctionY;

        targetWorldPos.x = Mathf.Round(targetWorldPos.x);
        targetWorldPos.y = Mathf.Round(targetWorldPos.y);

        return targetWorldPos;
    }

    // --- Coverage Management ---

    public void UpdatePieceCoverage(PieceUsingGrid piece, bool placed)
    {
        if (placed)
        {
            if (placedPiecesMap.ContainsKey(piece))
                placedPiecesMap[piece] = piece.transform.position;
            else
                placedPiecesMap.Add(piece, piece.transform.position);
        }
        else
        {
            placedPiecesMap.Remove(piece);
        }
        
        RebuildCoverageGrid();
    }

    private void RebuildCoverageGrid()
    {
        coverageGrid = new int[boardWidth, boardHeight];
        
        foreach (var kvp in placedPiecesMap)
        {
            PieceUsingGrid piece = kvp.Key;
            Vector3 piecePos = kvp.Value;

            foreach (Vector2 relativePos in piece.GetBlockPositions())
            {
                Vector3 blockWorldPos = piecePos + new Vector3(relativePos.x, relativePos.y, 0);
                
                if (WorldToGridIndex(blockWorldPos, out int x, out int y))
                {
                    coverageGrid[x, y] = 1; 
                }
            }
        }
    }

    // --- Placement Check (The Core Snapping Logic) ---
    public bool CheckPlacement(PieceUsingGrid pieceToCheck, Vector3 potentialPosition)
    {
        // 1. Calculate a temporary coverage grid of all *other* currently placed pieces.
        int[,] tempOtherCoverage = new int[boardWidth, boardHeight];
        foreach (var kvp in placedPiecesMap)
        {
            if (kvp.Key == pieceToCheck) continue;

            PieceUsingGrid otherPiece = kvp.Key;
            Vector3 otherPiecePos = kvp.Value;

            foreach (Vector2 relativePos in otherPiece.GetBlockPositions())
            {
                Vector3 blockWorldPos = otherPiecePos + new Vector3(relativePos.x, relativePos.y, 0);
                
                if (WorldToGridIndex(blockWorldPos, out int x, out int y))
                {
                    tempOtherCoverage[x, y] = 1; 
                }
            }
        }
        
        // 2. Check the pieceToCheck at the potentialPosition
        foreach (Vector2 relativePos in pieceToCheck.GetBlockPositions())
        {
            Vector3 blockWorldPos = potentialPosition + new Vector3(relativePos.x, relativePos.y, 0);
            
            if (!WorldToGridIndex(blockWorldPos, out int x, out int y))
            {
                // Check 1: Bounds/Overhang
                return false;
            }

            // Check 2: Overlap with OTHER pieces
            if (tempOtherCoverage[x, y] == 1) 
            {
                return false; 
            }
            
            // Check 3: Overhang (Must be on a required slot)
            if (solutionMap[x, y] == 0) 
            {
                return false; 
            }
        }
        
        return true; 
    }

    // --- Final Win Condition Check ---
    public bool CheckWinCondition()
    {
        RebuildCoverageGrid();
        
        // Check for GAPS
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if (solutionMap[x, y] == 1 && coverageGrid[x, y] == 0)
                {
                    return false; 
                }
            }
        }

        Debug.Log("UBONGO! Puzzle Solved!");
        return true; 
    }
}