using System.Collections;
using TMPro;
using UnityEngine;

public class SkillCheck : GenericNonCombatEncounter
{
    public TextAnimator text;
    [SerializeField] private int trapDamage = 5;
    [SerializeField] private GameObject dicePrefab;

    private PlayerCombat playerCombat;
    private Dice activeDice;

    private void Awake()
    {
        text.SetText("Let's test your skill!");
        playerCombat = Player.Instance.GetPlayerCombat();
    }


    public void AttemptSkill()
    {
        text.SetText("Rolling the dice...");

        StartCoroutine(Board.Instance.RollTheDiceCoroutine((diceFace) => {
            if (diceFace >= 4)
                SkillWon();
            else
                SkillFail();
        }, movePlayer: false));
    }


    private void SkillWon()
    {
        int coinReward = 10;
        Player.Instance.AddCoins(coinReward);

        text.SetText($"Congratulations! You earned {coinReward} coins!");
        StartCoroutine(FinishEncounter());
    }


    private void SkillFail()
    {
        text.SetText($"Oh well. Better luck next time. You lost {trapDamage} HP.");
        playerCombat.SubtractHealth(trapDamage);
        StartCoroutine(FinishEncounter());
    }
}
