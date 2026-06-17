using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour
{
    
    public void CreditsSetUp()
    {
        gameObject.SetActive(true);
    }

    public void CreditsLoadMenu()
    {
        gameObject.SetActive(false);
    }
}