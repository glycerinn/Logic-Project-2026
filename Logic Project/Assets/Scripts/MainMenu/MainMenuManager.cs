using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject MissionPanel;
    public GameObject CharacterPanel;
    public Animator animator;

    public void OnPlay()
    {
        StartCoroutine(LoadNextLevel());
    }
    

    IEnumerator LoadNextLevel()
    {
        yield return StartCoroutine(TransitionControl.Instance.PlayTransition());

        SceneManager.LoadScene("SampleScene");

        yield return StartCoroutine(TransitionControl.Instance.EndTransition());
    }

    public void Quit()
    {
        Application.Quit();
    }
}
