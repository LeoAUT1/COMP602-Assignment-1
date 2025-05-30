using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerupsOverlay : MonoBehaviour
{

    [SerializeField] private GameObject powerUpIconPrefab;
    [SerializeField] private GameObject toggleableContainer; //Turn this on and off to show the menu
    [SerializeField] private Transform iconContainer; // Icons go in here

    [SerializeField] private int iconOffset; // vertical offset for icons

    [SerializeField] private Button buttonShow;
    [SerializeField] private Button buttonHide;

    private void Awake()
    {
        if (buttonShow != null)
        {
            buttonShow.onClick.AddListener(ShowContainer);
        }

        if (buttonHide != null)
        {
            buttonHide.onClick.AddListener(HideContainer);
        }
    }

    private void ShowContainer()
    {
        Debug.Log("Showing container");
        toggleableContainer.SetActive(true);
        ClearIcons();
        GeneratePowerupIcons(Player.Instance.GetPlayerCombat().GetPowerUps());
    }

    private void HideContainer()
    {
        toggleableContainer.SetActive(false);
    }

    // Delete all the old icons and redraw
    private void ClearIcons()
    {
        foreach (Transform child in iconContainer)
        {
            Debug.Log(child);
            Destroy(child.gameObject);
        }
    }

    private void GeneratePowerupIcons(Dictionary<string, PowerupData> powerups)
    {
        // Values only
        int count = 0;
        foreach (PowerupData powerup in powerups.Values)
        {
            Debug.Log($"Adding {powerup} to UI");
            GameObject go = Instantiate(powerUpIconPrefab, iconContainer);

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2( -160, (-count * iconOffset) + 110);

            PowerUpIcon pup = go.GetComponent<PowerUpIcon>();
            pup.SetSprite(powerup.icon);
            pup.SetCount($"x{powerup.count}");
            pup.SetTitle(powerup.powerupName);
            pup.SetDesc(powerup.description);

            count++;
        }
    }
}
