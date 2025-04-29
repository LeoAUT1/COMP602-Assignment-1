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

    public Text combatTxt;

    public CombatHud playerHud;
    public CombatHud EnemyHud;
    public GameObject buttonContainer; //Hide these when its not the player's turn

    public BattleState state;

    public void InitialiseCombat(Player player, EncounterData encounter)
    {
        // Do stuff here to set up whatever is necessary for the combat scene 

        this.encounter = encounter;
        this.player = player;
        this.enemies = encounter.enemies;

        state = BattleState.START;
        SetupBattle();
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
        Debug.Log("test");
        Debug.Log(encounter.enemies[0]);

        enemyUnit = Instantiate(encounter.enemies[0]);

        playerUnit = player.GetComponent<PlayerCombat>();

        //GameObject playerObj = Instantiate(playerPreFab);
        //playerUnit = playerObj.GetComponent<CombatEntity>();

        //GameObject enemyObj = Instantiate(enemyPreFab);
        //enemyUnit = enemyObj.GetComponent<CombatEntity>();

        combatTxt.text = "A wild " + enemyUnit.GetName() + " has appeared.";

        playerHud.setHUD(playerUnit);

        EnemyHud.setHUD(enemyUnit);

        yield return new WaitForSeconds(2f);


        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    //player attack after pressing button
    IEnumerator PlayerAttack()
    {
        int enemyRemainingHealth = enemyUnit.SubtractHealth(playerUnit.GetStrength());

        Debug.Log($"Enemy now has {enemyRemainingHealth} HP");

        EnemyHud.setHP(enemyUnit.GetHealth());
        combatTxt.text = "Attack succesful!";

        yield return new WaitForSeconds(2f);

        //check if dead
        if (!enemyUnit.GetIsAlive())
        {
            //end combat
            state = BattleState.WIN;
            EndBattle();
        }
        else
        {
            Debug.Log("player still alive");
            //enemy turn
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }
    IEnumerator PlayerHeal()
    {
        playerUnit.AddHealth(5);
        playerHud.setHP(playerUnit.GetHealth());
        playerHud.healthText.text = "HP: " + playerUnit.GetHealth() + "/" + playerUnit.GetMaxHealth();

        combatTxt.text = "you feel rejuvenated!";

        yield return new WaitForSeconds(2f);

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    //enemy turn
    IEnumerator EnemyTurn()
    {
        combatTxt.text = enemyUnit.GetName() + " attacks!";

        yield return new WaitForSeconds(1f);

        playerUnit.SubtractHealth(enemyUnit.GetStrength());

        playerHud.setHP(playerUnit.GetHealth());
        
        playerHud.healthText.text = "HP: " + playerUnit.GetHealth() + "/" + playerUnit.GetMaxHealth();

        yield return new WaitForSeconds(1f);

        if (!playerUnit.GetIsAlive())
        {
            state = BattleState.LOSE;
            EndBattle();
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }

    }

    //ends battle
    void EndBattle()
    {
        if (state == BattleState.WIN)
        {
            combatTxt.text = enemyUnit.GetName() + " has been slain!";
            GameManager.Instance.ExitCombat();
        }
        else if (state == BattleState.LOSE)
        {
            combatTxt.text = "you were defeated!";
        }
    }

    //player turn action
    void PlayerTurn()
    {
        combatTxt.text = "Select an action:";
    }

    //attack button
    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            Debug.Log("It is not the player's turn");
            return;
        }

        StartCoroutine(PlayerAttack());

    }

    public void OnHealButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerHeal());

    }
}
