using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public enum BattleState { START, PLAYERTURN, ENEMYTURN, WIN, LOSE }

public class CombatManager : MonoBehaviour
{

    private Player player;
    private Enemy[] enemies;
    private EncounterData encounter;

    PlayerCombat playerUnit;
    Enemy enemyUnit;

    [SerializeField] CombatHud combatHud;

    public BattleState state;

    public void InitialiseCombat(Player player, EncounterData encounter)
    {
        // Do stuff here to set up whatever is necessary for the combat scene 

        this.encounter = encounter;
        this.player = player;
        this.enemies = encounter.enemies;

        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    public void ExitCombat()
    {

        //Grant player the reward for completing the encounter
        player.AddExperience(encounter.grantedExperience);
        player.AddCoins(encounter.grantedCoins);

        //back to the Board Scene
        GameManager.Instance.ExitCombat();
    }
    IEnumerator SetupBattle()
    {
        enemyUnit = Instantiate(encounter.enemies[0]);

        playerUnit = player.GetComponent<PlayerCombat>();


        combatHud.combatMessage.SetText($"A { enemyUnit.GetName()} appeared.");

        if (enemyUnit == null)
        {
            Debug.LogError("Enemy is missing");
        }

        if (playerUnit == null) {
            Debug.LogError("Player is missing");
        }

        combatHud.Initialise(enemyUnit, playerUnit);

        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    //player attack after pressing button
    IEnumerator PlayerAttack()
    {
        enemyUnit.SubtractHealth(playerUnit.GetStrength());
        combatHud.combatMessage.SetText("Attack succesful!");
        combatHud.UpdateEnemyHud(enemyUnit);

        yield return new WaitForSeconds(2f);

        //check if dead
        if (!enemyUnit.GetIsAlive())
        {
            //end combat
            state = BattleState.WIN;
            StartCoroutine(EndBattle());
        }
        else
        {
            //enemy turn
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }
    IEnumerator PlayerHeal()
    {
        playerUnit.AddHealth(5);
        combatHud.combatMessage.SetText("You feel rejuvenated");

        yield return new WaitForSeconds(0.3f);

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    //enemy turn
    IEnumerator EnemyTurn()
    {
        combatHud.combatMessage.SetText($"{enemyUnit.GetName()} attacks!");

        yield return new WaitForSeconds(0.3f);

        playerUnit.SubtractHealth(enemyUnit.GetStrength());
        combatHud.UpdatePlayerHud(playerUnit);

        yield return new WaitForSeconds(0.3f);

        if (!playerUnit.GetIsAlive())
        {
            state = BattleState.LOSE;
            StartCoroutine(EndBattle());
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }

    }

    //ends battle
    IEnumerator EndBattle()
    {
        if (state == BattleState.WIN)
        {
            combatHud.combatMessage.SetText($"{enemyUnit.GetName()} was slain!");
        }
        if (state == BattleState.LOSE)
        {
            combatHud.combatMessage.SetText($"You were defeated");
        }

        yield return new WaitForSeconds(1f);

        GameManager.Instance.ExitCombat();

    }

    //player turn action
    void PlayerTurn()
    {
        combatHud.ShowPlayerCombatActions(true);
        combatHud.combatMessage.SetText($"Select an action:");
    }

    //attack button
    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            Debug.Log("It is not the player's turn");
            return;
        }

        combatHud.ShowPlayerCombatActions(false);
        StartCoroutine(PlayerAttack());

    }

    public void OnHealButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerHeal());

    }
}
