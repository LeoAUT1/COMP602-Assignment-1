using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class PowerUpUIManager : MonoBehaviour
{
    [SerializeField] private GameObject powerUpIcon;
    [SerializeField] private Transform container; //holds all the icons

    public void RedrawPowerups()
    {
        Player.Instance.GetPlayerCombat().GetPowerUps();
        ClearPowerupIcons();
        GeneratePowerupIcons(Player.Instance.GetPlayerCombat().GetPowerUps());
    }

    private void ClearPowerupIcons()
    {
        Debug.Log(container);
        foreach (Transform child in container)
        {
            Debug.Log(child);
            Destroy(child.gameObject);
        }
    }

    private void GeneratePowerupIcons(Dictionary<string, PowerupData> powerups)
    {
        // Values only
        foreach (PowerupData powerup in powerups.Values)
        {
            Debug.Log($"Adding {powerup} to UI");
            GameObject go = Instantiate(powerUpIcon, container);

            PowerUpIcon pup = go.GetComponent<PowerUpIcon>();
            pup.SetSprite(powerup.icon);
            pup.SetText($"x{powerup.count}");
        }
    }
}
