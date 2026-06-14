using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;
    public int coins;
    public TextMeshProUGUI coinText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = coins.ToString();
        }
    }
}