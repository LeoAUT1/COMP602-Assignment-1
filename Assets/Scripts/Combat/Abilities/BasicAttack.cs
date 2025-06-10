using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BasicAttackAbility : AbilityBase
{
    public override string AbilityName => "Attack";
    public override string Description => "A standard physical attack.";

    public override int Cooldown => 0;

    public override int CurrentCooldown { get; set; }
    public override TargetType TargetType => TargetType.Enemy;

    public override IEnumerator Execute(CombatEntity caster, CombatEntity primaryTarget, CombatManager combatManager, CombatHud hud)
    {
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
            Debug.Log(effect);
            effect.OnDamageDealt(caster, primaryTarget, ref damage, hud);
        }

        // Process messages queued by OnDamageDealt or the announcement
        Coroutine preDamageMessages = hud.ProcessMessageQueue();
        if (preDamageMessages != null) yield return preDamageMessages;

        primaryTarget.TakeDamage(damage, caster);

        // Determine if target is Player or Enemy for HUD update
        if (primaryTarget is PlayerCombat) hud.UpdatePlayerHud(primaryTarget as PlayerCombat);
        else if (primaryTarget is Enemy) hud.UpdateEnemyHud(primaryTarget as Enemy);


        Coroutine postDamageMessages = hud.ProcessMessageQueue();
        if (postDamageMessages != null) yield return postDamageMessages;
    }
}
