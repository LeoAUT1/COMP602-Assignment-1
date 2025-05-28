using UnityEngine;

[CreateAssetMenu(fileName = "New Regeneration Effect", menuName = "Combat/Status Effects/Regeneration")]
public class RegenPassive : StatusEffect
{
    [SerializeField] private int healAmount = 5;

    // No constructor needed - ScriptableObjects use serialized fields instead

    public override void OnTurnStart(CombatHud hud)
    {
        base.OnTurnStart(hud);

        if (Target != null && Target.IsAlive())
        {
            Target.AddHealth(healAmount);
            hud.QueueCombatMessage($"{Target.GetName()} regenerates {healAmount} HP from {EffectName}!");
        }
    }

    // Optional: Override the Initialize method if you need to do any setup
    public override void Initialize()
    {
        base.Initialize();
        // Any additional initialization
    }
}
