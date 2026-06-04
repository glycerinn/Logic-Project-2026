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
    private int nextEnemyColumn = 3;

    [Header("Tile Data")]
    public TileData[] materialTiles;
    public TileData[] enemyTiles;
    public TileData[] weaponTiles;

    [Header("Movement")]
    public float moveSpeed = 12f;

    [Header("References")]
    public InventoryManager inventoryManager;

    public GameObject gridRoot;
    public GameObject inventory;
    public GameObject trash;

    private List<List<DropTile>> columns = new List<List<DropTile>>();
    private int activeColumn = 0;
    private float cellWidth;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
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
        List<DropTile> newColumn = new List<DropTile>();
        TileType columnType;

        if (totalColumnsCreated >= nextEnemyColumn)
        {
            columnType = TileType.Enemy;

            nextEnemyColumn += Random.Range(3, 5);
        }
        else
        {
            columnType =
                Random.value < 0.5f
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
            tile.ApplyVisuals();
            newColumn.Add(tile);
        }

        columns.Add(newColumn);

        totalColumnsCreated++;
    }


    void RecycleLeftColumn()
    {
        List<DropTile> recycledColumn = columns[0];

        columns.RemoveAt(0);
        TileType columnType;

        if (totalColumnsCreated >= nextEnemyColumn)
        {
            columnType = TileType.Enemy;

            nextEnemyColumn += Random.Range(3, 5);
        }
        else
        {
            columnType =
                Random.value < 0.5f
                ? TileType.Material
                : TileType.Weapon;
        }

        foreach (DropTile tile in recycledColumn)
        {
            // move tile to end of hierarchy
            tile.transform.SetAsLastSibling();

            TileData data = GetRandomTileFromType(columnType);

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

        if (chosenTile.hasItem && chosenTile.tileData.itemReward != null)
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
        if (activeColumn > maxColumnsLeft)
        {
            RecycleLeftColumn();
            activeColumn--;

            RectTransform gridRect = gridParent.GetComponent<RectTransform>();
            gridRect.anchoredPosition += new Vector2(cellWidth, 0);
        }
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
        gridRoot.SetActive(false);
        inventory.SetActive(false);
        trash.SetActive(false);

        PrepareBattleWeapons();

        yield return SceneManager.LoadSceneAsync("Battle Scene", LoadSceneMode.Additive);
    }

    public void ReturnFromBattle()
    {
        StartCoroutine(ReturnRoutine());
    }

    IEnumerator ReturnRoutine()
    {
        yield return SceneManager.UnloadSceneAsync("Battle Scene");

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
}