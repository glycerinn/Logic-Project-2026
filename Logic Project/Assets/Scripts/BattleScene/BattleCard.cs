using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleCard : MonoBehaviour
{
    public Image icon;
    private ItemData weapon;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Use);
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