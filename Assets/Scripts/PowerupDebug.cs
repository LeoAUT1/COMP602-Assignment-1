using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PowerupDebug : MonoBehaviour
{
    [SerializeField] private List<PowerupData> powerups = new List<PowerupData>();
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Button buttonPrefab; // Reference to a button prefab

    private void Start()
    {
        GeneratePowerupButtons();
    }

    private void GeneratePowerupButtons()
    {
        // Clear existing buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Create a button for each powerup
        for (int i = 0; i < powerups.Count; i++)
        {
            int index = i; // Capture the index for the lambda
            Button newButton = Instantiate(buttonPrefab, buttonContainer);

            RectTransform rectTransform = newButton.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -i * 50+ 100);

            // For TextMeshPro support
            TMPro.TextMeshProUGUI tmpText = newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = "DEBUG " + powerups[i].powerupName;
            }

            // Add click listener
            newButton.onClick.AddListener(() => GrantPowerup(index));
        }
    }

    // Call this if you modify the powerups list at runtime
    public void RefreshButtons()
    {
        GeneratePowerupButtons();
    }

    public void GrantPowerup(int index)
    {
        if (index < 0 || index >= powerups.Count)
        {
            Debug.LogError($"Invalid powerup index: {index}. Valid range is 0-{powerups.Count - 1}");
            return;
        }

        Debug.Log($"Giving the player {powerups[index].powerupName}");
        PowerupData copy = Instantiate(powerups[index]);
        Player.Instance.AddPowerup(copy);
    }

    public void GrantPowerupByName(string powerupName)
    {
        PowerupData powerup = powerups.Find(p => p.powerupName == powerupName);
        if (powerup != null)
        {
            Debug.Log($"Giving the player {powerup.powerupName}");
            PowerupData copy = Instantiate(powerup);
            Player.Instance.AddPowerup(copy);
        }
        else
        {
            Debug.LogError($"Powerup not found: {powerupName}");
        }
    }

    public string[] GetPowerupNames()
    {
        return powerups.Select(p => p.powerupName).ToArray();
    }
}
