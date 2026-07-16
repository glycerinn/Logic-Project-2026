using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BattleCard : MonoBehaviour
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
    }


    public void Setup(ItemData item)
    {
        weapon = item;
        icon.sprite = item.icon;
    }

    public void Use()
    {
        BattleManager.Instance.SelectWeapon(weapon);
    }
}