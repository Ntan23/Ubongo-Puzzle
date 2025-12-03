using UnityEngine;

public class PieceSelectManager : MonoBehaviour
{
    #region Singleton
    public static PieceSelectManager instance;
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    #endregion
    
    public Piece currentSelectedPiece;

    public void SelectPiece(Piece piece)
    {
        if(currentSelectedPiece != null & currentSelectedPiece != piece)
        {
            currentSelectedPiece.DeselectPiece();
        }

        currentSelectedPiece = piece;
        piece.SelectPiece();
    }

    public void DeselectCurrentPiece()
    {
        if(currentSelectedPiece != null)
        {
            currentSelectedPiece.DeselectPiece();
            currentSelectedPiece = null;
        }
    }
}
