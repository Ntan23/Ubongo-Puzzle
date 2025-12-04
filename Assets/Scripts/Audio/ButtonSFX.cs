using UnityEngine;
using UnityEngine.UI;

public class ButtonSFX : MonoBehaviour
{
    Button button;
    AudioManager audioManager;

    void Start()
    {
        audioManager = AudioManager.instance;
        button = GetComponent<Button>();
    }

    public void PlayHoverSFX()
    {
        audioManager.PlayButtonHoverSFX();
    }

    public void PlayClickSFX()
    {
        audioManager.PlayButtonClickSFX();
    }
}
