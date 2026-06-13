using UnityEngine;

public enum TileType
{
    Material,
    Enemy,
    Weapon
}

[CreateAssetMenu(fileName = "Tile", menuName = "Game/Tile")]
public class TileData : ScriptableObject
{
    public string tileName;

    public TileType type;

    public Sprite sprite;
    public Color color = Color.white;

    [Header("Enemy Stats")]
    public int maxHP;
    public int attack;

    public ItemData itemReward;
}