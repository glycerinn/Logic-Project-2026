using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform gridParent;
    public InventoryManager inventoryManager;
    private int activeRow = 0;
    private float cellHeight;
    public int maxRowsBelow = 3;
    public float cleanupYThreshold = -200f;
    int totalRowsCreated = 0;

    public int width = 4;
    public int visibleRows = 5;
    int bufferRows = 3;

    private List<List<DropTile>> rows = new List<List<DropTile>>();

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

        for (int x = 0; x < width; x++)
        {
            GameObject tileObj = Instantiate(tilePrefab, gridParent);

            DropTile tile = tileObj.GetComponent<DropTile>();
            tile.gridManager = this;
            tile.isLocked = true;

            Image img = tileObj.GetComponent<Image>();
            if (img != null)
            {
                bool isBlue = (rowIndex % 2) == 0;
                img.color = isBlue ? Color.lightBlue : Color.white;
            }

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

    public void OnTileChosen(DropTile chosenTile, GameObject cardObj)
    {
        LockRow(chosenTile);
        StartCoroutine(HandleMove(chosenTile, cardObj));
    }

    IEnumerator HandleMove(DropTile chosenTile, GameObject cardObj)
    {
        
        // find row + column
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

        // hide card to prevent flicker
        CanvasGroup cg = cardObj.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        LayoutRebuilder.ForceRebuildLayoutImmediate(gridParent.GetComponent<RectTransform>());

        yield return new WaitForEndOfFrame();

        // move card ONLY once, cleanly
        
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
        
        // advance grid AFTER placement
        AddNewRow();
        UpdateActiveRow();

        yield return new WaitForEndOfFrame();

        // show card again
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

        if (tile.hasItem && tile.tileImage != null)
        {
            inventoryManager.AddItem(tile.tileImage.sprite);

            tile.hasItem = false;

            // optional: hide tile image after collecting
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
                    Debug.Log("Locking row");
                    tile.isLocked = true;
                }
                break;
            }
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

        rect.anchoredPosition = targetPos; // snap exactly at end
    }

    void EnsureRowsAhead(int currentRow)
    {
        int neededRows = currentRow + bufferRows + 1;

        while (rows.Count < neededRows)
        {
            AddNewRow();
        }
    }    

    void CleanupRowsByPosition()
    {
        RectTransform gridRect = gridParent.GetComponent<RectTransform>();

        // how far we've moved down in "rows"
        float movedDistance = -gridRect.anchoredPosition.y;

        int rowsToRemove = Mathf.FloorToInt(movedDistance / cellHeight) - maxRowsBelow;

        if (rowsToRemove <= 0) return;

        for (int i = 0; i < rowsToRemove; i++)
        {
            if (rows.Count == 0) break;

            RemoveBottomRow();

            // 🔥 FIX: keep activeRow in sync
            activeRow = Mathf.Max(0, activeRow - 1);
        }

        // 🔥 reset grid position so it doesn't go infinitely down
        gridRect.anchoredPosition += new Vector2(0, rowsToRemove * cellHeight);
    }
}