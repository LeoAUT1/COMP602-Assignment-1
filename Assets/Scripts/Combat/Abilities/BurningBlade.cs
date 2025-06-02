using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BurningBlade : AbilityBase
{
    public override string AbilityName => "Burning Blade";
    public override string Description => "Inflicts damage and deals burn damage equivalent to the caster's Intelligence for 3 turns";

    public override int Cooldown => 2;

    public override int CurrentCooldown { get; set; }
    public override TargetType TargetType => TargetType.Enemy;

    public override IEnumerator Execute(CombatEntity caster, CombatEntity primaryTarget, CombatManager combatManager, CombatHud hud)
    {
        //New DoT
        DegenPassive dot = ScriptableObject.CreateInstance<DegenPassive>();

        dot.SetEffectName("Burn");
        dot.SetDescription("The target is burning");
        dot.SetDuration(3);
        dot.SetDamageAmount(caster.GetIntelligence());

        Debug.Log($"{caster} used {AbilityName}");

        if (primaryTarget == null)
        {
            hud.QueueCombatMessage($"{caster.GetName()} has no target for {AbilityName}!");
            yield break;
        }

        hud.QueueCombatMessage($"{caster.GetName()} uses {AbilityName} on {primaryTarget.GetName()}!");

        int damage = caster.GetStrength();

        List<StatusEffect> casterEffects = caster.GetActiveStatusEffects();
        foreach (var effect in casterEffects)
        {
            effect.OnDamageDealt(primaryTarget, ref damage, hud);
        }

        // Process messages queued by OnDamageDealt or the announcement
        Coroutine preDamageMessages = hud.ProcessMessageQueue();
        if (preDamageMessages != null) yield return preDamageMessages;

        // Do the thing

        int hitDamage = damage;
        bool isCrit = false;

        if (caster.CritAttempt())
        {
            isCrit = true;

            float critMultiplier = 1.5f; //crit multiplier at 150% damage
            hitDamage = Mathf.RoundToInt(damage * critMultiplier);

        }

        if (isCrit)
        {
            hud.QueueCombatMessage($"Critical Hit!");
        }
        primaryTarget.TakeDamage(hitDamage);

        // Apply to target after hit
        primaryTarget.AddStatusEffect(dot);

        // Determine if target is Player or Enemy for HUD update
        if (primaryTarget is PlayerCombat) hud.UpdatePlayerHud(primaryTarget as PlayerCombat);
        else if (primaryTarget is Enemy) hud.UpdateEnemyHud(primaryTarget as Enemy);


        Coroutine postDamageMessages = hud.ProcessMessageQueue();
        if (postDamageMessages != null) yield return postDamageMessages;
    }
}
