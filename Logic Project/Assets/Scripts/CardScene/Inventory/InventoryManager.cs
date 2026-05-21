using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] slots;

    private int currentIndex = 0;


    void Start()
    {
        slots = GetComponentsInChildren<InventorySlot>(true);
        Debug.Log("Slots found: " + slots.Length);
    }

    public void AddItem(UnityEngine.UI.Image sourceImage)
    {
        if (currentIndex >= slots.Length)
        {
            Debug.Log("Inventory full");
            return;
        }

        slots[currentIndex].SetItem(sourceImage);
        
        currentIndex++;
    }

    public void RemoveItem(int index)
    {
        if(index < 0 || index >= currentIndex)
        {
            return;
        }

        for(int i = 0; i < currentIndex - 1; i++)
        {
            slots[i].item.slotIndex = i;
            slots[i].icon.sprite = slots[i+1].icon.sprite;
            slots[i].icon.color = slots[i+1].icon.color;

        }

        slots[currentIndex - 1].RemoveItem();    
        currentIndex--;   
    }
}