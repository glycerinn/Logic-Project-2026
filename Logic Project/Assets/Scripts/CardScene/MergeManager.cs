using UnityEngine;

public class MergeManager : MonoBehaviour
{
    public RecipeData[] recipes;

    public InventoryManager inventoryManager;

    public void TryCraft(ItemData a, ItemData b)
    {
        foreach (RecipeData recipe in recipes)
        {
            bool matches =
                (recipe.ingredientA == a && recipe.ingredientB == b)
                ||
                (recipe.ingredientA == b && recipe.ingredientB == a);

            if (matches)
            {
                Debug.Log("Crafted: " + recipe.result.itemName);

                inventoryManager.RemoveItemByData(a);
                inventoryManager.RemoveItemByData(b);

                inventoryManager.AddItem(recipe.result);

                return;
            }
        }

        Debug.Log("No recipe found");
    }
}