using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterPartCategory
{
    public string categoryName;
    public Transform socket;
    public GameObject[] prefabs;
    public bool allowEmptySelection;
}

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("Fighter")]
    [SerializeField] private FighterAnimationController fighterAnimationController;

    [Header("Character Parts")]
    [SerializeField] private CharacterPartCategory[] partCategories;

    [Header("Face Textures")]
    [SerializeField] private Texture2D[] eyeImages;
    [SerializeField] private Texture2D[] pupilImages;
    [SerializeField] private Texture2D[] mouthImages;

    [Header("Face Materials")]
    [SerializeField] private Material eyeMaterial;
    [SerializeField] private Material mouthMaterial;

    [Header("Shader Properties")]
    [SerializeField] private string eyeTextureProperty = "_Eye";
    [SerializeField] private string pupilTextureProperty = "_Pupil";
    [SerializeField] private string mouthTextureProperty = "_BaseMap";

    private readonly List<GameObject> spawnedParts = new();

    private Texture defaultEyeTexture;
    private Texture defaultPupilTexture;
    private Texture defaultMouthTexture;

    public FighterAnimationController CurrentFighter => fighterAnimationController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (fighterAnimationController == null)
            fighterAnimationController = GetComponent<FighterAnimationController>();

        CacheDefaultFaceTextures();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void GenerateCharacter()
    {
        ClearSpawnedParts();

        if (partCategories != null)
        {
            foreach (CharacterPartCategory category in partCategories)
                GeneratePart(category);
        }

        ApplyRandomFaceTextures();
    }

    public void ClearCharacter()
    {
        ClearSpawnedParts();
        RestoreDefaultFaceTextures();
    }

    private void GeneratePart(CharacterPartCategory category)
    {
        if (category == null || category.socket == null || category.prefabs == null || category.prefabs.Length == 0)
            return;

        int selectionCount = category.prefabs.Length + (category.allowEmptySelection ? 1 : 0);
        int selectedIndex = UnityEngine.Random.Range(0, selectionCount);

        if (category.allowEmptySelection && selectedIndex == category.prefabs.Length)
            return;

        GameObject selectedPrefab = category.prefabs[selectedIndex];

        if (selectedPrefab == null)
            return;

        GameObject spawnedPart = Instantiate(selectedPrefab, category.socket.position, category.socket.rotation, category.socket);
        spawnedParts.Add(spawnedPart);
    }

    private void ClearSpawnedParts()
    {
        foreach (GameObject spawnedPart in spawnedParts)
        {
            if (spawnedPart != null)
                Destroy(spawnedPart);
        }

        spawnedParts.Clear();
    }

    private void CacheDefaultFaceTextures()
    {
        if (eyeMaterial != null)
        {
            if (eyeMaterial.HasProperty(eyeTextureProperty))
                defaultEyeTexture = eyeMaterial.GetTexture(eyeTextureProperty);

            if (eyeMaterial.HasProperty(pupilTextureProperty))
                defaultPupilTexture = eyeMaterial.GetTexture(pupilTextureProperty);
        }

        if (mouthMaterial != null && mouthMaterial.HasProperty(mouthTextureProperty))
            defaultMouthTexture = mouthMaterial.GetTexture(mouthTextureProperty);
    }

    private void ApplyRandomFaceTextures()
    {
        if (eyeMaterial != null)
        {
            SetRandomTexture(eyeMaterial, eyeTextureProperty, eyeImages);
            SetRandomTexture(eyeMaterial, pupilTextureProperty, pupilImages);
        }

        if (mouthMaterial != null)
            SetRandomTexture(mouthMaterial, mouthTextureProperty, mouthImages);
    }

    private void RestoreDefaultFaceTextures()
    {
        if (eyeMaterial != null)
        {
            if (eyeMaterial.HasProperty(eyeTextureProperty))
                eyeMaterial.SetTexture(eyeTextureProperty, defaultEyeTexture);

            if (eyeMaterial.HasProperty(pupilTextureProperty))
                eyeMaterial.SetTexture(pupilTextureProperty, defaultPupilTexture);
        }

        if (mouthMaterial != null && mouthMaterial.HasProperty(mouthTextureProperty))
            mouthMaterial.SetTexture(mouthTextureProperty, defaultMouthTexture);
    }

    private void SetRandomTexture(Material material, string propertyName, Texture2D[] textures)
    {
        if (material == null || textures == null || textures.Length == 0 || !material.HasProperty(propertyName))
            return;

        Texture2D selectedTexture = textures[UnityEngine.Random.Range(0, textures.Length)];

        if (selectedTexture != null)
            material.SetTexture(propertyName, selectedTexture);
    }
}