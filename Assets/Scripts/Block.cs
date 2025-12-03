using UnityEngine;

public class Block : MonoBehaviour
{
    public BoardSlot currentSlot;
    public Piece piece;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BoardSlot"))
        {
            BoardSlot boardSlot = collision.GetComponent<BoardSlot>();
            currentSlot = boardSlot;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("BoardSlot"))
        {
            BoardSlot boardSlot = collision.GetComponent<BoardSlot>();

            if (currentSlot == boardSlot)
            {
                currentSlot = null;
            }
        }
    }
}
