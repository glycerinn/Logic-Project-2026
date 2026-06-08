using UnityEngine;

public enum ItemType
{
    Material,
    Weapon
}

[CreateAssetMenu(fileName = "Item", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;

    public ItemType itemType;

    public Sprite icon;

    public int damage;

    public int maxDurability = 10;
}