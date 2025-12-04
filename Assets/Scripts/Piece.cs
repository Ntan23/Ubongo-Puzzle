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
        if (IsValidPosition(out Vector3 snapPos))
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

    private bool IsValidPosition(out Vector3 resultPos)
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
    }

    void BindBlocksToBoard()
    {
        foreach (Block block in blocks)
        {
            if (block.currentSlot != null)
            {
                block.currentSlot.Fill(block);
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
}
