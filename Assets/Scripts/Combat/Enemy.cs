using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Define the options for the serializable field
public enum EnemyPassive
{
    Thorns,
    Regen,
    Decay,
    None
}

// Implementation example class
public class Enemy : CombatEntity
{
    [SerializeField] protected int experienceGranted; //experience given to the player when they defeat this enemy

    [SerializeField] private EnemyPassive passiveType = EnemyPassive.None; // Default to None

    public int getExperience() { return experienceGranted; }

    void Start() // Assuming CombatEntity has a virtual Start, otherwise just 'void Start()'
    {

        //This is incredibly shit code but we're incredibly short on time right now
        Thorns thorns = ScriptableObject.CreateInstance<Thorns>();

        thorns.SetEffectName("Thorns");
        thorns.SetDescription("Recoil damage");
        thorns.SetDuration(-1);
        thorns.SetReflectMultiplier(0.1f); //Reflect 10% damage 


        //Enemy with this passive takes 5 damage per turn for 3 turns
        DegenPassive dot = ScriptableObject.CreateInstance<DegenPassive>();

        dot.SetEffectName("Decay");
        dot.SetDescription("The target is burning");
        dot.SetDuration(3);
        dot.SetDamageAmount(5);

        //Enemy with this passive recovers health
        RegenPassive regen = ScriptableObject.CreateInstance<RegenPassive>();

        regen.SetEffectName("Regen");
        regen.SetDescription("The target is burning");
        regen.SetDuration(3);
        regen.SetHealAmount(5);


        switch (passiveType)
        {
            case EnemyPassive.None:
                //No passive, do nothing
                break;
            case EnemyPassive.Thorns:
                AddStatusEffect(thorns,1);
                break;

            case EnemyPassive.Regen:
                AddStatusEffect(regen,1);
                break;

            case EnemyPassive.Decay:
                AddStatusEffect(dot,1);

                break;

            default:
                break;
        }
    }
}
