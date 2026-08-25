using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    public const int SlotCount = 3;
    private const string FilePrefix = "battle_brews_slot_";

    public static bool SlotExists(int slot) => File.Exists(GetPath(slot));

    public static SaveGameData Load(int slot)
    {
        ValidateSlot(slot);
        string path = GetPath(slot);

        if (!File.Exists(path))
            return null;

        try
        {
            return JsonUtility.FromJson<SaveGameData>(File.ReadAllText(path));
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not load save slot {slot}: {exception.Message}");
            return null;
        }
    }

    public static bool Save(int slot, SaveGameData data)
    {
        ValidateSlot(slot);

        if (data == null)
            return false;

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            string path = GetPath(slot);
            string temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(data, true));

            if (File.Exists(path))
                File.Delete(path);

            File.Move(temporaryPath, path);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not save slot {slot}: {exception.Message}");
            return false;
        }
    }

    public static void Delete(int slot)
    {
        ValidateSlot(slot);
        string path = GetPath(slot);

        if (File.Exists(path))
            File.Delete(path);
    }

    public static IReadOnlyList<SaveGameData> LoadAllSlots()
    {
        List<SaveGameData> saves = new(SlotCount);

        for (int slot = 1; slot <= SlotCount; slot++)
            saves.Add(Load(slot));

        return saves;
    }

    private static string GetPath(int slot)
    {
        ValidateSlot(slot);
        return Path.Combine(Application.persistentDataPath, $"{FilePrefix}{slot}.json");
    }

    private static void ValidateSlot(int slot)
    {
        if (slot < 1 || slot > SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slot), $"Save slot must be between 1 and {SlotCount}.");
    }
}
