using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrapEncounter : GenericNonCombatEncounter
{
    public TextAnimator text;
    public int trapDamage = 5;
    public int dexLevelForSuccess = 3;

    PlayerCombat playerCombat;

    void Awake()
    {
        text.SetText("It's a trap!");
        playerCombat = Player.Instance.GetPlayerCombat();
    }

    public void PassTrap(int roll, int level)
    {
        text.SetText($"You skillfully dodge the trap (needed:{dexLevelForSuccess}, rolled {roll} + {level})");
        StartCoroutine(FinishEncounter());
    }

    public void FailTrap(int roll, int level)
    {
        text.SetText($"You fail to dodge the trap, and take {trapDamage} damage.");
        playerCombat.SubtractHealth(trapDamage);
        StartCoroutine(FinishEncounter());
    }

    public void AttemptTrap()
    {
        int roll = Random.Range(1, 7);
        int playerLevel = playerCombat.GetDexterity();


        if (roll + playerLevel > dexLevelForSuccess)
        {
            PassTrap(roll, playerLevel);
        }
        else
        {
            FailTrap(roll, playerLevel);
        }

    }
}
