using UnityEngine;

[CreateAssetMenu(fileName = "New Vampirism", menuName = "Combat/Status Effects/Vampirism")]
public class Vampirism : StatusEffect
{

    public override void OnDamageDealt(CombatEntity caster, CombatEntity victim, ref int damageAmount, CombatHud hud)
    {
        base.OnDamageDealt(caster, victim, ref damageAmount, hud);

        int recoveryAmount = damageAmount / 2 * count;

        Debug.Log("Vampirism OnDamageDealt triggered");
        hud.QueueCombatMessage($"{Target.GetName()} drained {recoveryAmount} HP from {victim.GetName()}!");

        caster.AddHealth(recoveryAmount);
    }

    // Optional: Override the Initialize method if you need to do any setup
    public override void Initialize()
    {
        base.Initialize();
        // Any additional initialization
    }
}
