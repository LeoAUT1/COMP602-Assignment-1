using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceScore : MonoBehaviour
{
    DiceRoll dice;
    
    [SerializeField]
    Text scoreText;
    
    private void Awake()
    {
        dice = FindObjectOfType<DiceRoll>();
    }

    private void Update()
    {
        if (dice != null)
        {
            if (dice.diceFaceNum != 0)
            {
                scoreText.text = dice.diceFaceNum.ToString();
            }
        }
    }
}
