using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpUIManager : MonoBehaviour
{
    [SerializeField] private GameObject powerUpIcon;
    [SerializeField] private Transform container; //holds all the icons

    public void RedrawPowerups(Dictionary<string, PowerupData> powerups)
    {
        ClearPowerupIcons();
        GeneratePowerupIcons(powerups);
    }

    private void ClearPowerupIcons()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void GeneratePowerupIcons(Dictionary<string, PowerupData> powerups)
    {

    }
}
