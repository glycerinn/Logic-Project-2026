using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public TextMeshProUGUI resultText;

    public Image playerImage;
    public Image enemyImage;
    public Slider playerHealthSlider;
    public Slider enemyHealthSlider;

    public GameObject cardPrefab;
    public Transform cardParent;
    public Animator playerAnimator;
    public Animator enemyAnimator;

    private List<ItemData> selectedWeapons = new();

    private bool isSelecting;
    private bool playerTurn = true;
    private bool battleEnded;

    public int playerHP = 100;
    private int enemyHP;
    private int enemyAttack;
    public GameObject continuePanel;
    public TextMeshProUGUI continueButtonText;
    private bool playerWonBattle;
    private TileData enemyData;

    [Header("Energy")]
    public Slider energySlider;
    public int maxEnergy = 10;
    private int currentEnergy;
    private AudioManager audioManager;

    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager")?.GetComponent<AudioManager>();
        Instance = this;
    }

    void Start()
    {
        enemyData = BattleData.currentEnemy;
        Debug.Log(currentEnergy);
        if (enemyData != null)
        {
            enemyImage.sprite = enemyData.sprite;
            enemyHP = enemyData.maxHP;
            enemyAttack = enemyData.attack;
        }

        playerHealthSlider.minValue = 0;
        playerHealthSlider.maxValue = playerHP;
        playerHealthSlider.value = playerHP;

        enemyHealthSlider.minValue = 0;
        enemyHealthSlider.maxValue = enemyHP;
        enemyHealthSlider.value = enemyHP;

        currentEnergy = maxEnergy;

        energySlider.minValue = 0;
        energySlider.maxValue = maxEnergy;
        energySlider.value = currentEnergy;

        CreateWeaponCards();

        UpdateUI();
        resultText.text = "";

        continuePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isSelecting = true;
            selectedWeapons.Clear();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSelecting = false;

            if (selectedWeapons.Count > 0)
            {
                StartCoroutine(ExecuteCombo());
            }
        }
    }

    void CreateWeaponCards()
    {
        foreach (ItemData weapon in BattleData.selectedWeapons)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardParent);
            BattleCard card = cardObj.GetComponent<BattleCard>();

            card.Setup(weapon);
        }

         CardCarousel carousel = cardParent.GetComponent<CardCarousel>();

        if (carousel != null)
        {
            carousel.RefreshCards();
        }
    }

    public void SelectWeapon(ItemData weapon)
    {
        if (!playerTurn || battleEnded)
            return;

        audioManager?.playClickSFX();

        if (!isSelecting)
        {
            if (!HasEnoughEnergy(weapon.energyCost))
                return;

            StartCoroutine(SingleWeaponAttack(weapon));
            return;
        }

        if (selectedWeapons.Count >= 3)
            return;

        int currentCost = 0;

        foreach(ItemData selected in selectedWeapons)
        {
            currentCost += selected.energyCost;
        }

        if (currentCost + weapon.energyCost > currentEnergy)
            return;

        selectedWeapons.Add(weapon);
    }

    IEnumerator SingleWeaponAttack(ItemData weapon)
    {
        SpendEnergy(weapon.energyCost);
        playerTurn = false;

        playerAnimator.SetTrigger("Attack");
        audioManager?.playAttackSFX();

        yield return new WaitForSeconds(0.3f);

        enemyAnimator.SetTrigger("EnemyHurt");
        audioManager?.playeHurtSFX();

        yield return new WaitForSeconds(0.1f);

        enemyHP -= weapon.damage;
        enemyHP = Mathf.Max(0, enemyHP);

        UpdateUI();

        yield return new WaitForSeconds(0.5f);

        if (enemyHP <= 0)
        {
            WinBattle();
            yield break;
        }

        EnemyTurn();
    }

    IEnumerator ExecuteCombo()
    {
        playerTurn = false;
        int totalCost = 0;

        foreach(ItemData weapon in selectedWeapons)
        {
            totalCost += weapon.energyCost;
        }

        SpendEnergy(totalCost);

        foreach (ItemData weapon in selectedWeapons)
        {
            playerAnimator.SetTrigger("Attack");
            audioManager?.playAttackSFX();

            yield return new WaitForSeconds(0.3f);

            enemyAnimator.SetTrigger("EnemyHurt");
            audioManager?.playeHurtSFX();

            yield return new WaitForSeconds(0.1f);

            enemyHP -= weapon.damage;
            enemyHP = Mathf.Max(0, enemyHP);
            UpdateUI();

            yield return new WaitForSeconds(0.5f);

            if (enemyHP <= 0)
            {
                WinBattle();
                yield break;
            }
        }

        selectedWeapons.Clear();

        yield return new WaitForSeconds(0.5f);

        EnemyTurn();
    }

    void EnemyTurn()
    {
        if (battleEnded)
            return;

        enemyAnimator.SetTrigger("EnemyAttack");
        audioManager?.playeAttackSFX();

        playerAnimator.SetTrigger("Hurt");
        audioManager?.playHurtSFX();

        playerHP -= enemyAttack;
        playerHP = Mathf.Max(0, playerHP);

        UpdateUI();

        if (playerHP <= 0)
        {
            playerImage.gameObject.SetActive(false);
            EndBattle(false);
            return;
        }
        RegenerateEnergy(2);

        playerTurn = true;
    }

    void WinBattle()
    {
        enemyImage.gameObject.SetActive(false);
        EndBattle(true);
    }

    void EndBattle(bool playerWon)
    {
        battleEnded = true;
        playerWonBattle = playerWon;

        resultText.text = playerWon ? "YOU WIN!" : "YOU LOSE!";
        continueButtonText.text = playerWon ? "Continue" : "Main Menu";

        continuePanel.SetActive(true);
    }

    public void OnContinueButton()
    {
        audioManager?.playButtonSFX();
        if (playerWonBattle)
        {
            if (GridManager.Instance.finalBattleTriggered)
            {
                GridManager.Instance.StartCoroutine(GridManager.Instance.ReturnAndShowVictory());
            }
            else
            {
                GridManager.Instance.ReturnFromBattle();
            }
        }
        else
        {
            StartCoroutine(Transition());
        }
    }

    IEnumerator Transition()
    {
        Time.timeScale = 1f;
        
        yield return StartCoroutine(TransitionControl.Instance.PlayTransition());
        SceneManager.LoadScene("Main Menu");
        yield return StartCoroutine(TransitionControl.Instance.EndTransition());
    }

    void UpdateUI()
    {
        playerHealthSlider.value = playerHP;
        enemyHealthSlider.value = enemyHP;
    }

    bool HasEnoughEnergy(int cost)
    {
        return currentEnergy >= cost;
    }

    void SpendEnergy(int cost)
    {
        currentEnergy -= cost;
        currentEnergy = Mathf.Max(0, currentEnergy);
        Debug.Log(currentEnergy);
        energySlider.value = currentEnergy;
    }

    void RegenerateEnergy(int amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
        Debug.Log(currentEnergy);
        energySlider.value = currentEnergy;
    }
}