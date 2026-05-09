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

    public void RemoveItem()
    {
        slots[--currentIndex].RemoveItem();
    }
}