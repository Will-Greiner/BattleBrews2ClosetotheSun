using UnityEngine;

[CreateAssetMenu(fileName = "EncounterData", menuName = "Scriptable Objects/EncounterData")]
public class EncounterData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string encounterName;
    [SerializeField] private string opponentName;

    [Header("Potion Selection")]
    [SerializeField] private bool selectRandomPotion = true;
    [SerializeField] private PotionData bestPotion;

    [Header("Round Availability")]
    [Min(1)]
    [SerializeField] private int firstRound = 1;

    [Min(1)]
    [SerializeField] private int lastRound = 99;

    [Header("Dialogue")]
    [TextArea(3, 6)]
    [SerializeField] private string requestDialogue = "I'm fighting {opponent} in {encounter}. Brew me a {potion}!";

    [Header("Outcome Text")]
    [TextArea(3, 6)]
    [SerializeField] private string winOutcomeText;

    [TextArea(3, 6)]
    [SerializeField] private string loseOutcomeText;

    public string EncounterName => encounterName;
    public string OpponentName => opponentName;
    public bool SelectRandomPotion => selectRandomPotion;
    public PotionData BestPotion => bestPotion;
    public int FirstRound => firstRound;
    public int LastRound => lastRound;
    public string RequestDialogue => requestDialogue;
    public string WinOutcomeText => winOutcomeText;
    public string LoseOutcomeText => loseOutcomeText;

    public bool IsAvailableInRound(int round)
    {
        return round >= firstRound && round <= lastRound;
    }

    public PotionData SelectRequestedPotion(int round, PotionDatabase potionDatabase)
    {
        if (!selectRandomPotion)
            return bestPotion;

        if (potionDatabase == null)
        {
            Debug.LogError($"{name} cannot select a random potion because no PotionDatabase was provided.", this);
            return null;
        }

        PotionData selectedPotion = potionDatabase.GetRandomAvailablePotion(round);

        if (selectedPotion == null)
            Debug.LogError($"{name} could not find a potion available during round {round}.", this);

        return selectedPotion;
    }

    public string BuildRequestDialogue(PotionData requestedPotion)
    {
        string potionName = requestedPotion != null ? requestedPotion.PotionName : "potion";

        if (string.IsNullOrWhiteSpace(requestDialogue))
            return $"I'm fighting {opponentName} in {encounterName}. Brew me a {potionName}!";

        return requestDialogue.Replace("{encounter}", encounterName).Replace("{opponent}", opponentName).Replace("{potion}", potionName);
    }

    private void OnValidate()
    {
        firstRound = Mathf.Max(1, firstRound);
        lastRound = Mathf.Max(firstRound, lastRound);
    }
}