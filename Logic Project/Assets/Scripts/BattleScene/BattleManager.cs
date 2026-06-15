using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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

    private List<ItemData> selectedWeapons = new();

    private bool isSelecting;
    private bool playerTurn = true;
    private bool battleEnded;

    private int playerHP = 100;
    private int enemyHP;
    private int enemyAttack;

    private TileData enemyData;
    [Header("Energy")]
    public Slider energySlider;
    public int maxEnergy = 5;
    private int currentEnergy;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        enemyData = BattleData.currentEnemy;

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

        yield return new WaitForSeconds(0.3f);

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

            yield return new WaitForSeconds(0.3f);
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

        playerAnimator.SetTrigger("Hurt");

        playerHP -= enemyAttack;
        playerHP = Mathf.Max(0, playerHP);

        UpdateUI();

        if (playerHP <= 0)
        {
            playerImage.gameObject.SetActive(false);
            EndBattle(false);
            return;
        }
        RegenerateEnergy(1);

        playerTurn = true;
    }

    void WinBattle()
    {
        enemyImage.gameObject.SetActive(false);
        EndBattle(true);

        if (GridManager.Instance.finalBattleTriggered)
        {
            GridManager.Instance.StartCoroutine(
                GridManager.Instance.ReturnAndShowVictory()
            );
        }
        else
        {
            GridManager.Instance.ReturnFromBattle();
        }
    }

    void EndBattle(bool playerWon)
    {
        battleEnded = true;
        resultText.text = playerWon ? "YOU WIN!" : "YOU LOSE!";
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

        energySlider.value = currentEnergy;
    }

    void RegenerateEnergy(int amount = 1)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Min(currentEnergy, maxEnergy);

        energySlider.value = currentEnergy;
    }
}