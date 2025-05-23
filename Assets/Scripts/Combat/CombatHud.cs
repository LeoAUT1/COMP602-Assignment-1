using System.Collections;
using System.Collections.Generic; // Required for Queue<T>
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Assuming TextAnimator has a SetText(string) method
// using YourNamespace; // If TextAnimator is in a specific namespace

public class CombatHud : MonoBehaviour
{
    [SerializeField] private CombatEntityHud playerHud;
    [SerializeField] private CombatEntityHud enemyHud;
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private TextAnimator combatMessage; // Make private, assign in Inspector

    private PlayerCombat playerUnit; // Store references if needed for internal updates
    private Enemy enemyUnit;       // though current updates pass entities

    private Queue<string> messageQueue = new Queue<string>();
    private bool isProcessingQueue = false;
    private Coroutine messageProcessingCoroutine;
    [SerializeField] private float messageDisplayTime = 0.75f; // Configurable display time per message

    public void Initialise(Enemy newEnemyUnit, PlayerCombat newPlayerUnit)
    {
        this.enemyUnit = newEnemyUnit;
        this.playerUnit = newPlayerUnit;

        if (playerHud == null || enemyHud == null || combatMessage == null || buttonContainer == null)
        {
            Debug.LogError("CombatHud is missing one or more required component references!");
            return;
        }

        playerHud.SetHUD(newPlayerUnit);
        enemyHud.SetHUD(newEnemyUnit);
    }

    public void ShowPlayerCombatActions(bool show)
    {
        if (buttonContainer != null)
        {
            buttonContainer.SetActive(show);
        }
    }

    // Specific update methods are preferred
    public void UpdatePlayerHud(PlayerCombat player)
    {
        if (playerHud != null && player != null)
        {
            playerHud.SetHP(player.GetHealth(), player.GetMaxHealth());
        }
    }

    public void UpdateEnemyHud(Enemy enemy)
    {
        if (enemyHud != null && enemy != null)
        {
            enemyHud.SetHP(enemy.GetHealth(), enemy.GetMaxHealth());
        }
    }

    
    /// Sets a primary combat message, clearing any queued messages and stopping current message processing.
    /// Use this for important messages like turn indicators or battle results.
    
    public void SetPrimaryCombatMessage(string message)
    {
        if (combatMessage == null) return;

        if (messageProcessingCoroutine != null)
        {
            StopCoroutine(messageProcessingCoroutine);
            messageProcessingCoroutine = null;
        }
        messageQueue.Clear();
        isProcessingQueue = false;
        combatMessage.SetText(message);
    }

    
    /// Queues a combat message to be displayed sequentially.
    
    public void QueueCombatMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        messageQueue.Enqueue(message);
    }

    
    /// Starts processing and displaying any queued messages.
    /// Returns the coroutine handling message display, so the caller can yield on it.
    
    public Coroutine ProcessMessageQueue()
    {
        if (combatMessage == null) return null;

        // If already processing and there are messages, or no messages to process, return current/null.
        if (isProcessingQueue && messageProcessingCoroutine != null) return messageProcessingCoroutine;
        if (messageQueue.Count == 0)
        {
            isProcessingQueue = false; // Ensure state is correct if queue was emptied externally
            return null;
        }

        messageProcessingCoroutine = StartCoroutine(ProcessMessageQueueCoroutine());
        return messageProcessingCoroutine;
    }

    private IEnumerator ProcessMessageQueueCoroutine()
    {
        isProcessingQueue = true;
        while (messageQueue.Count > 0)
        {
            combatMessage.SetText(messageQueue.Dequeue());
            yield return new WaitForSeconds(messageDisplayTime);
        }
        isProcessingQueue = false;
        messageProcessingCoroutine = null;
    }
}
