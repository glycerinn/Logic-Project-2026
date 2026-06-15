using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropTile : MonoBehaviour, IDropHandler
{
    public GridManager gridManager;
    public Image tileImage;
    public TileData tileData;

    public bool hasItem = true;
    public bool isLocked = false;
    public bool isBossTile;
    public bool isEnemy;

    void Start()
    {
        ApplyVisuals();
    }

    public void ApplyVisuals()
    {
        if (tileData == null || tileImage == null) return;

        tileImage.sprite = tileData.sprite;
        tileImage.color = tileData.color;

        if (tileData.type == TileType.Enemy)
        {
            tileImage.rectTransform.localEulerAngles = Vector3.zero;
        }
        else
        {
            tileImage.rectTransform.localEulerAngles = new Vector3(0, 0, -90);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (isLocked) return;

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;
        
        DraggableCard card = dropped.GetComponent<DraggableCard>();
        if (card == null) return;
        if (card != null)
        {
            card.wasDropped = true;
        }

        // Let GridManager decide where it goes
        gridManager.OnTileChosen(this, dropped);
        card.originalParent = dropped.transform.parent;
        card.originalPosition = dropped.GetComponent<RectTransform>().anchoredPosition;
    }
}