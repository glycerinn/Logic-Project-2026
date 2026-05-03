using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropTile : MonoBehaviour, IDropHandler
{
    public GridManager gridManager;
    public bool isLocked = false;

    // The image shown on the tile
    public Image tileImage;

    // Whether it still has an item
    public bool hasItem = true;

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