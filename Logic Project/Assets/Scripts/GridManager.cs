using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform gridParent;
    private int activeRow = 0;
    private float cellHeight;
    public int maxRowsBelow = 3;
    public float cleanupYThreshold = -200f;

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

        for (int x = 0; x < width; x++)
        {
            GameObject tileObj = Instantiate(tilePrefab, gridParent);

            DropTile tile = tileObj.GetComponent<DropTile>();
            tile.gridManager = this;
            tile.isLocked = true;

            newRow.Add(tile);
        }

        rows.Add(newRow);
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
            ShiftGridDown();
            CleanupRowsByPosition();
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

    void ShiftGridDown()
    {
        RectTransform rect = gridParent.GetComponent<RectTransform>();

        rect.anchoredPosition -= new Vector2(0, cellHeight);
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