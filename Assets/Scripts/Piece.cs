using System.Collections;
using UnityEngine;

[System.Serializable]
public enum PieceType
{
    J,L,T,O,I,S,Z
}

public class Piece : MonoBehaviour
{
    [Header("Piece Settings")]
    [SerializeField] private PieceType pieceType;
    [SerializeField] private float snapRadius;
    [SerializeField] private float rotateDuration;
    [SerializeField] private float moveDuration;

    private Vector3 originalPos;
    private bool isDragging;
    private bool isSelected;
    private bool isRotating;
    private bool isMoving;
    private bool isPlaced;

    [SerializeField] private Block[] blocks;
    [SerializeField] private LayerMask boardLayerMask;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unselectedColor;
    [SerializeField] private ParticleSystem particleEffect;
    private Camera cam;


    PieceSelectManager pieceSelectManager;
    GameManager gameManager;

    void Start()
    {
        pieceSelectManager = PieceSelectManager.instance;
        gameManager = GameManager.instance;
        cam = Camera.main;

        originalPos = transform.position;
    }

    void Update()
    {
        if (isSelected && !isDragging)
        {
            if (Input.GetKeyDown(KeyCode.R) && !isRotating && !isMoving)
            {
                StartCoroutine(RotatePiece());
            }
        }
    }

    public void SelectPiece()
    {
        isSelected = true;

        spriteRenderer.color = selectedColor;
    }

    public void DeselectPiece()
    {
        isSelected = false;

        spriteRenderer.color = unselectedColor;
    }

    void OnMouseDown()
    {
        pieceSelectManager.SelectPiece(this);
        isDragging = false;
        isPlaced = false;

        spriteRenderer.sortingOrder = 3;

        ClearAllBlocksFromBoard();
    }

    void OnMouseDrag()
    {
        isDragging = true;

        Vector3 newPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        newPosition.z = 0;
        transform.position = newPosition;
    }

    void OnMouseUp()
    {
        isDragging = false;

        TrySnapToBoard();
    }

    IEnumerator MovePiece(Vector3 targetPosition)
    {
        isMoving = true;

        Vector3 startPosition = transform.position;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        isMoving = false;
        transform.position = targetPosition;
    }

    IEnumerator RotatePiece()
    {
        isRotating = true;

        Quaternion startAngle = transform.rotation;
        Quaternion targetAngle = startAngle * Quaternion.Euler(0, 0, 90f);

        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / rotateDuration;
            transform.rotation = Quaternion.Lerp(startAngle, targetAngle, t);
            yield return null;
        }

