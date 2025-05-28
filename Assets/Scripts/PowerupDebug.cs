using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupDebug : MonoBehaviour
{
    [SerializeField] private PowerupData vampirism;
    [SerializeField] private PowerupData regen;
    public void GrantVampirism()
    {
        Debug.Log("GIving the player vampirism");
        PowerupData copy = Instantiate(vampirism);
        Player.Instance.AddPowerup(copy);
    }
    public void GrantRegen()
    {
        PowerupData copy = Instantiate(regen);
        Player.Instance.AddPowerup(copy);
    }
}
