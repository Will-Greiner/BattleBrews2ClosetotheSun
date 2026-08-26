#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class SpawnerVisualStripper
{
    private const string CompletionKey = "BattleBrews.SpawnerVisualStripper.Completed.v1";

    static SpawnerVisualStripper()
    {
        EditorApplication.delayCall += TryRunOnce;
    }

    [MenuItem("Battle Brews/Scene Tools/Strip Ingredient Spawner Visual Prefabs")]
    public static void RunFromMenu()
    {
        int converted = ConvertSceneSpawnerVisuals();
        Debug.Log($"Converted {converted} ingredient spawner visuals into scene-only mesh objects.");
    }

    private static void TryRunOnce()
    {
        if (SessionState.GetBool(CompletionKey, false) || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (EditorSceneManager.GetActiveScene().name != "FirstScene")
            return;

        int converted = ConvertSceneSpawnerVisuals();
        SessionState.SetBool(CompletionKey, true);

        if (converted > 0)
            Debug.Log($"Converted {converted} ingredient spawner visuals into scene-only mesh objects.");
    }

    private static int ConvertSceneSpawnerVisuals()
    {
        int convertedCount = 0;

        foreach (IngredientSpawner spawner in Object.FindObjectsByType<IngredientSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!spawner.gameObject.scene.IsValid())
                continue;

            SerializedObject serializedSpawner = new(spawner);
            IngredientData ingredient = serializedSpawner.FindProperty("ingredient").objectReferenceValue as IngredientData;

            if (ingredient == null || ingredient.Prefab == null)
                continue;

            string ingredientPrefabPath = AssetDatabase.GetAssetPath(ingredient.Prefab);
            HashSet<GameObject> matchingRoots = FindMatchingPrefabRoots(spawner.transform, ingredientPrefabPath);

            foreach (GameObject matchingRoot in matchingRoots)
            {
                if (matchingRoot == null)
                    continue;

                PrefabUtility.UnpackPrefabInstance(matchingRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                StripToVisualComponents(matchingRoot);
                matchingRoot.name = $"{ingredient.IngredientName} Visual";
                convertedCount++;
            }
        }

        if (convertedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        return convertedCount;
    }

    private static HashSet<GameObject> FindMatchingPrefabRoots(Transform spawnerRoot, string ingredientPrefabPath)
    {
        HashSet<GameObject> results = new();

        foreach (Transform descendant in spawnerRoot.GetComponentsInChildren<Transform>(true))
        {
            if (descendant == spawnerRoot || !PrefabUtility.IsPartOfPrefabInstance(descendant.gameObject))
                continue;

            GameObject instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(descendant.gameObject);

            if (instanceRoot == null || !instanceRoot.transform.IsChildOf(spawnerRoot))
                continue;

            GameObject sourceRoot = PrefabUtility.GetCorrespondingObjectFromSource(instanceRoot);

            if (sourceRoot != null && AssetDatabase.GetAssetPath(sourceRoot) == ingredientPrefabPath)
                results.Add(instanceRoot);
        }

        return results;
    }

    private static void StripToVisualComponents(GameObject visualRoot)
    {
        Component[] components = visualRoot.GetComponentsInChildren<Component>(true);

        for (int i = components.Length - 1; i >= 0; i--)
        {
            Component component = components[i];

            if (component == null || component is Transform || component is MeshFilter || component is Renderer)
                continue;

            Object.DestroyImmediate(component);
        }
    }
}
#endif
