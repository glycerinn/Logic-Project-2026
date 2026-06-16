using UnityEngine;

public class CharacterFollower : MonoBehaviour
{
    public RectTransform playerCard;
    RectTransform rect;
    public Vector2 offset;

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
}