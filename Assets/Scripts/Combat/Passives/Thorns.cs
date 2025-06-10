using UnityEngine;

[CreateAssetMenu(fileName = "New Thorns", menuName = "Combat/Status Effects/Thorns")]
public class Thorns : StatusEffect
{
    public float reflectMultiplier = 0.25f;
    public override void OnDamageTaken(CombatEntity source, CombatEntity victim, ref int damageAmount, CombatHud hud)
    {
        base.OnDamageTaken(source, victim, ref damageAmount, hud);

        Debug.Log("Thorns OnDamageDealt triggered");
        Debug.Log($"Thorns targetting {victim}");
        Debug.Log($"{victim}");

        int reflectAmount = (int) ((float) damageAmount * reflectMultiplier * count);
        hud.QueueCombatMessage($"Reflected {reflectAmount} damage!");

        victim.SubtractHealth(reflectAmount);
    }

    // Optional: Override the Initialize method if you need to do any setup
    public override void Initialize()
    {
        base.Initialize();
        // Any additional initialization
    }

    public void SetReflectMultiplier(float multi)
    {
        reflectMultiplier = multi;
    }

}
