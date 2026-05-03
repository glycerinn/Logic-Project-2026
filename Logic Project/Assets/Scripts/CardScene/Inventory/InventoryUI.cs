using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotParent;

    public int slotCount = 8;

    void Start()
    {
        GenerateSlots();
    }

    void GenerateSlots()
    {
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, slotParent);
        }
    }
}