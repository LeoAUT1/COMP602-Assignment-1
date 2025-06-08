using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericNonCombatEncounter : MonoBehaviour
{
    protected Board board;
    public float delayBeforeDestroy = 1f; // Configurable delay in seconds
    private int experienceGranted = 0;

    public void SetBoard(Board b)
    {
        Debug.Log($"Setting board to {b}");
        board = b; ;
    }

    protected IEnumerator FinishEncounter(bool success=true)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delayBeforeDestroy);

        if (success) {

            Debug.Log($"Granting the player {experienceGranted}xp");

            Player.Instance.GrantRandomPowerup();
            bool hasLeveledUp = Player.Instance.AddExperience(experienceGranted);
        }

        PlayerCombat playerCombat = Player.Instance.GetPlayerCombat();

        if (playerCombat.IsAlive() == false)
        {
            SceneLoader.Instance.LoadGameEnd();
        }

        // Re-enable board buttons and destroy this component
        board.EnableBoardButtons();
        board.UpdatePlayerStatsUi();
        Destroy(gameObject);
    }

    public void SetExperienceGranted(int exp)
    {
        experienceGranted = exp;
    }
}
