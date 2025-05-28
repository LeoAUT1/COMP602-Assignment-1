using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HeavyAttack : AbilityBase
{
    public override string AbilityName => "Heavy Attack";
    public override string Description => "Moderate physical attack with double the damage";

    public override int Cooldown => 2;

    public override int CurrentCooldown { get; set; }
    public override TargetType TargetType => TargetType.Enemy;

    public override IEnumerator Execute(CombatEntity caster, CombatEntity primaryTarget, CombatManager combatManager, CombatHud hud)
    {

        Debug.Log($"{caster} used {AbilityName}");

        if (primaryTarget == null)
        {
            hud.QueueCombatMessage($"{caster.GetName()} has no target for {AbilityName}!");
            yield break;
        }

        hud.QueueCombatMessage($"{caster.GetName()} uses {AbilityName} on {primaryTarget.GetName()}!");

        int damage = caster.GetStrength() * 2;

        List<StatusEffect> casterEffects = caster.GetActiveStatusEffects();
        foreach (var effect in casterEffects)
        {
            effect.OnDamageDealt(primaryTarget, ref damage, hud);
        }

        // Process messages queued by OnDamageDealt or the announcement
        Coroutine preDamageMessages = hud.ProcessMessageQueue();
        if (preDamageMessages != null) yield return preDamageMessages;

        // Do the thing
        primaryTarget.TakeDamage(damage);

        // Determine if target is Player or Enemy for HUD update
        if (primaryTarget is PlayerCombat) hud.UpdatePlayerHud(primaryTarget as PlayerCombat);
        else if (primaryTarget is Enemy) hud.UpdateEnemyHud(primaryTarget as Enemy);


        Coroutine postDamageMessages = hud.ProcessMessageQueue();
        if (postDamageMessages != null) yield return postDamageMessages;
    }
}
