#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ShopSceneSetup
{
    private const string PrefabFolder = "Assets/Features/Shop/Prefabs";
    private const string PrefabPath = PrefabFolder + "/ShopOfferRow.prefab";

    [MenuItem("Battle Brews/Setup Shop UI In FirstScene")]
    public static void SetupFromMenu() => Setup();

    private static bool Setup()
    {
        ShopUI shopUI = FindSceneShopUI();

        if (shopUI == null)
            return false;

        SerializedObject serializedUI = new(shopUI);
        TMP_Text currency = serializedUI.FindProperty("currencyText").objectReferenceValue as TMP_Text;
        Transform offerContainer = serializedUI.FindProperty("offerContainer").objectReferenceValue as Transform;

        if (currency == null || offerContainer == null || offerContainer.parent is not RectTransform canvasRoot)
            return false;

        ConfigureCanvas(canvasRoot, currency, offerContainer as RectTransform);

        ShopOfferRowUI prefab = AssetDatabase.LoadAssetAtPath<ShopOfferRowUI>(PrefabPath);
        if (prefab == null)
            prefab = CreateOfferPrefab(currency.font);

        Button closeButton = GetOrCreateCloseButton(canvasRoot, currency.font);
        serializedUI.FindProperty("offerRowPrefab").objectReferenceValue = prefab;
        serializedUI.FindProperty("closeButton").objectReferenceValue = closeButton;
        serializedUI.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(shopUI);
        EditorSceneManager.MarkSceneDirty(shopUI.gameObject.scene);
        EditorSceneManager.SaveScene(shopUI.gameObject.scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Shop UI was created in FirstScene and is now fully editable in the Inspector.", shopUI);
        return true;
    }

    private static ShopUI FindSceneShopUI()
    {
        foreach (ShopUI shopUI in Resources.FindObjectsOfTypeAll<ShopUI>())
            if (shopUI.gameObject.scene.IsValid() && shopUI.gameObject.scene.name == "FirstScene") return shopUI;
        return null;
    }

    private static void ConfigureCanvas(RectTransform root, TMP_Text currency, RectTransform offers)
    {
        Image backdrop = GetOrCreateImage(root, "ShopBackdrop");
        Stretch(backdrop.rectTransform);
        backdrop.color = new Color(0.055f, 0.035f, 0.025f, 0.94f);
        backdrop.transform.SetAsFirstSibling();

        TMP_Text title = root.Find("ShopTitle")?.GetComponent<TMP_Text>();
        bool createdTitle = title == null;
        if (createdTitle) title = CreateText(root, "ShopTitle", currency.font);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(720f, 72f));
        if (createdTitle) title.text = "THE UNDERGRATE";
        title.fontSize = 46f;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(0.96f, 0.77f, 0.34f);

        TMP_Text subtitle = root.Find("ShopSubtitle")?.GetComponent<TMP_Text>();
        bool createdSubtitle = subtitle == null;
        if (createdSubtitle) subtitle = CreateText(root, "ShopSubtitle", currency.font);
        SetRect(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(700f, 38f));
        if (createdSubtitle) subtitle.text = "Rare tools. Questionable provenance. No refunds.";
        subtitle.fontSize = 20f;
        subtitle.fontStyle = FontStyles.Italic;
        subtitle.alignment = TextAlignmentOptions.Center;
        subtitle.color = new Color(0.78f, 0.70f, 0.60f);

        SetRect(currency.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-48f, -44f), new Vector2(270f, 54f));
        currency.rectTransform.pivot = new Vector2(1f, 1f);
        currency.fontSize = 29f;
        currency.fontStyle = FontStyles.Bold;
        currency.alignment = TextAlignmentOptions.MidlineRight;
        currency.color = new Color(1f, 0.86f, 0.42f);

        SetRect(offers, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(900f, 430f));
        VerticalLayoutGroup layout = offers.GetComponent<VerticalLayoutGroup>();
        if (layout == null) layout = Undo.AddComponent<VerticalLayoutGroup>(offers.gameObject);
        layout.padding = new RectOffset(20, 20, 14, 14);
        layout.spacing = 14f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    private static ShopOfferRowUI CreateOfferPrefab(TMP_FontAsset font)
    {
        Directory.CreateDirectory(PrefabFolder);
        GameObject root = new("ShopOfferRow", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(ShopOfferRowUI));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(860f, 112f);
        root.GetComponent<Image>().color = new Color(0.13f, 0.085f, 0.055f, 0.98f);
        LayoutElement element = root.GetComponent<LayoutElement>();
        element.preferredHeight = 112f;
        element.flexibleWidth = 1f;

        Image icon = CreateImage(root.transform, "Icon");
        SetRect(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(72f, 72f));
        icon.rectTransform.pivot = new Vector2(0f, 0.5f);
        icon.preserveAspect = true;

        TMP_Text name = CreateText(root.transform, "OfferName", font);
        SetRect(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(120f, 25f), new Vector2(390f, 38f));
        name.rectTransform.pivot = new Vector2(0f, 0.5f);
        name.fontSize = 27f;
        name.fontStyle = FontStyles.Bold;
        name.alignment = TextAlignmentOptions.MidlineLeft;
        name.color = new Color(0.98f, 0.82f, 0.42f);

        TMP_Text description = CreateText(root.transform, "Description", font);
        SetRect(description.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(120f, -24f), new Vector2(500f, 48f));
        description.rectTransform.pivot = new Vector2(0f, 0.5f);
        description.fontSize = 17f;
        description.alignment = TextAlignmentOptions.TopLeft;
        description.color = new Color(0.86f, 0.80f, 0.71f);

        Button purchase = CreateButton(root.transform, "PurchaseButton", new Color(0.27f, 0.40f, 0.20f, 1f));
        SetRect(purchase.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-22f, 0f), new Vector2(190f, 64f));
        purchase.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
        TMP_Text price = CreateText(purchase.transform, "Price", font);
        Stretch(price.rectTransform);
        price.fontSize = 22f;
        price.fontStyle = FontStyles.Bold;
        price.alignment = TextAlignmentOptions.Center;
        price.color = new Color(1f, 0.92f, 0.62f);

        SerializedObject serializedRow = new(root.GetComponent<ShopOfferRowUI>());
        serializedRow.FindProperty("icon").objectReferenceValue = icon;
        serializedRow.FindProperty("nameText").objectReferenceValue = name;
        serializedRow.FindProperty("descriptionText").objectReferenceValue = description;
        serializedRow.FindProperty("priceText").objectReferenceValue = price;
        serializedRow.FindProperty("purchaseButton").objectReferenceValue = purchase;
        serializedRow.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<ShopOfferRowUI>();
    }

    private static Button GetOrCreateCloseButton(RectTransform root, TMP_FontAsset font)
    {
        Transform existing = root.Find("LeaveShopButton");
        Button button = existing != null ? existing.GetComponent<Button>() : null;
        if (button == null) button = CreateButton(root, "LeaveShopButton", new Color(0.34f, 0.15f, 0.10f, 0.98f));
        SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(300f, 62f));
        button.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
        TMP_Text label = button.transform.Find("Label")?.GetComponent<TMP_Text>();
        bool createdLabel = label == null;
        if (createdLabel) label = CreateText(button.transform, "Label", font);
        Stretch(label.rectTransform);
        if (createdLabel) label.text = "RETURN TO THE BREWERY";
        label.fontSize = 22f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1f, 0.88f, 0.68f);
        return button;
    }

    private static Image GetOrCreateImage(Transform parent, string name) => parent.Find(name)?.GetComponent<Image>() ?? CreateImage(parent, name);
    private static TMP_Text GetOrCreateText(Transform parent, string name, TMP_FontAsset font) => parent.Find(name)?.GetComponent<TMP_Text>() ?? CreateText(parent, name, font);

    private static Image CreateImage(Transform parent, string name)
    {
        GameObject gameObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        return gameObject.GetComponent<Image>();
    }

    private static TMP_Text CreateText(Transform parent, string name, TMP_FontAsset font)
    {
        GameObject gameObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        gameObject.transform.SetParent(parent, false);
        TMP_Text text = gameObject.GetComponent<TMP_Text>();
        text.font = font;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, Color color)
    {
        Image image = CreateImage(parent, name);
        image.color = color;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
#endif
