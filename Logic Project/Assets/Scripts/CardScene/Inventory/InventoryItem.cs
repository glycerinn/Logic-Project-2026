using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public Transform originalParent;
    public Vector2 originalPosition;
    public Transform dragLayer;
    public InventorySlot currentSlot;
    private Outline outline;

    public bool isSelected = false;
    public bool isPlaced = false;
    public bool wasDropped = false;

    void Awake()
    {
        outline = GetComponent<Outline>();

        if (outline != null)
        {
            outline.enabled = false;
        }

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        dragLayer = GameObject.FindGameObjectWithTag("DragLayer").transform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        currentSlot = GetComponentInParent<InventorySlot>();
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        wasDropped = false;
        
        transform.SetParent(dragLayer, false);

        rectTransform.rotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        rectTransform.rotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (isSelected)
            {
                Deselect();

                MergeManager.Instance.selectedItems.Remove(this);
            }
            else
            {
                Select();

                MergeManager.Instance.selectedItems.Add(this);
            }
        }
    }

    public void Select()
    {
        isSelected = true;

         if (outline != null)
        {
            outline.enabled = true;
        }
    }

    public void Deselect()
    {
        isSelected = false;

        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}