        transform.rotation = targetAngle;
        isRotating = false;
    }

    private void TrySnapToBoard()
    {
        if (IsValidAlignment(out Vector3 snapPos))
        {
            StartCoroutine(MovePiece(snapPos));

            BindBlocksToBoard();

            isPlaced = true;
            spriteRenderer.sortingOrder = 2;
            particleEffect.Play();

            pieceSelectManager.DeselectCurrentPiece();
            gameManager.CheckBoard();
        }
        else
        {
            StartCoroutine(MovePiece(originalPos));
        }
    }

    private bool IsValidAlignment(out Vector3 resultPos)
    {
        resultPos = Vector3.zero;

        BoardSlot[] hitSlots = new BoardSlot[blocks.Length];

        for (int i = 0; i < blocks.Length; i++)
        {
            Block currentBlock = blocks[i];

            Collider2D hit = Physics2D.OverlapPoint(currentBlock.transform.position, boardLayerMask);

            if (hit == null) return false;

            BoardSlot slot = hit.GetComponent<BoardSlot>();
            if (slot == null) return false;

            if (slot.isFilled) return false;

            hitSlots[i] = slot;
            currentBlock.currentSlot = slot;
        }

        Vector3 snapPos;

        if (pieceType == PieceType.O || pieceType == PieceType.I)
        {
            Vector3 currentPivotPos = transform.position;

            float snappedX = Mathf.Round(currentPivotPos.x * 2f) / 2f;
            float snappedY = Mathf.Round(currentPivotPos.y * 2f) / 2f;

            snapPos = new Vector3(snappedX, snappedY, currentPivotPos.z);

            if (snapPos.x % 1 != 0 || snapPos.y % 1 != 0)
            {
                return false;  
            }
        }
        else
        {
            Vector3 targetSlotPos = hitSlots[0].transform.position;
            Vector3 blockLocalOffset = blocks[0].transform.localPosition;

            snapPos = targetSlotPos - blockLocalOffset;
        }

        resultPos = snapPos;
        return true;

        // foreach (Block block in blocks)
        // {
        //     if (block.currentSlot == null) return false;

        //     if (block.currentSlot != null && block.currentSlot.isFilled)
        //     {
        //         return false;
        //     }
        // }

        // foreach (Block block in blocks)
        // {
        //     Vector3 blockLocal = blocks[0].transform.localPosition;
        //     Vector3 slotPos = block.currentSlot.transform.position;

        //     Vector3 snapPos = slotPos - blockLocal;
        //     snapPos.x = Mathf.RoundToInt(snapPos.x) + extraOffsetX;
        //     snapPos.y = Mathf.RoundToInt(snapPos.y) + extraOffsetY;

        //     resultPos = snapPos;
        //     return true;
        // }

        // return false;
    }

    // bool IsValidPlacement(Vector3 pos)
    // {
    //     foreach (Block block in blocks)
    //     {
    //         Vector3 checkPos = pos + block.transform.localPosition;

    //         Collider2D hit = Physics2D.OverlapPoint(checkPos);
    //         if (!hit) return false;

    //         BoardSlot slot = hit.GetComponent<BoardSlot>();
    //         if (!slot || slot.isFilled) return false;
    //     }

    //     return true;
    // }

    bool IsAlign(Vector3 pos)
    {
        Vector3 initialPos = transform.position;
        transform.position = pos;

        foreach (Block block in blocks)
        {
            Collider2D hit = Physics2D.OverlapPoint(block.transform.position);

            if (hit == null)
            {
                transform.position = initialPos;
                return false;
            }

            BoardSlot slot = hit.GetComponent<BoardSlot>();
            if (slot == null)
            {
                transform.position = initialPos;
                return false;
            }
            else
            {
                if (slot.isFilled)
                {
                    transform.position = initialPos;
                    return false;
                }
            }
        }

        transform.position = initialPos;
        return true;
    }

    void BindBlocksToBoard()
    {
        foreach (Block block in blocks)
        {
            if (block.currentSlot != null)
            {
                block.currentSlot.currentBlock = block;
                block.currentSlot.isFilled = true;
            }
        }
    }

    void ClearAllBlocksFromBoard()
    {
        foreach (Block block in blocks)
        {
            if (block.currentSlot != null)
            {
                block.currentSlot.Clear();
                block.currentSlot = null;
            }
        }
    }

    // void TrySnapToBoard()
    // {
    //     BoardSlot closestSlot = null;
    //     float closestDistance = Mathf.Infinity;

    //     // Find nearest slot to ANY block
    //     foreach (Block block in blocks)
    //     {
    //         Collider2D hit = Physics2D.OverlapCircle(block.transform.position, 0.5f);

    //         if (hit == null) continue;

    //         BoardSlot slot = hit.GetComponent<BoardSlot>();
    //         if (slot == null) continue;

    //         float distance = Vector2.Distance(block.transform.position, slot.transform.position);

    //         if (distance < closestDistance)
    //         {
    //             closestDistance = distance;
    //             closestSlot = slot;
    //         }
    //     }

    //     // Nothing found → return to last valid position
    //     if (closestSlot == null)
    //     {
    //         transform.position = lastValidPos;
    //         return;
    //     }

    //     // Snap the piece so that blocks align perfectly
    //     transform.position = closestSlot.transform.position;

    //     // Now check & bind each block
    //     if (!ValidateBlocks())
    //     {
    //         transform.position = lastValidPos; // revert
    //         ClearAllBlocksFromBoard();
    //         return;
    //     }
    //     else
    //     {
    //         foreach (Block block in blocks)
    //         {
    //             Collider2D hit = Physics2D.OverlapCircle(block.transform.position, 0.5f);
    //             BoardSlot slot = hit.GetComponent<BoardSlot>();

    //             slot.Fill(block);
    //             block.currentSlot = slot;
    //         }
    //     }

    //     lastValidPos = transform.position;
    //     pieceSelectManager.DeselectCurrentPiece();

    //     // Auto check board after placed
    //     gameManager.CheckBoard();
    // }

    // bool ValidateBlocks()
    // {
    //     foreach (Block block in blocks)
    //     {
    //         Collider2D hit = Physics2D.OverlapCircle(block.transform.position, 0.5f);
    //         if (hit == null) 
    //         {
    //             return false;
    //         }

    //         BoardSlot slot = hit.GetComponent<BoardSlot>();
    //         if (slot == null) 
    //         {
    //             return false;
    //         }

    //         if (slot.isFilled) 
    //         {
    //             return false;
    //         }
    //     }

    //     return true;
    // }

    // private BoardSlot FindNearestSlotAvailable()
    // {
    //     BoardSlot[] boardSlots = gameManager.GetBoardSlots();

    //     BoardSlot nearestSlot = null;
    //     float minDistance = Mathf.Infinity;

    //     foreach(BoardSlot slot in boardSlots)
    //     {
    //         if(slot.isFilled) 
    //         {
    //             continue;
    //         }

    //         float distance = Vector2.Distance(transform.position, slot.transform.position);

    //         if(distance < minDistance && distance <= snapRadius)
    //         {
    //             minDistance = distance;
    //             nearestSlot = slot;
    //         }
    //     }

    //     return nearestSlot;
    // }
}
