using UnityEngine;
using UnityEngine.EventSystems;

public class DropTile : MonoBehaviour, IDropHandler
{
    public GridManager gridManager;
    public bool isLocked = false;

    public void OnDrop(PointerEventData eventData)
    {
        if (isLocked) return;

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;
        
        DraggableCard card = dropped.GetComponent<DraggableCard>();
        if (card != null && card.isPlaced) return;
        if (card != null)
        {
            card.wasDropped = true;
            card.isPlaced = true;
        }

        // Let GridManager decide where it goes
        gridManager.OnTileChosen(this, dropped);
        card.originalParent = dropped.transform.parent;
        card.originalPosition = dropped.GetComponent<RectTransform>().anchoredPosition;
    }
}