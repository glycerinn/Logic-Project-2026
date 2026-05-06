using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI resultText;
    public Image playerImage;
    public Image enemyImage;

    List<int> selectedAttacks = new List<int>();
    bool isSelecting = false;

    int playerHP = 100;
    int enemyHP = 50;

    int enemyAttack = 8;

    bool playerTurn = true;
    bool battleEnded = false;

    void Start()
    {
        UpdateUI();
        resultText.text = "";
    }

    void Update()
    {
        // Start selecting when Shift is held
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isSelecting = true;
            selectedAttacks.Clear();
            Debug.Log("Started selecting attacks");
        }

        // When Shift is released
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSelecting = false;

            if (selectedAttacks.Count > 0)
            {
                StartCoroutine(ExecuteCombo());
            }
        }
    }

    void UpdateUI()
    {
        playerHPText.text = "Player HP: " + playerHP;
        enemyHPText.text = "Enemy HP: " + enemyHP;
    }

    void EnemyTurn()
    {
        if (battleEnded) return;

        playerHP -= enemyAttack;
        if (playerHP < 0) playerHP = 0;

        UpdateUI();

        if (playerHP <= 0)
        {
            playerHP = 0;
            playerImage.gameObject.SetActive(false); // disappear

            EndBattle(false);
            return;
        }

        playerTurn = true;
    }

    void EndBattle(bool playerWon)
    {
        battleEnded = true;

        resultText.text = playerWon ? "YOU WIN!" : "YOU LOSE!";
    }

    void SelectAttack(int damage)
    {
        if (!playerTurn || battleEnded) return;

        // If NOT holding shift
        if (!isSelecting)
        {
            StartCoroutine(SingleAttack(damage));
            return;
        }

        // Limit to 3 selections
        if (selectedAttacks.Count >= 3)
        {
            Debug.Log("Max 3 attacks!");
            return;
        }

        selectedAttacks.Add(damage);
        Debug.Log("Selected attack: " + damage);
    }

    IEnumerator ExecuteCombo()
    {
        playerTurn = false;

        foreach (int dmg in selectedAttacks)
        {
            enemyHP -= dmg;
            if (enemyHP < 0) enemyHP = 0;

            UpdateUI();

            yield return new WaitForSeconds(0.5f);

            if (enemyHP <= 0)
            {
                enemyImage.gameObject.SetActive(false);
                EndBattle(true);
                if (GridManager.Instance != null)
                {
                    GridManager.Instance.ReturnFromBattle();
                }
                yield break;
            }
        }

        // Enemy turn after combo
        yield return new WaitForSeconds(0.5f);
        EnemyTurn();
    }

    IEnumerator SingleAttack(int dmg)
    {
        playerTurn = false;

        enemyHP -= dmg;
        if (enemyHP < 0) enemyHP = 0;

        UpdateUI();

        yield return new WaitForSeconds(0.5f);

        if (enemyHP <= 0)
        {
            enemyImage.gameObject.SetActive(false);
            EndBattle(true);
            if (GridManager.Instance != null)
            {
                GridManager.Instance.ReturnFromBattle();
            }
            yield break;
        }

        EnemyTurn();
    }

    public void Attack1() { SelectAttack(5); }
    public void Attack2() { SelectAttack(10); }
    public void Attack3() { SelectAttack(15); }
    public void Attack4() { SelectAttack(20); }
    public void Attack5() { SelectAttack(25); }
}