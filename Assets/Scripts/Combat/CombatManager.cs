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
        //back to the Board Scene
        GameManager.Instance.ExitCombat();
    }

    IEnumerator SetupBattle()
    {
        enemyUnit = Instantiate(encounter.enemies[0], instantiateEnemyHere);
	enemyUnit.AutoScaleToPlayer();
        GameObject playerPiece = Instantiate(playerPiecePrefab, instantiatePlayerHere.position, instantiatePlayerHere.rotation);

        Player.Instance.SetPlayerPiece(playerPiece);

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

        Coroutine queuedMessagesCoroutine = combatHud.ProcessMessageQueue();
        if (queuedMessagesCoroutine != null) yield return queuedMessagesCoroutine;

        //Wait a sec before starting player's turn
        yield return new WaitForSeconds(1f);

        state = BattleState.PLAYERTURN;
        StartCoroutine(PlayerTurnCoroutine());
    }

    //player attack after pressing button
    // In CombatManager.cs

    // This will be called by UI buttons, passing the selected ability.
    // It then may need to handle target selection before executing.
    public void OnPlayerAbilitySelected(AbilityBase ability)
    {
        if (state != BattleState.PLAYERTURN || playerUnit == null) return;

        // --- TARGET SELECTION LOGIC ---
        // This is a crucial part. For now, a simplified version:
        CombatEntity primaryTarget = null;
        List<CombatEntity> allSelectedTargets = new List<CombatEntity>();

        switch (ability.TargetType)
        {
            case TargetType.Self:
                primaryTarget = playerUnit;
                allSelectedTargets.Add(playerUnit);
                break;
            case TargetType.Enemy:
                // In a real game, you'd enable a targeting mode here.
                // Player clicks an enemy, that enemy becomes 'primaryTarget'.
                // For now, assume the current 'enemyUnit' is the only/intended target.
                if (enemyUnit != null && enemyUnit.IsAlive()) // Ensure enemyUnit is valid
                {
                    primaryTarget = enemyUnit;
                    allSelectedTargets.Add(enemyUnit);
                }
                else
                {
                    combatHud.QueueCombatMessage("No valid enemy target available!");
                    combatHud.ProcessMessageQueue(); // Show message immediately
                    return; // Don't proceed
                }
                break;
            default:
                Debug.LogError($"Target type {ability.TargetType} not fully handled for selection.");
                combatHud.QueueCombatMessage("Cannot determine target for this ability.");
                combatHud.ProcessMessageQueue();
                return;
        }

        if (ability.CanUse(playerUnit, primaryTarget, allSelectedTargets))
        {
            combatHud.ShowPlayerCombatActions(false); // Hide ability panel
            StartCoroutine(ExecutePlayerAbilityCoroutine(ability, primaryTarget, allSelectedTargets));
        }
        else
        {
            combatHud.QueueCombatMessage($"Cannot use {ability.AbilityName} now.");
            // Consider showing a more specific reason from CanUse if you implement that.
            combatHud.ProcessMessageQueue();
            // Do not hide actions, let player choose again.
        }
    }

    IEnumerator ExecutePlayerAbilityCoroutine(AbilityBase ability, CombatEntity primaryTarget, List<CombatEntity> allSelectedTargets)
    {
        // Call the ability's own execution logic
        yield return StartCoroutine(ability.Execute(playerUnit, primaryTarget, this, combatHud));

        // --- Post-Ability Common Logic ---
        // Update HUDs (the ability itself might have done some, but a general refresh can be good)
        combatHud.UpdatePlayerHud(playerUnit);
        if (enemyUnit != null) combatHud.UpdateEnemyHud(enemyUnit); // Update main enemy HUD
                                                                    // If 'allSelectedTargets' could contain other entities, update their HUDs too.

        // Check for enemy death (simplified for one enemy)
        if (enemyUnit != null && !enemyUnit.IsAlive())
        {
            state = BattleState.WIN;
            StartCoroutine(EndBattle());
            yield break; // Stop further processing this turn
        }
        // If multiple enemies, you'd check if ALL are dead.

        // Player Turn End Effects
        playerUnit.ProcessTurnEndEffects();
        combatHud.UpdatePlayerHud(playerUnit); // Update after effects

        Coroutine turnEndMessages = combatHud.ProcessMessageQueue();
        if (turnEndMessages != null) yield return turnEndMessages;

        if (!playerUnit.IsAlive()) // Check if player died from turn-end effects
        {
            state = BattleState.LOSE;
            StartCoroutine(EndBattle());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
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
        if (playerUnit.DodgeAttempt())
        {
            combatHud.QueueCombatMessage("you successfully dodged the attack!");
        }
        else
        {
            playerUnit.TakeDamage(damage, enemyUnit);
            combatHud.UpdatePlayerHud(playerUnit);
        }
            

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
    IEnumerator PlayerTurnCoroutine()
    {
        playerUnit.ProcessTurnStartEffects();
        combatHud.UpdatePlayerHud(playerUnit);

        Coroutine turnStartMessages = combatHud.ProcessMessageQueue();
        if (turnStartMessages != null) yield return turnStartMessages;

        if (!playerUnit.IsAlive())
        {
            state = BattleState.LOSE;
            StartCoroutine(EndBattle());
            yield break;
        }

        // Clear the text so there is room for buttons
        combatHud.SetPrimaryCombatMessage($"");
        // Pass the player's abilities to the HUD to generate buttons
        combatHud.DisplayPlayerAbilities(playerUnit.Abilities, OnPlayerAbilitySelected);
        combatHud.ShowPlayerCombatActions(true); // This now shows the panel with dynamic buttons
    }
}
