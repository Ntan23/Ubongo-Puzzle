using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class PieceUsingGrid : MonoBehaviour
{
    [Header("Behavior Settings")]
    [SerializeField] private float rotateDuration = 0.15f; 
    [SerializeField] private float moveDuration = 0.15f; 
    
    [Header("Alignment Correction (Fine-Tuning)")]
    [Tooltip("Adjust X/Y to perfectly align the Piece pivot center to the grid lines if needed.")]
    [SerializeField] private float snapCorrectionX = 0f; 
    [SerializeField] private float snapCorrectionY = 0f; 

    // Internal State
    private List<Vector2> shape = new List<Vector2>(); 
    private Block[] blocks; 
    private GridManager gridManager;
    private Vector3 dragOffset;
    private Vector3 originalPos; 
    
    // Flags
    [HideInInspector] public bool isPlaced = false; 
    private bool isDragging;
    private bool isRotating;
    private bool isMoving;

    void Start()
    {
        gridManager = GridManager.instance; 
        if (gridManager == null) Debug.LogError("GridManager not found in the scene!");

        blocks = GetComponentsInChildren<Block>();
        CalculateShapeFromChildren();
      
        originalPos = transform.position;
    }

    // --- Setup ---

    private void CalculateShapeFromChildren()
    {
        shape.Clear();
        foreach (Block block in blocks)
        {
            shape.Add(block.transform.localPosition);
        }
        if (shape.Count == 0) Debug.LogError(gameObject.name + ": Piece has no blocks assigned.");
    }
    
    public List<Vector2> GetBlockPositions()
    {
        return shape;
    }

    // --- Input & Snapping ---
    
    void Update()
    {
        if (!isDragging && !isRotating && !isMoving && Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(RotatePiece());
        }
    }

    void OnMouseDown()
    {
        dragOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dragOffset.z = 0; 
        
        if (isPlaced)
        {
            gridManager.UpdatePieceCoverage(this, false); 
        }
        isPlaced = false;
        isDragging = false; 
    }

    void OnMouseDrag()
    {
        isDragging = true;
        Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newPosition = mouseWorldPoint + dragOffset;
        newPosition.z = 0;
        transform.position = newPosition;
    }

    void OnMouseUp()
    {
        if (isDragging)
        {
            isDragging = false;
            TrySnapToBoard();
        }
    }

    private void TrySnapToBoard()
    {
        Vector3 block0WorldPos = blocks[0].transform.position;
        Vector3 block0LocalPos = blocks[0].transform.localPosition;

        Vector3 snapPos = gridManager.CalculateSnapTarget(
            block0WorldPos, 
            block0LocalPos, 
            snapCorrectionX, 
            snapCorrectionY);

        if (gridManager.CheckPlacement(this, snapPos))
        {
            StartCoroutine(MovePiece(snapPos, true));
        }
        else
        {
            StartCoroutine(MovePiece(originalPos, false)); 
        }
    }

    // --- Coroutines ---

    IEnumerator MovePiece(Vector3 targetPosition, bool isPlacementAttempt) 
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        float t = 0;

        while(t < 1)
        {
            t += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
        
        if (isPlacementAttempt)
        {
            isPlaced = true; 
            gridManager.UpdatePieceCoverage(this, true); 
            gridManager.CheckWinCondition();
        }
    }

    IEnumerator RotatePiece()
    {
        isRotating = true;
        
        List<Vector2> newShape = new List<Vector2>();
        foreach (Vector2 blockPos in shape)
        {
            newShape.Add(new Vector2(-blockPos.y, blockPos.x));
        }
        shape = newShape;

        Quaternion startAngle = transform.rotation;
        Quaternion targetAngle = startAngle * Quaternion.Euler(0, 0, -90f);
        
        float t = 0;

        while(t < 1f)
        {
            t += Time.deltaTime / rotateDuration;
            transform.rotation = Quaternion.Lerp(startAngle, targetAngle, t);
            yield return null;
        }
        
        transform.rotation = targetAngle;
        isRotating = false;
    }
}