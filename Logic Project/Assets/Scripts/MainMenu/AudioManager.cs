using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource BGM;
    [SerializeField] AudioSource SFX;

    public AudioClip MainMenuBGM;
    public AudioClip GameBGM;
    public AudioClip BossBGM;
    public AudioClip Buttonsfx;
    public AudioClip Clicksfx;
    public AudioClip[] playerattacksfx;
    public AudioClip[] playerhurtsfx;
    public AudioClip[] enemyattacksfx;
    public AudioClip[] enemyhurtsfx;
    public AudioClip[] dropsfx;
    public AudioClip[] pickupsfx;

    private float savedCardMusicTime;

    public static AudioManager instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    public void playMainMenuBGM()
    {
        BGM.clip = MainMenuBGM;
        BGM.Play();
    }

    public void playGameBGM()
    {
        BGM.clip = GameBGM;
        BGM.Play();
    }

    public void resumeGameBGM()
    {
        BGM.clip = GameBGM;
        BGM.time = savedCardMusicTime;
        BGM.Play();
    }

    public void playBossBGM()
    {
        savedCardMusicTime = BGM.time;
        BGM.clip = BossBGM;
        BGM.Play();
    }

    public void playButtonSFX()
    {
        SFX.PlayOneShot(Buttonsfx);
    }

    public void playClickSFX()
    {
        SFX.PlayOneShot(Clicksfx);
    }

    public void playAttackSFX()
    {   
        // if (isGameOver) return;
        // if (isPaused) return;
        int rand = Random.Range(0, playerattacksfx.Length);
        SFX.PlayOneShot(playerattacksfx[rand]);
    }

    public void playHurtSFX()
    {   
        // if (isGameOver) return;
        // if (isPaused) return;
        int rand = Random.Range(0, playerhurtsfx.Length);
        SFX.PlayOneShot(playerhurtsfx[rand]);
    }

    public void playeAttackSFX()
    {   
        // if (isGameOver) return;
        // if (isPaused) return;
        int rand = Random.Range(0, enemyattacksfx.Length);
        SFX.PlayOneShot(enemyattacksfx[rand]);
    }

    public void playeHurtSFX()
    {   
        // if (isGameOver) return;
        // if (isPaused) return;
        int rand = Random.Range(0, enemyhurtsfx.Length);
        SFX.PlayOneShot(enemyhurtsfx[rand]);
    }

    public void playDropSFX()
    {   
        // if (isGameOver) return;
        // if (isPaused) return;
        int rand = Random.Range(0, dropsfx.Length);
        SFX.PlayOneShot(dropsfx[rand]);
    }

    public void playpickupSFX()
    {   
        // if (isGameOver) return;
        // if (isPaused) return;
        AudioClip clip = pickupsfx[Random.Range(0, pickupsfx.Length)];
        SFX.PlayOneShot(clip);
    }
}
