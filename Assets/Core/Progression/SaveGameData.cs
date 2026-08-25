using System;
using System.Collections.Generic;

[Serializable]
public sealed class SaveGameData
{
    public int version = 1;
    public string createdUtc;
    public string lastPlayedUtc;
    public int currentRound;
    public int lives;
    public int currency;
    public List<string> unlockedContentIds = new();
    public List<string> discoveredPropertyIds = new();
    public List<UpgradeSaveData> upgrades = new();
}

[Serializable]
public sealed class UpgradeSaveData
{
    public string id;
    public int level;
}
