using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Game/Recipe")]
public class RecipeData : ScriptableObject
{
    public ItemData[] ingredients;

    public ItemData result;
}