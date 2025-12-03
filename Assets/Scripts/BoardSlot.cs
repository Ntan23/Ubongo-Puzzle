using UnityEngine;

public class BoardSlot : MonoBehaviour
{
    public bool isFilled;
    public Block currentBlock;

    public void Fill(Block b)
    {
        isFilled = true;
        currentBlock = b;
    }

    public void Clear()
    {
        isFilled = false;
        currentBlock = null;
    }
}
