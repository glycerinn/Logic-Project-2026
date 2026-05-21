using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardCarousel : MonoBehaviour
{
    public List<RectTransform> cards = new List<RectTransform>();

    [Header("Shape")]
    public float radius = 500f;
    public float angleSpread = 120f;

    [Header("Movement")]
    public float rotationSpeed = 5f;

    [Header("Scale")]
    public float minScale = 0.9f;
    public float maxScale = 1.1f;

    private float currentRotation = 0f;
    private float targetRotation = 0f;

    public float minRotation = -40f;
    public float maxRotation = 40f;
    public float scrollSpeed = 10f;

    void Start()
    {
        ArrangeCards();
    }

    void Update()
    {
        // Mouse wheel
        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0)
        {
            RotateLeft();
        }
        else if (scroll < 0)
        {
            RotateRight();
        }

        currentRotation += Input.mouseScrollDelta.y * scrollSpeed;

        currentRotation = Mathf.Clamp(currentRotation, minRotation, maxRotation);

        ArrangeCards();
    }

    public void RotateLeft()
    {
        targetRotation += angleSpread / cards.Count;
    }

    public void RotateRight()
    {
        targetRotation -= angleSpread / cards.Count;
    }

    void ArrangeCards()
    {
        if (cards.Count == 0) return;

        float step = angleSpread / (cards.Count - 1);
        float startAngle = -angleSpread / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            float angle = startAngle + (step * i) + currentRotation;

            RectTransform card = cards[i];

            // shared pivot point
            card.pivot = new Vector2(0.5f, 0f);

            // fan rotation
            card.localEulerAngles = new Vector3(0, 0, angle);

            // optional slight scale toward center
            float normalizedAngle = Mathf.DeltaAngle(0, angle);
            float raiseAmount = Mathf.Lerp(60f, 0f, normalizedAngle / angleSpread);
            float centerAmount = 1f - Mathf.Abs(normalizedAngle / angleSpread);
            centerAmount = Mathf.Clamp01(centerAmount);
            float scale = Mathf.Lerp(minScale, maxScale, centerAmount);

            card.localScale = Vector3.one * scale;

            // all cards originate from center
            card.anchoredPosition = new Vector2(0, raiseAmount);

            // render order
            card.SetSiblingIndex(i);
        }
    }
}