using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject tilePrefab;
    public Transform gridParent;
    public static GridManager Instance;
    public int width = 4;
    public int visibleRows = 5;
    int bufferRows = 3;

    [Header("Tile Data")]
    public TileData[] materialTiles;
    public TileData[] enemyTiles;
    public TileData[] weaponTiles;

    [Header("System References")]
    public InventoryManager inventoryManager;

    [Header("Row Control")]
    private List<List<DropTile>> rows = new List<List<DropTile>>();
    private int activeRow = 0;
    int totalRowsCreated = 0;

    [Header("Grid Movement")]
    private float cellHeight;
    public int maxRowsBelow = 3;
    public float cleanupYThreshold = -200f;

    [Header("References")]
    public GameObject gridRoot;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        for (int i = 0; i < visibleRows; i++)
        {
            AddNewRow();
        }

        UpdateActiveRow();

        GridLayoutGroup layout = gridParent.GetComponent<GridLayoutGroup>();
        cellHeight = layout.cellSize.y + layout.spacing.y;
    }

    void AddNewRow()
    {
        List<DropTile> newRow = new List<DropTile>();
        int rowIndex = totalRowsCreated;
        TileType rowType = (TileType)Random.Range(0, 3);

        for (int x = 0; x < width; x++)
        {
            GameObject tileObj = Instantiate(tilePrefab, gridParent);

            DropTile tile = tileObj.GetComponent<DropTile>();
            tile.gridManager = this;
            tile.isLocked = true;

            TileData data = GetRandomTileFromType(rowType);
            tile.tileData = data;
            tile.ApplyVisuals();

            newRow.Add(tile);
        }

        rows.Add(newRow);
        totalRowsCreated++;
    }

    void RemoveBottomRow()
    {
        if (rows.Count == 0) return;

        List<DropTile> bottomRow = rows[0];

        foreach (var tile in bottomRow)
        {
            Destroy(tile.gameObject);
        }

        rows.RemoveAt(0);
    }

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
        LockRow(chosenTile);
        StartCoroutine(HandleMove(chosenTile, cardObj));
    }

    public void ReturnFromBattle()
    {
        StartCoroutine(ReturnRoutine());
    }

    IEnumerator ReturnRoutine()
    {
        Debug.Log("GRID: Returning from battle");

        // unload battle scene safely from HERE
        yield return SceneManager.UnloadSceneAsync("Battle Scene");

        Debug.Log("GRID: Battle unloaded");

        gridRoot.SetActive(true);
    }

    IEnumerator EnterBattle()
    {
        gridRoot.SetActive(false);
        AudioListener gridListener = Camera.main.GetComponent<AudioListener>();
        if (gridListener != null)
            gridListener.enabled = false;

        yield return SceneManager.LoadSceneAsync("Battle Scene", LoadSceneMode.Additive);
    }

    IEnumerator HandleMove(DropTile chosenTile, GameObject cardObj)
    {
        int rowIndex = -1;
        int colIndex = -1;

        for (int i = 0; i < rows.Count; i++)
        {
            int index = rows[i].IndexOf(chosenTile);
            if (index != -1)
            {
                rowIndex = i;
                colIndex = index;
                break;
            }
        }

        if (rowIndex == -1) yield break;

        EnsureRowsAhead(rowIndex);

        DropTile targetTile = rows[rowIndex][colIndex];

        CanvasGroup cg = cardObj.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        LayoutRebuilder.ForceRebuildLayoutImmediate(gridParent.GetComponent<RectTransform>());
        yield return new WaitForEndOfFrame();

        cardObj.transform.SetParent(targetTile.transform, false);

        RectTransform rect = cardObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;

        activeRow++;
        if (activeRow >= rows.Count)
        {
            activeRow = rows.Count - 1;
        }

        AddNewRow();
        UpdateActiveRow();

        yield return new WaitForEndOfFrame();

        if (cg != null) cg.alpha = 1f;

        LayoutElement le = cardObj.GetComponent<LayoutElement>();
        if (le == null) le = cardObj.AddComponent<LayoutElement>();
        le.ignoreLayout = true;

        if (rowIndex >= 1)
        {
            yield return StartCoroutine(ShiftGridDownSmooth());
            CleanupRowsByPosition();
        }

        DropTile tile = chosenTile;

        if (tile.tileData.type == TileType.Enemy)
        {
            StartCoroutine(EnterBattle());
            yield break; 
        }

        if (tile.hasItem && tile.tileImage != null)
        {
            inventoryManager.AddItem(tile.tileImage);
            tile.hasItem = false;
            tile.tileImage.enabled = false;
        }
    }

    void UpdateActiveRow()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            foreach (var tile in rows[i])
            {
                tile.isLocked = (i != activeRow);
            }
        }
    }

    void LockRow(DropTile chosenTile)
    {
        foreach (var row in rows)
        {
            if (row.Contains(chosenTile))
            {
                foreach (var tile in row)
                {
                    tile.isLocked = true;
                }
                break;
            }
        }
    }

    void EnsureRowsAhead(int currentRow)
    {
        int neededRows = currentRow + bufferRows + 1;

        while (rows.Count < neededRows)
        {
            AddNewRow();
        }
    }

    IEnumerator ShiftGridDownSmooth()
    {
        RectTransform rect = gridParent.GetComponent<RectTransform>();

        Vector2 startPos = rect.anchoredPosition;
        Vector2 targetPos = startPos - new Vector2(0, cellHeight);

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = targetPos;
    }

    void CleanupRowsByPosition()
    {
        RectTransform gridRect = gridParent.GetComponent<RectTransform>();

        float movedDistance = -gridRect.anchoredPosition.y;
        int rowsToRemove = Mathf.FloorToInt(movedDistance / cellHeight) - maxRowsBelow;

        if (rowsToRemove <= 0) return;

        for (int i = 0; i < rowsToRemove; i++)
        {
            if (rows.Count == 0) break;

            RemoveBottomRow();
            activeRow = Mathf.Max(0, activeRow - 1);
        }

        gridRect.anchoredPosition += new Vector2(0, rowsToRemove * cellHeight);
    }
}