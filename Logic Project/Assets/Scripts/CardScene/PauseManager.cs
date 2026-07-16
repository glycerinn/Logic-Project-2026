using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    private bool isLoading = false;

    public void onPause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void MainMenu()
    {
        if (isLoading)
            return;

        isLoading = true;
        StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        Time.timeScale = 1f;
        
        if (TransitionControl.Instance != null)
        {
            yield return StartCoroutine(TransitionControl.Instance.EndTransition());
        }
        
        SceneManager.LoadScene("Main Menu");

        if (TransitionControl.Instance != null)
        {
            yield return StartCoroutine(TransitionControl.Instance.EndTransition());
        }
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }
}
