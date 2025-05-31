using UnityEngine;

//This script exists so that the powerups can be serialised, and easily accessed via the player.
public class PowerupDistribution : MonoBehaviour
{
    [SerializeField] private PowerupData[] powerups;

    [SerializeField] private GameObject powerupModal; // Display this when the player gets a powerup
    public PowerupData GrantRandomPowerup()
    {
        PowerupData pup = powerups[Random.Range(0, powerups.Length)];
        PowerupData instancedPup = Instantiate(pup);

        Debug.Log($"Player got random power up {instancedPup.powerupName}");

        GameObject instancedModal = Instantiate(powerupModal);
        PowerUpIcon icon = instancedModal.GetComponent<PowerUpIcon>();
        Debug.Log(powerupModal);
        Debug.Log(powerupModal.name);
        Debug.Log(icon);
        icon.SetAll(instancedPup);

        return instancedPup;
    }
}
