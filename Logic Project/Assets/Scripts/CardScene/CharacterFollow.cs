using UnityEngine;
using UnityEngine.UI;

public class CharacterFollower : MonoBehaviour
{
    public RectTransform playerCard;
    RectTransform rect;
    public Vector2 offset;
    public Image characterImage;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (playerCard != null)
        {
            rect.position = (Vector2)playerCard.position + offset;
        }
    }

    public void SetRaycast(bool enabled)
    {
        characterImage.raycastTarget = enabled;
    }
}