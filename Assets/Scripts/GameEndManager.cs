using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//This script handles functionality related to the game ending scene
public class GameEndManager : MonoBehaviour
{
    public TextMeshProUGUI endingText;

    private void Awake()
    {
        if (GameManager.Instance.GetIsVictorious())
        {
            PlayerVictorious();
        }
        else
        {
            AudioManager.Instance.PlayDefeatTheme();
        }
    }

    //The player has won the game, so disable and enable appropriate scene elements
    private void PlayerVictorious()
    {
        endingText.text = "The Serpent is Slain!";
        AudioManager.Instance.PlayVictoryTheme();
    }
}
