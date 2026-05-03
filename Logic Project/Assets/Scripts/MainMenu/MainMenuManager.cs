using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject MissionPanel;
    public GameObject CharacterPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlay()
    {
        MissionPanel.SetActive(true);
        MainMenuPanel.SetActive(false);
    }

    public void OnMission()
    {
        CharacterPanel.SetActive(true);
        MissionPanel.SetActive(false);
    }

    public void OnCharacter()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnBackCharacter()
    {
        MissionPanel.SetActive(true);
        CharacterPanel.SetActive(false);
    }

    public void OnBackMission()
    {
        MainMenuPanel.SetActive(true);
        MissionPanel.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
