using UnityEngine;

[CreateAssetMenu(fileName = "New Regeneration Effect", menuName = "Combat/Status Effects/Degeneration")]
public class DegenPassive : StatusEffect
{
    [SerializeField] private int damageAmount = 5;

    public override void OnTurnStart(CombatHud hud)
    {
        base.OnTurnStart(hud);

        int dmgAm = damageAmount * count;

        if (Target != null && Target.IsAlive())
        {
            Target.TakeDamage(dmgAm);
            hud.QueueCombatMessage($"{Target.GetName()} takes {dmgAm} damage from {EffectName}!");
        }
    }

    // Override the Initialize method if you need to do any setup
    public override void Initialize()
    {
        base.Initialize();
        // Additional code goes here
    }

    public void SetDamageAmount(int amount) => damageAmount = amount;
}
