using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Game/Recipe")]
public class RecipeData : ScriptableObject
{
    public ItemData ingredientA;
    public ItemData ingredientB;

    public ItemData result;
}