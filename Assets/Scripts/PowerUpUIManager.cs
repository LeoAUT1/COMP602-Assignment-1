using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PowerUpUIManager : MonoBehaviour
{
    [SerializeField] private GameObject powerUpIcon;
    [SerializeField] private Transform container; //holds all the icons
    [SerializeField] private GameObject powersUpButton; //"POWERUPS"

    public int iconOffset = 80;

    void Awake()
    {
        RedrawPowerups();

        //Quick hack to hide the button when in the combat menu
        if (SceneManager.GetActiveScene().name == "Combat")
        {
            powersUpButton.SetActive(false);
        }
    }

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
        powersUpButton.SetActive(false);
    }

    private void GeneratePowerupIcons(Dictionary<string, PowerupData> powerups)
    {
        // Values only
        int count = 0;
        foreach (PowerupData powerup in powerups.Values)
        {
            Debug.Log($"Adding {powerup} to UI");
            GameObject go = Instantiate(powerUpIcon, container);

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2((-count * iconOffset)-70, 40);

            PowerUpIcon pup = go.GetComponent<PowerUpIcon>();
            pup.SetAll(powerup);

            count++;
        }

        if (count > 0) {
            powersUpButton.SetActive(true);
        }
    }
}
