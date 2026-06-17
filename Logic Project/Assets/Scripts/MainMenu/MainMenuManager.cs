using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject MissionPanel;
    public GameObject CharacterPanel;
    public Animator animator;
    public SettingsManager settingsManager;
    public CreditsManager credits;

    private AudioManager audioManager;

    public void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    } 

    public void Start()
    {
        settingsManager.loadVolume();
        AudioManager.instance.playMainMenuBGM();
        Time.timeScale = 1f;
    }

    public void OnPlay()
    {
        audioManager.playButtonSFX();
        StartCoroutine(LoadNextLevel());
    }

    public void OnSettings()
    {
        audioManager.playButtonSFX();
        settingsManager.SetUp();
    }
    
    IEnumerator LoadNextLevel()
    {
        yield return StartCoroutine(TransitionControl.Instance.PlayTransition());

        SceneManager.LoadScene("SampleScene");

        yield return StartCoroutine(TransitionControl.Instance.EndTransition());
    }

    public void Quit()
    {
        audioManager.playButtonSFX();
        Application.Quit();
    }

    public void onCredits()
    {
        audioManager.playButtonSFX();
        credits.CreditsSetUp();
    }
}
