using UnityEngine;

public enum EncounterType
{
    COMBAT, TRAP, SKILLCHECK, BOON
}

[CreateAssetMenu(fileName = "EncounterData", menuName = "Game/Encounter")]
public class EncounterData : ScriptableObject
{
    public string encounterName;
    public string description;
    public bool isFinalBoss; // True if you want beating this encounter to win the game

    public EncounterType encounterType;

    [Header("Non Combat Prefab")]
    public GameObject prefab;

    [Header("Combat Setup")]
    public Enemy[] enemies;

    [Header("Rewards")]
    public int grantedCoins;
    public int grantedExperience;
}
