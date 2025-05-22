using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceManager : Singleton<DiceManager>
{
    [SerializeField] private GameObject die;
    [SerializeField] private float delayAfterRolling = 2f; // How long to wait until the die function returns the number, default 2seconds

    // In DiceManager
    public IEnumerator RollTheDiceCoroutine(System.Action<int> onComplete, Vector3 spawnPosition) // Or use a pre-configured spawn
    {
        int result = 0;
        bool rollComplete = false;

        // Simplified call to the existing RollTheDice, passing the spawn position
        RollTheDiceInternal((rollResult) => { // Renamed to avoid confusion, or make RollTheDice private
            result = rollResult;
            rollComplete = true;
        }, spawnPosition); // Pass the determined spawn position

        yield return new WaitForSeconds(delayAfterRolling);

        yield return new WaitUntil(() => rollComplete);
        onComplete?.Invoke(result);
    }

    // The method that actually spawns the die
    private void RollTheDiceInternal(System.Action<int> callback, Vector3 spawnPoint)
    {
        GameObject diceObj = Instantiate(die, spawnPoint, Quaternion.identity);
        Dice dice = diceObj.GetComponentInChildren<Dice>();

        dice.OnDiceRollComplete += (res) => {
            callback(res);
            Destroy(diceObj, 2f);
        };
        dice.PhysicalRoll();
    }
}
