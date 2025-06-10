using UnityEngine;

[CreateAssetMenu(fileName = "New Regeneration Effect", menuName = "Combat/Status Effects/Regeneration")]
public class RegenPassive : StatusEffect
{
    [SerializeField] private int healAmount = 5;

    public override void OnTurnStart(CombatHud hud)
    {
        base.OnTurnStart(hud);

        if (Target != null && Target.IsAlive())
        {
            int healAm = healAmount * count;
            Target.AddHealth(healAm);
            hud.QueueCombatMessage($"{Target.GetName()} regenerates {healAm} HP from {EffectName}!");
        }
    }

    // Optional: Override the Initialize method if you need to do any setup
    public override void Initialize()
    {
        base.Initialize();
        // Any additional initialization
    }

    public void SetHealAmount(int amount) => healAmount = amount;
}
