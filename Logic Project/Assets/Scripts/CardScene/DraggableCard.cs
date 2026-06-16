using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableCard : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public Transform originalParent;
    public Vector2 originalPosition;
    public Transform dragLayer;
    public Image avatar;

    public bool isPlaced = false;
    public bool wasDropped = false;
    private AudioManager audioManager;

    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager")?.GetComponent<AudioManager>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        Debug.Log("BEGIN DRAG");
        avatar.raycastTarget = false;
        audioManager?.playpickupSFX();
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        wasDropped = false;
        
        transform.SetParent(dragLayer, false);

        rectTransform.position = eventData.position;
        rectTransform.localRotation = Quaternion.Euler(0, 0, 0);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform canvasRect =
            dragLayer.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        rectTransform.anchoredPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        audioManager?.playDropSFX();
        avatar.raycastTarget = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        rectTransform.rotation = Quaternion.identity;

        if (isPlaced)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            return;
        }

        if (!wasDropped)
        {
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}