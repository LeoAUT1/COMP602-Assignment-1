using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PriestEncounter : GenericNonCombatEncounter
{
    public TextAnimator text;

    void Start()
    {
        text.SetText("A friendly priest offers to bless you");
    }

    public void AcceptBlessing()
    {
        text.SetText("The priest restores you to full health");
        PlayerCombat p = Player.Instance.GetPlayerCombat();
        p.SetHealth(p.GetMaxHealth());
        StartCoroutine(FinishEncounter());
    }

    public void RejectBlessing()
    {
        text.SetText("You tough it out, increasing your maximum health.");
        PlayerCombat p = Player.Instance.GetPlayerCombat();

        //Increase the player's max health by 10
        p.SetMaxHealth(p.GetMaxHealth()+10);
        StartCoroutine(FinishEncounter());
    }
}
