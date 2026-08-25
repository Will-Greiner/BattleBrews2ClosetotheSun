using UnityEngine;

public class GameContentCatalog : MonoBehaviour
{
    public static GameContentCatalog Instance { get; private set; }

    [SerializeField] private IngredientDatabase ingredientDatabase;
    [SerializeField] private PotionDatabase potionDatabase;

    public IngredientDatabase IngredientDatabase => ingredientDatabase;
    public PotionDatabase PotionDatabase => potionDatabase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
