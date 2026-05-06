using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;

    public void SetItem(Image sourceImage)
    {
        if (sourceImage == null)
        {
            Debug.LogWarning("Source image is NULL");
            return;
        }

        // Copy BOTH sprite and color
        icon.sprite = sourceImage.sprite;
        icon.color = sourceImage.color;

        // Prevent stretching
        icon.preserveAspect = true;

        // Reset transform (important for UI)
        icon.rectTransform.localScale = Vector3.one;
        icon.rectTransform.anchoredPosition = Vector2.zero;
        icon.rectTransform.rotation = Quaternion.identity;
    }
}