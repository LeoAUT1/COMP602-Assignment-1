using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrapEncounter : GenericNonCombatEncounter
{
    public TextAnimator text;
    private int trapDamage = 5;

    PlayerCombat playerCombat;

    void Awake()
    {
        text.SetText("It's a trap!");
        playerCombat = Player.Instance.GetPlayerCombat();
    }

    public void PassTrap()
    {
        text.SetText("You skillfully dodge the trap");
        StartCoroutine(FinishEncounter());
    }

    public void FailTrap()
    {
        text.SetText($"You fail to dodge the trap, and take {trapDamage} damage.");
        playerCombat.SubtractHealth(trapDamage);
        StartCoroutine(FinishEncounter());
    }

    public void AttemptTrap()
    {
        int roll = Random.Range(1, 7);
        
        if (roll + playerCombat.GetDexterity() > 3)
        {
            PassTrap();
        }
        else
        {
            FailTrap();
        }

    }
}
