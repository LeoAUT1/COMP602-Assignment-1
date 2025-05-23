using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public enum BattleState { START, PLAYERTURN, ENEMYTURN, WIN, LOSE }

public class CombatManager : MonoBehaviour
{
    private Enemy[] enemies;
    private EncounterData encounter;

    PlayerCombat playerUnit;
    Enemy enemyUnit;

    // For queuing messages from status effects or other sources
    private Queue<string> messageQueue = new Queue<string>();
    private bool isDisplayingMessages = false;

    [SerializeField] public CombatHud combatHud;

    [SerializeField] private Transform instantiatePlayerHere; // When the scene loads put the player's model on this transform
    [SerializeField] private Transform instantiateEnemyHere; // When the scene loads put the enemy model on this transform

    [SerializeField] private GameObject playerPiecePrefab; //player's model

    public BattleState state;

    public void InitialiseCombat(EncounterData encounter)
    {
        // Do stuff here to set up whatever is necessary for the combat scene 

        this.encounter = encounter;

        //This shouldn't happen
        if (encounter == null || encounter.enemies == null || encounter.enemies.Length == 0)
        {
            Debug.LogError("EncounterData is invalid or has no enemies.");
            GameManager.Instance.ExitCombat(); // Example error handling
            return;
        }

        this.enemies = encounter.enemies;

        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    public void ExitCombat()
    {

        //Grant player the reward for completing the encounter
        Player.Instance.AddExperience(encounter.grantedExperience);

        //back to the Board Scene
        GameManager.Instance.ExitCombat();
    }

    IEnumerator SetupBattle()
    {
        enemyUnit = Instantiate(encounter.enemies[0], instantiateEnemyHere);
        Instantiate(playerPiecePrefab, instantiatePlayerHere.position, Quaternion.Euler(0,180,0));

        playerUnit = Player.Instance.GetComponent<PlayerCombat>();

        if (enemyUnit == null)
        {
            Debug.LogError("Enemy is missing");
        }

        if (playerUnit == null) {
            Debug.LogError("Player is missing");
        }

        // Give the player and enemy access to the manager and hud
        enemyUnit.Initialise(this, combatHud);
        playerUnit.Initialise(this, combatHud);

        // Set the start combat message and give the hud access to the player and enemy
        combatHud.SetPrimaryCombatMessage($"A {enemyUnit.GetName()} appeared.");
        combatHud.Initialise(enemyUnit, playerUnit);

        // Test status effect
        playerUnit.AddStatusEffect(new RegenPassive(1, 100));

        Coroutine queuedMessagesCoroutine = combatHud.ProcessMessageQueue();
        if (queuedMessagesCoroutine != null) yield return queuedMessagesCoroutine;

        //Wait a sec before starting player's turn
        yield return new WaitForSeconds(1f);

        state = BattleState.PLAYERTURN;
        StartCoroutine(PlayerTurnCoroutine());
    }

    //player attack after pressing button
    IEnumerator PlayerAttack()
    {
        int damage = playerUnit.GetStrength(); // Or ((PlayerCombat)playerUnit).GetStrength();

        // --- Example of OnDamageDealt hook for player ---
        // This is if the player has an effect that triggers when THEY deal damage
        List<StatusEffect> playerEffects = playerUnit.GetActiveStatusEffects();
        foreach(var effect in playerEffects) { effect.OnDamageDealt(enemyUnit, ref damage, combatHud); }
        // ---

        combatHud.QueueCombatMessage("Attack successful!"); // Queued message
        Coroutine preDamageMessages = combatHud.ProcessMessageQueue();
        if (preDamageMessages != null) yield return preDamageMessages;

        enemyUnit.TakeDamage(damage, playerUnit); // Pass attacker
        combatHud.UpdateEnemyHud(enemyUnit); // Update HUD after damage and effects

        // Process messages from OnDamageTaken effects (e.g., Thorns)
        Coroutine postDamageMessages = combatHud.ProcessMessageQueue();
        if (postDamageMessages != null) yield return postDamageMessages;

        if (!enemyUnit.IsAlive())
        {
            state = BattleState.WIN;
            StartCoroutine(EndBattle());
        }
        else
        {
            // --- Player Turn End Effects ---
            playerUnit.ProcessTurnEndEffects();
            combatHud.UpdatePlayerHud(playerUnit); // Update HUD if effects changed stats

            Coroutine turnEndMessages = combatHud.ProcessMessageQueue();
            if (turnEndMessages != null) yield return turnEndMessages;
            // ---

            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator PlayerHeal() // This could become a "Use Ability" coroutine
    {
        playerUnit.Heal(5); // Example heal amount
        combatHud.UpdatePlayerHud(playerUnit);
        combatHud.QueueCombatMessage("You feel rejuvenated.");

        Coroutine healMessages = combatHud.ProcessMessageQueue();
        if (healMessages != null) yield return healMessages;

        yield return new WaitForSeconds(0.3f);

        // --- Player Turn End Effects ---
        playerUnit.ProcessTurnEndEffects();
        combatHud.UpdatePlayerHud(playerUnit);

        Coroutine turnEndMessages = combatHud.ProcessMessageQueue();
        if (turnEndMessages != null) yield return turnEndMessages;
        // ---

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    //enemy turn
    IEnumerator EnemyTurn()
    {
        // --- Enemy Turn Start Effects ---
        enemyUnit.ProcessTurnStartEffects();
        combatHud.UpdateEnemyHud(enemyUnit); // Update HUD if effects changed stats

        // Turn start messages
        Coroutine turnStartMessages = combatHud.ProcessMessageQueue();
        if (turnStartMessages != null) yield return turnStartMessages;

        if (!enemyUnit.IsAlive()) // Check if died from start-of-turn effects
        {
            state = BattleState.WIN; // Or some other state if enemy dies on its own turn start
            StartCoroutine(EndBattle());
            yield break;
        }
        // ---

        combatHud.QueueCombatMessage($"{enemyUnit.GetName()} attacks!");
        Coroutine attackAnnounceMessages = combatHud.ProcessMessageQueue();
        if (attackAnnounceMessages != null) yield return attackAnnounceMessages;

        int damage = ((Enemy)enemyUnit).GetStrength();

        // --- Example of OnDamageDealt hook for enemy ---
        // List<StatusEffect> enemyEffects = enemyUnit.GetActiveStatusEffects();
        // foreach(var effect in enemyEffects) { effect.OnDamageDealt(playerUnit, ref damage); }
        // ---

        playerUnit.TakeDamage(damage, enemyUnit);
        combatHud.UpdatePlayerHud(playerUnit);

        // Process messages from player's OnDamageTaken effects
        Coroutine playerDamagedMessages = combatHud.ProcessMessageQueue();
        if (playerDamagedMessages != null) yield return playerDamagedMessages;

        yield return new WaitForSeconds(0.3f);

        if (!playerUnit.IsAlive())
        {
            state = BattleState.LOSE;
            StartCoroutine(EndBattle());
        }
        else
        {
            // --- Enemy Turn End Effects ---
            enemyUnit.ProcessTurnEndEffects();
            combatHud.UpdateEnemyHud(enemyUnit);
            Coroutine enemyTurnEndMessages = combatHud.ProcessMessageQueue();
            if (enemyTurnEndMessages != null) yield return enemyTurnEndMessages;
            // ---

            state = BattleState.PLAYERTURN;
            StartCoroutine(PlayerTurnCoroutine());
        }
    }

    //ends battle
    IEnumerator EndBattle()
    {
        // Ensure any lingering messages from the final action are displayed
        Coroutine finalActionMessages = combatHud.ProcessMessageQueue();
        if (finalActionMessages != null) yield return finalActionMessages;

        if (state == BattleState.WIN)
        {
            combatHud.SetPrimaryCombatMessage($"{enemyUnit.GetName()} was slain!");
        }
        else if (state == BattleState.LOSE) // Use else if for clarity
        {
            combatHud.SetPrimaryCombatMessage($"You were defeated.");
        }

        yield return new WaitForSeconds(2f); // Longer pause for end battle message

        ExitCombat();
    }

    //player turn action
    IEnumerator PlayerTurnCoroutine() // Renamed for clarity, or just change PlayerTurn
    {
        // --- Player Turn Start Effects ---
        playerUnit.ProcessTurnStartEffects(); // This calls OnTurnStart for all player effects
        combatHud.UpdatePlayerHud(playerUnit); // Update HUD if effects changed stats (e.g., health from regen)

        // Process any messages queued by OnTurnStart effects
        Coroutine turnStartMessages = combatHud.ProcessMessageQueue();
        if (turnStartMessages != null) yield return turnStartMessages;

        // Important: Check if player died from start-of-turn effects (e.g., heavy poison)
        if (!playerUnit.IsAlive())
        {
            state = BattleState.LOSE;
            StartCoroutine(EndBattle());
            yield break; // End turn processing
        }
        // ---

        combatHud.SetPrimaryCombatMessage($"Select an action:");
        combatHud.ShowPlayerCombatActions(true);
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
