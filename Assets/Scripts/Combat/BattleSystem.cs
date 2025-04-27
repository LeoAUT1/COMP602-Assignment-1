using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public enum BattleState { START, PLAYERTURN, ENEMYTURN, WIN, LOSE }


public class BattleSystem : MonoBehaviour

{


    public GameObject playerPreFab;
    public GameObject enemyPreFab;

    Unit playerUnit;
    Unit enemyUnit;

    public Text combatTxt;

    public BattleHud playerHud;
    public BattleHud EnemyHud;

    public BattleState state;
    // Start is called before the first frame update
    void Start()
    {
        state = BattleState.START;
        SetupBattle();
        StartCoroutine(SetupBattle());

    }

    IEnumerator SetupBattle()
    {
        GameObject playerObj = Instantiate(playerPreFab);
        playerUnit = playerObj.GetComponent<Unit>();

        GameObject enemyObj = Instantiate(enemyPreFab);
        enemyUnit = enemyObj.GetComponent<Unit>();

        combatTxt.text = "A wild " + enemyUnit.unitName +" has appeared.";

        playerHud.setHUD(playerUnit);

        EnemyHud.setHUD(enemyUnit);
        
        yield return new WaitForSeconds(2f);


        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    //player attack after pressing button
    IEnumerator PlayerAttack()
    {
        bool isDead = enemyUnit.TakeDamage(playerUnit.damage);

        EnemyHud.setHP(enemyUnit.currentHp);
        combatTxt.text = "Attack succesful!";

        yield return new WaitForSeconds(2f);

        //check if dead
        if (isDead)
        {
            //end combat
            state = BattleState.WIN;
            EndBattle();
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
        playerUnit.heal(5);
        playerHud.setHP(playerUnit.currentHp);
        playerHud.healthText.text = "HP: " + playerUnit.currentHp + "/" + playerUnit.maxHp;

        combatTxt.text = "you feel rejuvenated!";

        yield return new WaitForSeconds(2f);

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());


    }

    //enemy turn
    IEnumerator EnemyTurn()
    {
        combatTxt.text = enemyUnit.unitName + " attacks!";

        yield return new WaitForSeconds(1f);

        bool isDead = playerUnit.TakeDamage(enemyUnit.damage);

        playerHud.setHP(playerUnit.currentHp);

        playerHud.healthText.text = "HP: " + playerUnit.currentHp + "/" + playerUnit.maxHp;

        yield return new WaitForSeconds(1f);

        if (isDead)
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
        if(state == BattleState.WIN)
        {
            combatTxt.text = enemyUnit.unitName + " has been slain!";
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
            return;

        StartCoroutine(PlayerAttack());

    }

    public void OnHealButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerHeal());

    }


}
