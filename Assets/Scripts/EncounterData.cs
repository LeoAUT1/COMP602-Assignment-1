using UnityEngine;

[CreateAssetMenu(fileName = "EncounterData", menuName = "Game/Encounter")]
public class EncounterData : ScriptableObject
{
    public string encounterName;
    public string description;
    public bool isFinalBoss; // True if you want beating this encounter to win the game

    [Header("Combat Setup")]
    public Enemy[] enemies;

    [Header("Rewards")]
    public int grantedCoins;
    public int grantedExperience;
}
