using UnityEngine;
using UnityEngine.EventSystems;

public class TrashSlot : MonoBehaviour, IDropHandler
{
    public InventoryManager inventoryManager;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        InventoryItem item = dropped.GetComponent<InventoryItem>();
        if (item == null) return;
        
        InventorySlot slot = item.currentSlot;

        if (slot == null) return;

        inventoryManager.RemoveItem(slot.slotIndex);
    }
}