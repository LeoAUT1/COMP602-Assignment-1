using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupDebug : MonoBehaviour
{
    [SerializeField] private PowerupData vampirism;
    [SerializeField] private PowerupData regen;

    private PlayerCombat pcbt;

    private void Start()
    {
        pcbt = Player.Instance.GetPlayerCombat();
    }
    public void GrantVampirism()
    {
        Player.Instance.AddPowerup(vampirism);
    }
    public void GrantRegen()
    {
        Player.Instance.AddPowerup(regen);
    }
}
