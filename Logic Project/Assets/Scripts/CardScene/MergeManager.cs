using UnityEngine;
using System.Collections.Generic;

public class MergeManager : MonoBehaviour
{
    public RecipeData[] recipes;
    public static MergeManager Instance;
    public InventoryManager inventoryManager;
    public List<InventoryItem> selectedItems =
    new List<InventoryItem>();

    void Awake()
{
    Instance = this;

    Debug.Log(
        "MergeManager on: " + gameObject.name
    );

    Debug.Log(
        "Recipes Length: " + recipes.Length
    );
}

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            AttemptMerge();
        }
    }

    public void TryCraft(List<ItemData> selectedItems)
    {
        Debug.Log("Recipe Count: " + recipes.Length);
        foreach (RecipeData recipe in recipes)
        {
            if (RecipeMatches(selectedItems, recipe.ingredients))
            {
                Debug.Log("Crafted: " + recipe.result.itemName);

                // remove all ingredients
                foreach (ItemData item in recipe.ingredients)
                {
                    inventoryManager.RemoveItemByData(item);
                }

                // add crafted item
                inventoryManager.AddItem(recipe.result);

                return;
            }
        }

        Debug.Log("No recipe found");
    }

   bool RecipeMatches(
    List<ItemData> selected,
    ItemData[] recipeIngredients)
{
    Debug.Log("=== RECIPE CHECK ===");

    foreach (ItemData item in selected)
    {
        Debug.Log(
            "Selected: " +
            item.itemName +
            " | " +
            item.GetInstanceID()
        );
    }

    foreach (ItemData item in recipeIngredients)
    {
        Debug.Log(
            "Recipe: " +
            item.itemName +
            " | " +
            item.GetInstanceID()
        );
    }

    if (selected.Count != recipeIngredients.Length)
    {
        Debug.Log("COUNT FAILED");
        return false;
    }

    List<ItemData> temp =
        new List<ItemData>(selected);

    foreach (ItemData ingredient in recipeIngredients)
    {
        if (!temp.Contains(ingredient))
        {
            Debug.Log(
                "FAILED ON: " +
                ingredient.itemName
            );

            return false;
        }

        temp.Remove(ingredient);
    }

    Debug.Log("MATCHED");

    return true;
} 

    void AttemptMerge()
    {
        
        if (selectedItems.Count <= 1)
        {
            ClearSelection();
            return;
        }

        List<ItemData> items =
            new List<ItemData>();

        foreach (InventoryItem item in selectedItems)
        {
            InventorySlot slot = item.currentSlot;

            if (
                slot != null &&
                slot.currentItem != null
            )
            {
                items.Add(slot.currentItem);

                Debug.Log(
                    "Added: " +
                    slot.currentItem.itemName
                );
            }

            Debug.Log(
                slot.currentItem == null
                    ? "NULL ITEM"
                    : slot.currentItem.itemName
            );
        }

        TryCraft(items);

        ClearSelection();
    }

    void ClearSelection()
    {
        foreach (InventoryItem item in selectedItems)
        {
            item.Deselect();
        }

        selectedItems.Clear();
    }
}