using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // Start is called before the first frame update
   
    public string unitName;
    public int unitLevel;

    public int damage;

    public int maxHp;
    public int currentHp;


    //taking damage 
    public bool TakeDamage(int dmg)
    {
        currentHp -= dmg;

        if (currentHp <= 0)
            return true;
        else
            return false;
    }

    public void heal(int amount)
    {
        currentHp += amount;
        if (currentHp > maxHp)
        {
            currentHp = maxHp;
        }
    }
}
