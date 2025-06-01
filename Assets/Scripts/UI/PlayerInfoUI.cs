using TMPro; // Don't forget this if you haven't already added it.
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI strText;
    [SerializeField] private TextMeshProUGUI dexText;
    [SerializeField] private TextMeshProUGUI intText;

    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider expSlider;

    public void UpdateAll()
    {
        string expTextToSet = $"Level in {Player.Instance.GetExpToLevel()} exp";

        if (Player.Instance == null)
        {
            Debug.LogError("Player.Instance is null. Cannot update UI.");
            return;
        }

        PlayerCombat pcbt = Player.Instance.GetPlayerCombat();

        // HP
        hpText.text = $"HP: {pcbt.GetHealth()} / {pcbt.GetMaxHealth()}";
        hpSlider.value = (float) pcbt.GetHealth() / (float) pcbt.GetMaxHealth();

        // Stats
        dexText.text = $"{pcbt.GetDexterity()}";
        strText.text = $"{pcbt.GetStrength()}";
        intText.text = $"{pcbt.GetIntelligence()}";

        Debug.Log($"player exp {Player.Instance.GetExperience()}");
        Debug.Log($"player exp to level {Player.Instance.GetExpToLevel()}");

        // Level and Exp
        expText.text = expTextToSet;
        expSlider.value = Player.Instance.GetNormalizedExperienceProgress();
        levelText.text = $"{Player.Instance.GetLevel()}";
    }
}
