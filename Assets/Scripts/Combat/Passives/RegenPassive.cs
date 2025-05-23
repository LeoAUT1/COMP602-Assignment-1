public class RegenPassive : StatusEffect
{
    private int healAmount;

    public RegenPassive(int healPerTurn, int duration)
        : base("Regeneration", $"Heals {healPerTurn} HP at the start of the turn.", duration, false)
    {
        this.healAmount = healPerTurn;
    }

    public override void OnTurnStart(CombatHud hud) // Accept CombatHud
    {
        base.OnTurnStart(hud); // Good practice

        // Target should be set by OnApply or ensured by the calling CombatEntity method
        if (Target != null && Target.IsAlive())
        {
            Target.AddHealth(healAmount); // Or Target.Heal(healAmount)
            // Use the passed hud instance to queue the message
            hud.QueueCombatMessage($"{Target.GetName()} regenerates {healAmount} HP from {EffectName}!");
            // REMOVE: Target.UpdateHud(); // CombatManager will handle this
        }
    }
}
