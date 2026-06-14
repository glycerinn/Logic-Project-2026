using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public InventoryItem item;
    public TextMeshProUGUI durabilityText;

    public ItemData currentItem;
    public int durability;

    public int slotIndex;

    void Awake()
    {
        item = GetComponentInChildren<InventoryItem>();
    }

    public void SetItem(ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning("ItemData is NULL");
            return;
        }

        currentItem = itemData;
        durability = currentItem.maxDurability;

        icon.enabled = true;
        icon.sprite = itemData.icon;

        item.currentSlot = this;

        icon.preserveAspect = true;

        icon.rectTransform.localScale = Vector3.one;
        icon.rectTransform.anchoredPosition = Vector2.zero;
        icon.rectTransform.rotation = Quaternion.identity;
    }

    public void RemoveItem()
    {
        currentItem = null;

        icon.sprite = null;
        icon.enabled = false;
        icon.color = Color.white;

        icon.rectTransform.localScale = Vector3.one;
        icon.rectTransform.anchoredPosition = Vector2.zero;
        icon.rectTransform.rotation = Quaternion.identity;
    }

    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public void CopyFrom(InventorySlot other)
    {
        currentItem = other.currentItem;

        icon.sprite = other.icon.sprite;
        icon.color = other.icon.color;
        icon.enabled = other.icon.enabled;
    }

    public void UpdateDurabilityUI()
    {
        if (currentItem == null)
        {
            durabilityText.text = "";
            return;
        }

        durabilityText.text = durability.ToString();
    }

}