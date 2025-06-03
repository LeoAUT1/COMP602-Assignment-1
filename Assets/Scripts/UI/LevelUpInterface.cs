using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpInterface : MonoBehaviour
{

    [SerializeField] private Button increaseStrButton;
    [SerializeField] private Button increaseDexButton;
    [SerializeField] private Button increaseIntButton;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI unlockAbilityText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCanvas(int currentLevel, AbilityBase newAbility)
    {

        SetLevelUpMessage(currentLevel);

        // THe player has unlocked a new ability, we want to display some relevant information
        SetAbilityText(newAbility);
    }

    public void SetLevelUpMessage(int currentLevel)
    {
        titleText.text = $"You reached level {currentLevel}";
    }

    public void SetAbilityText(AbilityBase newAbility)
    {
        if (newAbility == null)
        {
            unlockAbilityText.text = "";
            return;
        }
        unlockAbilityText.text = $"You unlocked {newAbility.AbilityName}";
    }

    public void PlayerChoosesStr()
    {
        Player.Instance.GetPlayerCombat().AddStrength(1);
    }

    public void PlayerChoosesDex()
    {
        Player.Instance.GetPlayerCombat().AddDexterity(1);

    }

    public void PlayerChoosesInt()
    {
        Player.Instance.GetPlayerCombat().AddIntelligence(1);
    }
}
