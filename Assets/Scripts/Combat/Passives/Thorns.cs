using UnityEngine;

[CreateAssetMenu(fileName = "New Thorns", menuName = "Combat/Status Effects/Thorns")]
public class Thorns : StatusEffect
{
    public float reflectMultiplier = 0.25f;
    public override void OnDamageDealt(CombatEntity caster, CombatEntity victim, ref int damageAmount, CombatHud hud)
    {
        base.OnDamageDealt(caster, victim, ref damageAmount, hud);

        int reflectAmount = (int) ((float) damageAmount / reflectMultiplier);

        Debug.Log("Thorns OnDamageDealt triggered");
        hud.QueueCombatMessage($"{Target.GetName()} reflected {reflectAmount} damage!");

        victim.TakeDamage(reflectAmount);
    }

    // Optional: Override the Initialize method if you need to do any setup
    public override void Initialize()
    {
        base.Initialize();
        // Any additional initialization
    }
}
