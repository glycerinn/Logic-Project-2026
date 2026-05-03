using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;

    void Awake()
    {
        icon.enabled = false; // start empty
    }

    public bool IsEmpty()
    {
        return !icon.enabled;
    }

    public void SetItem(Sprite sprite)
    {
        Debug.Log("Setting item: " + sprite);

        icon.sprite = sprite;
        icon.enabled = true;

        icon.rectTransform.localScale = Vector3.one;
        icon.rectTransform.anchoredPosition = Vector2.zero;
        icon.preserveAspect = true;
    }
}