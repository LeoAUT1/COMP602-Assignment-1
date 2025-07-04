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


    [SerializeField] private GameObject newAbilityContainer;
    [SerializeField] private TextMeshProUGUI unlockedAbiliyName;
    [SerializeField] private TextMeshProUGUI unlockedAbiliyDesc;

    [SerializeField] private int skillLevelsPerLevel = 10; //Amount the player can allocate in str dex or int per level

    [SerializeField] private AudioClip levelUpSfx;

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

        if (newAbility != null)
        {
            SetAbilityText(newAbility);
        } else
        {
            newAbilityContainer.SetActive(false);
        }
    }

    public void SetLevelUpMessage(int currentLevel)
    {
        titleText.text = $"You reached level {currentLevel}";
    }

    public void SetAbilityText(AbilityBase newAbility)
    {
        unlockedAbiliyName.text = newAbility.AbilityName;
        unlockedAbiliyDesc.text = newAbility.Description;
    }

    public void PlayerChoosesStr()
    {
        Debug.Log("Player chooses str");
        Player.Instance.GetPlayerCombat().AddStrength(skillLevelsPerLevel);
        AudioManager.Instance.PlaySoundEffect(levelUpSfx);
        Destroy(gameObject);
    }

    public void PlayerChoosesDex()
    {
        Player.Instance.GetPlayerCombat().AddDexterity(skillLevelsPerLevel);
        AudioManager.Instance.PlaySoundEffect(levelUpSfx);
        Destroy(gameObject);

    }

    public void PlayerChoosesInt()
    {
        Player.Instance.GetPlayerCombat().AddIntelligence(skillLevelsPerLevel);
        AudioManager.Instance.PlaySoundEffect(levelUpSfx);
        Destroy(gameObject);
    }
}
