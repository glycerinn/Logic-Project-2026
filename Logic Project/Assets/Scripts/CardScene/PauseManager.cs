using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;

    public void onPause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void MainMenu()
    {
        StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        Time.timeScale = 1f;
        
        yield return StartCoroutine(TransitionControl.Instance.PlayTransition());

        SceneManager.LoadScene("Main Menu");

        yield return StartCoroutine(TransitionControl.Instance.EndTransition());
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }
}
