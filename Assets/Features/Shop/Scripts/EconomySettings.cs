using UnityEngine;

[CreateAssetMenu(fileName = "EconomySettings", menuName = "Battle Brews/Economy Settings")]
public class EconomySettings : ScriptableObject
{
    [Min(0)] [SerializeField] private int successReward = 100;
    [Min(0)] [SerializeField] private int failureReward = 20;
    [Min(0)] [SerializeField] private int additionalRewardPerRound = 10;

    public int GetRoundReward(BattleOutcome outcome, int round)
    {
        int baseReward = outcome == BattleOutcome.Win ? successReward : failureReward;
        return baseReward + Mathf.Max(0, round - 1) * additionalRewardPerRound;
    }
}
