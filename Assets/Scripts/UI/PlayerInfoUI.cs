using TMPro; // Don't forget this if you haven't already added it.
using UnityEngine;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI strText;
    [SerializeField] private TextMeshProUGUI dexText;
    [SerializeField] private TextMeshProUGUI intText;

    public void UpdateAll()
    {
        if (Player.Instance == null)
        {
            Debug.LogError("Player.Instance is null. Cannot update UI.");
            return;
        }

        PlayerCombat pcbt = Player.Instance.GetPlayerCombat();

        if (pcbt == null)
        {
            Debug.LogError("PlayerCombat component is null. Cannot update combat stats.");
            hpText.text = "N/A";
            strText.text = "N/A";
            dexText.text = "N/A";
            intText.text = "N/A";

            expText.text = $"{Player.Instance.GetExperience()}";
            levelText.text = $"{Player.Instance.GetLevel()}";
            return;
        }

        hpText.text = $"HP: {pcbt.GetHealth()} / {pcbt.GetMaxHealth()}";

        dexText.text = $"{pcbt.GetDexterity()}";
        strText.text = $"{pcbt.GetStrength()}";
        intText.text = $"{pcbt.GetIntelligence()}";

        expText.text = $"{Player.Instance.GetExperience()}";
        levelText.text = $"{Player.Instance.GetLevel()}";
    }
}
