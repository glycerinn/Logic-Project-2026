using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI resultText;

    public Image playerImage;
    public Image enemyImage;

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

            Debug.Log("Fighting: " + enemyData.tileName);
        }

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
            StartCoroutine(SingleWeaponAttack(weapon));
            return;
        }

        if (selectedWeapons.Count >= 3)
            return;

        selectedWeapons.Add(weapon);

        Debug.Log(
            $"Selected {weapon.itemName} ({weapon.damage} dmg)"
        );
    }

    IEnumerator SingleWeaponAttack(ItemData weapon)
    {
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

        foreach (ItemData weapon in selectedWeapons)
        {
            enemyHP -= weapon.damage;
            enemyHP = Mathf.Max(0, enemyHP);

            Debug.Log(
                $"{weapon.itemName} dealt {weapon.damage}"
            );

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

        playerHP -= enemyAttack;
        playerHP = Mathf.Max(0, playerHP);

        UpdateUI();

        if (playerHP <= 0)
        {
            playerImage.gameObject.SetActive(false);
            EndBattle(false);
            return;
        }

        playerTurn = true;
    }

    void WinBattle()
    {
        enemyImage.gameObject.SetActive(false);

        EndBattle(true);

        if (GridManager.Instance != null)
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
        playerHPText.text = $"Player HP: {playerHP}";
        enemyHPText.text = $"Enemy HP: {enemyHP}";
    }
}