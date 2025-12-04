using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #endregion

    [SerializeField] private BoardSlot[] boardSlots;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SceneLoader.instance.RestartScene();
        }
    }

    public void CheckBoard()
    {
        if (IsBoardFillCorrectly())
        {
            Debug.Log("You Win!!");
        }
    }

    private bool IsBoardFillCorrectly()
    {
        foreach (BoardSlot slot in boardSlots)
        {
            if (!slot.isFilled)
            {
                return false;
            }
        }

        return true;
    }
}
