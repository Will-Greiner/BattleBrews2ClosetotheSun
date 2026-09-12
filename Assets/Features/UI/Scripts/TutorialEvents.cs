using System;
using UnityEngine;

public enum TutorialTrigger
{
    RoundActivated,
    IngredientSpawned,
    ItemGrabbed,
    IngredientAdded,
    StirringStarted,
    PotionCreated,
    PotionDelivered,
    ShopOpened,
    OfferPurchased,
    ProcessingIngredientInserted,
    ProcessingStarted,
    ProcessingCompleted,
    AltarSampleInserted,
    PropertyDiscovered,
    RecipeBookOpened,
    RecipeBookClosed
}

public static class TutorialEvents
{
    public static event Action<TutorialTrigger, string> Triggered;

    public static void Report(TutorialTrigger trigger, string id = "")
    {
        Triggered?.Invoke(trigger, id ?? string.Empty);
    }

    public static string GetItemId(GrabbableItem item)
    {
        if (item == null)
            return string.Empty;

        IngredientItem ingredient = item.GetComponent<IngredientItem>();

        if (ingredient != null && ingredient.Data != null)
            return GetIngredientId(ingredient.Data);

        PotionItem potion = item.GetComponent<PotionItem>();

        if (potion != null && potion.Data != null)
            return GetPotionId(potion.Data);

        return item.DisplayName;
    }

    public static string GetIngredientId(IngredientData ingredient)
    {
        if (ingredient == null)
            return string.Empty;

        return !string.IsNullOrWhiteSpace(ingredient.UnlockId) ? ingredient.UnlockId : ingredient.name;
    }

    public static string GetPotionId(PotionData potion)
    {
        return potion != null ? potion.PotionName : string.Empty;
    }
}
