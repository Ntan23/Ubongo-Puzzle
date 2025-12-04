using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    #region Singleton
    public static SceneLoader instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #endregion
    [SerializeField] private Animator animator;
    [SerializeField] private float transitionTime;

    public void LoadScene(string name)
    {
        StartCoroutine(LoadSceneWithString(name));
    }

    public void RestartScene()
    {
        StartCoroutine(LoadSceneWithIndex(SceneManager.GetActiveScene().buildIndex));
    }

    public void GoToNextScene()
    {
        StartCoroutine(LoadSceneWithIndex(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void GoToPreviousScene()
    {
        StartCoroutine(LoadSceneWithIndex(SceneManager.GetActiveScene().buildIndex - 1));
    }

    public void Quit()
    {
       StartCoroutine(QuitGame());
    }

    IEnumerator LoadSceneWithString(string name)
    {
        animator.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(name);
    }

    IEnumerator LoadSceneWithIndex(int index)
    {
        animator.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(index);
    }

    IEnumerator QuitGame()
    {
        animator.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        Application.Quit();
        Debug.Log("Quit Game!");
    }

}
