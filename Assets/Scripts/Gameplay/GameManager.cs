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
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private ParticleSystem winEfect;
    [SerializeField] private GameObject winPopup;
    [SerializeField] private Animator winPopupAnimator;

    AudioManager audioManager;

    void Start()
    {
        audioManager = AudioManager.instance;

        winPopup.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            sceneLoader.RestartScene();
        }
    }

    public void CheckBoard()
    {
        if (IsBoardFillCorrectly())
        {
            Debug.Log("You Win!!");

            winPopup.SetActive(true);
            winPopupAnimator.Play("Show");
            audioManager.PlayWinSFX();
            winEfect.Play();
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
