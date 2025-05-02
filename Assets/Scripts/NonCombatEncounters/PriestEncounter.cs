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
        text.SetText("You feel reinvigorated");
        PlayerCombat p = Player.Instance.GetPlayerCombat();
        p.SetHealth(p.GetMaxHealth());
        StartCoroutine(FinishEncounter());
    }

    public void RejectBlessing()
    {
        text.SetText("You decline the priest's offer");
        StartCoroutine(FinishEncounter());
    }
}
