using UnityEngine;
using System.Collections;

public class TransitionControl : MonoBehaviour
{
    public static TransitionControl Instance;
    public Animator animator;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public IEnumerator PlayTransition()
    {
        animator.SetTrigger("Start");

        yield return new WaitForSeconds(1f);
    }

    public IEnumerator EndTransition()
    {
        animator.SetTrigger("End");

        yield return new WaitForSeconds(1f);
    }
}