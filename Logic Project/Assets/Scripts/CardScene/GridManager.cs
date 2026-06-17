using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grid")]
    public GameObject tilePrefab;
    public Transform gridParent;

    public int width = 4;
    public int visibleColumns = 6;
    public int maxColumnsLeft = 2;
    private int totalColumnsCreated = 0;
    public int maxColumns = 100;
    private int nextEnemyColumn = 3;

    public bool finalBattleTriggered = false;

    [Header("Tile Data")]
    public TileData[] materialTiles;
    public TileData[] enemyTiles;
    public TileData[] weaponTiles;
    public TileData coinTileData;

    [Header("Movement")]
    public float moveSpeed = 12f;

    [Header("References")]
    public InventoryManager inventoryManager;

    public GameObject gridRoot;
    public GameObject victoryPanel;
    public GameObject inventory;
    public GameObject trash;

    private List<List<DropTile>> columns = new List<List<DropTile>>();
    private int activeColumn = 0;
    private float cellWidth;

    [Header("UI")]
    public Slider progressSlider;
    private AudioManager audioManager;

    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager")?.GetComponent<AudioManager>();
        Instance = this;
    }

    void Start()
    {
        audioManager?.playGameBGM();
        progressSlider.minValue = 0;
        progressSlider.maxValue = maxColumns;
        progressSlider.value = 0;
        nextEnemyColumn = Random.Range(3, 5);
        GridLayoutGroup layout = gridParent.GetComponent<GridLayoutGroup>();
        cellWidth = layout.cellSize.x + layout.spacing.x;

        for (int i = 0; i < visibleColumns; i++)
        {
            CreateColumn();
        }

        UpdateActiveColumn();
    }

    void CreateColumn()
    {
        if (totalColumnsCreated >= maxColumns) return;
        bool isBossColumn = totalColumnsCreated == maxColumns - 1;
        List<DropTile> newColumn = new List<DropTile>();
        TileType columnType;

        if (totalColumnsCreated >= maxColumns - 1)
        {
            columnType = TileType.Enemy;
        }
        else if (totalColumnsCreated >= nextEnemyColumn)
        {
            columnType = TileType.Enemy;
            nextEnemyColumn += Random.Range(3, 5);
        }
        else
        {
            columnType = Random.value < 0.5f
                ? TileType.Material
                : TileType.Weapon;
        }

        for (int y = 0; y < width; y++)
        {
            GameObject tileObj = Instantiate(tilePrefab, gridParent);
            DropTile tile = tileObj.GetComponent<DropTile>();

            tile.gridManager = this;
            tile.isLocked = true;

            TileData data = GetRandomTileFromType(columnType);

            tile.tileData = data;

            if (isBossColumn)
            {
                tile.isBossTile = true;
            }

            tile.ApplyVisuals();
            newColumn.Add(tile);
        }

        columns.Add(newColumn);

        totalColumnsCreated++;
        Debug.Log("Total columns created: " + totalColumnsCreated);
    }


    void RecycleLeftColumn()
    {
        if (finalBattleTriggered || totalColumnsCreated >= maxColumns) return;
        List<DropTile> recycledColumn = columns[0];

        columns.RemoveAt(0);
        TileType columnType;

        if (totalColumnsCreated >= maxColumns - 1)
        {
            columnType = TileType.Enemy;
        }
        else if (totalColumnsCreated >= nextEnemyColumn)
        {
            columnType = TileType.Enemy;
            nextEnemyColumn += Random.Range(3, 5);
        }
        else
        {
            columnType = Random.value < 0.5f
                ? TileType.Material
                : TileType.Weapon;
        }

        foreach (DropTile tile in recycledColumn)
        {
            // move tile to end of hierarchy
            tile.transform.SetAsLastSibling();

            TileData data = GetRandomTileFromType(columnType);

            if (columnType == TileType.Material && Random.value < 0.8f)
            {
                Debug.Log("Coin spawned!");
                data = coinTileData;
            }

            tile.tileData = data;
            tile.hasItem = true;
            tile.ApplyVisuals();

            if (tile.tileImage != null)
            {
                tile.tileImage.enabled = true;
            }
        }

        columns.Add(recycledColumn);

        totalColumnsCreated++;
    }

    // TILE DATA

    TileData GetRandomTileFromType(TileType type)
    {
        switch (type)
        {
            case TileType.Material:
                return materialTiles[Random.Range(0, materialTiles.Length)];
            case TileType.Enemy:
                return enemyTiles[Random.Range(0, enemyTiles.Length)];
            case TileType.Weapon:
                return weaponTiles[Random.Range(0, weaponTiles.Length)];
        }

        return null;
    }


    public void OnTileChosen(DropTile chosenTile, GameObject cardObj)
    {
        LockColumn(chosenTile);

        StartCoroutine(
            HandleMove(chosenTile, cardObj)
        );
    }
    
    void ReduceAllDurability(int amount)
    {
        foreach (InventorySlot slot in inventoryManager.slots)
        {
            if (slot.currentItem == null)
                continue;

            slot.durability -= amount;
            slot.UpdateDurabilityUI();

            if (slot.durability <= 0)
            {
                slot.RemoveItem();
            }
        }
    }

    void RestoreDurability(int amount)
    {
        foreach (InventorySlot slot in inventoryManager.slots)
        {
            if (slot.currentItem == null)
                continue;

            slot.durability += amount;
            slot.UpdateDurabilityUI();
            slot.durability = Mathf.Min(slot.durability, slot.currentItem.maxDurability);
        }
    }

    IEnumerator HandleMove(DropTile chosenTile, GameObject cardObj)
    {
        int columnIndex = -1;
        int rowIndex = -1;

        // find tile position
        for (int x = 0; x < columns.Count; x++)
        {
            int y = columns[x].IndexOf(chosenTile);

            if (y != -1)
            {
                columnIndex = x;
                rowIndex = y;
                break;
            }
        }

        if (columnIndex == -1)
            yield break;

        DropTile targetTile = columns[columnIndex][rowIndex];

        // move player card into tile
        cardObj.transform.SetParent(targetTile.transform, false);

        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);

        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.localScale = Vector3.one;

        activeColumn++;

        progressSlider.value = totalColumnsCreated - (columns.Count - activeColumn);
        
        if (chosenTile.isBossTile && !finalBattleTriggered)
        {
            finalBattleTriggered = true;
            BattleData.currentEnemy = chosenTile.tileData;
            StartCoroutine(EnterBattle());

            yield break;
        }

        ReduceAllDurability(1);
        UpdateActiveColumn();

        RectTransform gridRect = gridParent.GetComponent<RectTransform>();
        Vector2 startPos = gridRect.anchoredPosition;
        Vector2 targetPos = startPos - new Vector2(cellWidth, 0);

        while (Vector2.Distance(gridRect.anchoredPosition, targetPos) > 0.1f)
        {
            gridRect.anchoredPosition = Vector2.Lerp(
                    gridRect.anchoredPosition,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );

            yield return null;
        }

        gridRect.anchoredPosition = targetPos;
        CleanupColumns();

       if (chosenTile.tileData.isCoin)
        {
            CoinManager.Instance.AddCoins(chosenTile.tileData.coinValue);
            chosenTile.hasItem = false;

            if (chosenTile.tileImage != null)
            {
                chosenTile.tileImage.enabled = false;
            }
        }
        else if (chosenTile.hasItem && chosenTile.tileData.itemReward != null)
        {
            inventoryManager.AddItem(chosenTile.tileData.itemReward);
            chosenTile.hasItem = false;

            if (chosenTile.tileImage != null)
            {
                chosenTile.tileImage.enabled = false;
            }
        }

        if (chosenTile.tileData.type == TileType.Enemy)
        {
            RestoreDurability(3);
            BattleData.currentEnemy = chosenTile.tileData;
            StartCoroutine(EnterBattle());
        }
    }

    void CleanupColumns()
    {
        if (totalColumnsCreated >= maxColumns)
        return;

        if (activeColumn > maxColumnsLeft)
        {
            RecycleLeftColumn();
            activeColumn--;

            RectTransform gridRect = gridParent.GetComponent<RectTransform>();
            gridRect.anchoredPosition += new Vector2(cellWidth, 0);
        }
        Debug.Log("Final battle triggered: " + finalBattleTriggered);
    }


    void UpdateActiveColumn()
    {
        for (int x = 0; x < columns.Count; x++)
        {
            foreach (DropTile tile in columns[x])
            {
                tile.isLocked =
                    (x != activeColumn);
            }
        }
    }

    void LockColumn(DropTile chosenTile)
    {
        foreach (var column in columns)
        {
            if (column.Contains(chosenTile))
            {
                foreach (var tile in column)
                {
                    tile.isLocked = true;
                }

                break;
            }
        }
    }

    // BATTLE

    IEnumerator EnterBattle()
    {
        audioManager?.playBossBGM();
        if (TransitionControl.Instance != null)
        {
            yield return StartCoroutine(TransitionControl.Instance.PlayTransition());
        }

        gridRoot.SetActive(false);
        inventory.SetActive(false);
        trash.SetActive(false);

        PrepareBattleWeapons();

        yield return SceneManager.LoadSceneAsync("Battle Scene", LoadSceneMode.Additive);

        if (TransitionControl.Instance != null)
        {
            yield return StartCoroutine(TransitionControl.Instance.EndTransition());
        }
    }

    public void ReturnFromBattle()
    {
        StartCoroutine(ReturnRoutine());
    }

    IEnumerator ReturnRoutine()
    {
        if (TransitionControl.Instance != null)
        {
            yield return StartCoroutine(TransitionControl.Instance.PlayTransition());
        }

        yield return SceneManager.UnloadSceneAsync("Battle Scene");

        audioManager?.resumeGameBGM();

        gridRoot.SetActive(true);
        inventory.SetActive(true);
        trash.SetActive(true);

        InventoryItem player = FindFirstObjectByType<InventoryItem>();

        if (player != null)
        {
            CanvasGroup cg = player.GetComponent<CanvasGroup>();

            if (cg != null)
            {
                cg.blocksRaycasts = true;
                cg.alpha = 1f;
            }
        }

        if (TransitionControl.Instance != null)
        {
            yield return StartCoroutine(TransitionControl.Instance.EndTransition());
        }
    }

    void PrepareBattleWeapons()
    {
        List<ItemData> weapons = new List<ItemData>();

        foreach (InventorySlot slot in inventoryManager.slots)
        {
            if (slot.currentItem != null && slot.currentItem.itemType == ItemType.Weapon)
            {
                weapons.Add(slot.currentItem);
            }
        }

        BattleData.selectedWeapons.Clear();

        if (weapons.Count <= 3)
        {
            BattleData.selectedWeapons.AddRange(weapons);
        }
        else
        {
            List<ItemData> pool = new List<ItemData>(weapons);

            for (int i = 0; i < 3; i++)
            {
                int randomIndex = Random.Range(0, pool.Count);
                BattleData.selectedWeapons.Add(pool[randomIndex]);

                pool.RemoveAt(randomIndex);
            }
        }
    }

    public IEnumerator ReturnAndShowVictory()
    {
        if (TransitionControl.Instance != null)
        {
            yield return StartCoroutine(TransitionControl.Instance.PlayTransition());
        }

        yield return SceneManager.UnloadSceneAsync("Battle Scene");    

        gridRoot.SetActive(true);
        inventory.SetActive(true);
        trash.SetActive(true);

        victoryPanel.SetActive(true);

        if (TransitionControl.Instance != null)
        {
            yield return StartCoroutine(TransitionControl.Instance.EndTransition());
        }

        Time.timeScale = 0f;
    }

    public void ShowVictory()
    {
        victoryPanel.SetActive(true);
    }
}