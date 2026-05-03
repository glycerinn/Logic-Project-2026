using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] slots;

    void Start()
    {
        slots = GetComponentsInChildren<InventorySlot>(true);
        Debug.Log("Slots found: " + slots.Length);
    }

    public void AddItem(Sprite sprite)
    {
        Debug.Log("AddItem called with sprite: " + sprite);

        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(sprite);
                return;
            }
        }

        Debug.Log("Inventory full");
    }
}