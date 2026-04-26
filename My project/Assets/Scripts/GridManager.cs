using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform gridParent;

    public int width = 4;
    public int visibleRows = 5;

    private List<List<DropTile>> rows = new List<List<DropTile>>();

    void Start()
    {
        for (int i = 0; i < visibleRows; i++)
        {
            AddNewRow();
        }

        UpdateFrontRow();
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
        
        // 🔍 find row + column
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
        if (rowIndex + 1 >= rows.Count) yield break;

        DropTile targetTile = rows[rowIndex + 1][colIndex];

        // 🧠 hide card to prevent flicker
        CanvasGroup cg = cardObj.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        // ⏳ wait one frame so layout system finishes
        yield return null;

        // 🎯 move card ONLY once, cleanly
        cardObj.transform.SetParent(targetTile.transform, false);

        RectTransform rect = cardObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;

        // 🔄 advance grid AFTER placement
        RemoveBottomRow();
        AddNewRow();
        UpdateFrontRow();

        // ⏳ wait one more frame so grid settles
        yield return null;

        // 👁️ show card again
        if (cg != null) cg.alpha = 1f;

        LayoutElement le = cardObj.GetComponent<LayoutElement>();
        if (le == null) le = cardObj.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }

    void UpdateFrontRow()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            foreach (var tile in rows[i])
            {
                tile.isLocked = (i != 0); // bottom row = playable
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
}