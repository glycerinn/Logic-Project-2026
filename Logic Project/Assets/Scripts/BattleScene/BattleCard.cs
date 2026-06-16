using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BattleCard : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    private ItemData weapon;

    void Awake()
    {
        Debug.Log(gameObject.name + " Awake");

        Button btn = GetComponent<Button>();

        if (btn == null)
        {
            Debug.LogError("NO BUTTON FOUND");
            return;
        }

        btn.onClick.AddListener(Use);

        Debug.Log("Listener Added");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("POINTER HIT CARD");
    }


    public void Setup(ItemData item)
    {
        weapon = item;
        icon.sprite = item.icon;
    }

    public void Use()
    {
        Debug.Log("CARD CLICKED");
        BattleManager.Instance.SelectWeapon(weapon);
    }
}