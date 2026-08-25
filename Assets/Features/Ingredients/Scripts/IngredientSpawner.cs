using UnityEngine;

public class IngredientSpawner : MonoBehaviour, IHandInteractable
{
    [Header("Ingredient")]
    [SerializeField] private IngredientData ingredient;

    public bool CanInteract(GrabController grabController)
    {
        if (grabController == null || grabController.IsHoldingItem)
            return false;

        if (ingredient == null || ingredient.Prefab == null)
            return false;

        ContentAvailabilityService availability = ContentAvailabilityService.Instance;
        return availability == null || availability.IsIngredientUnlocked(ingredient);
    }

    public void Interact(GrabController grabController)
    {
        if (!CanInteract(grabController))
            return;

        GameObject spawnedObject = Instantiate(ingredient.Prefab, grabController.transform.position, grabController.transform.rotation);
        GrabbableItem grabbableItem = spawnedObject.GetComponent<GrabbableItem>();

        if (grabbableItem == null)
        {
            Debug.LogError($"{spawnedObject.name} does not have a GrabbableItem component.", spawnedObject);
            Destroy(spawnedObject);
            return;
        }

        IngredientItem ingredientItem = spawnedObject.GetComponent<IngredientItem>();

        if (ingredientItem == null)
        {
            Debug.LogError($"{spawnedObject.name} does not have an IngredientItem component.", spawnedObject);
            Destroy(spawnedObject);
            return;
        }

        Vector3 grabPointOffset = grabbableItem.GrabPoint.position - spawnedObject.transform.position;
        spawnedObject.transform.position -= grabPointOffset;

        if (!grabController.Grab(grabbableItem))
            Destroy(spawnedObject);
    }

    public string GetInteractionPrompt(GrabController grabController)
    {
        if (ingredient == null)
            return string.Empty;

        ContentAvailabilityService availability = ContentAvailabilityService.Instance;

        if (availability != null && !availability.IsIngredientUnlocked(ingredient))
            return $"{ingredient.IngredientName} dispenser locked";

        return $"Take {ingredient.IngredientName}";
    }
}
