using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] slots;
    public ItemData[] starter;

    private int currentIndex = 0;

    void Start()
    {
        slots = GetComponentsInChildren<InventorySlot>(true);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].slotIndex = i;
        }

        foreach (ItemData item in starter)
        {
            AddItem(item);
        }
        Debug.Log("Slots found: " + slots.Length);
    }

    public void AddItem(ItemData item)
    {
        if (currentIndex >= slots.Length)
            return;

        slots[currentIndex].SetItem(item);

        currentIndex++;
    }

    public void RemoveItem(int index)
    {
        if(index < 0 || index >= currentIndex)
        {
            return;
        }

        for(int i = index; i < currentIndex - 1; i++)
        {
            slots[i].CopyFrom(slots[i + 1]);
        }

        slots[currentIndex - 1].RemoveItem();    
        currentIndex--;   
    }

    public void RemoveItemByData(ItemData item)
    {
        for (int i = 0; i < currentIndex; i++)
        {
            if (slots[i].currentItem == item)
            {
                RemoveItem(i);
                return;
            }
        }
    }

    
}