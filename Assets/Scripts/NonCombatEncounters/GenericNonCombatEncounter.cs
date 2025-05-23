using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericNonCombatEncounter : MonoBehaviour
{
    protected Board board;
    public float delayBeforeDestroy = 1f; // Configurable delay in seconds

    public void SetBoard(Board b)
    {
        Debug.Log($"Setting board to {b}");
        board = b; ;
    }

    protected IEnumerator FinishEncounter()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delayBeforeDestroy);

        // Re-enable board buttons and destroy this component
        board.EnableBoardButtons();
        Destroy(gameObject);
    }
}
