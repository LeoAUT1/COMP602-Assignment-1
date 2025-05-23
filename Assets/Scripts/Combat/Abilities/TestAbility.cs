using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestAbility : AbilityBase
{
    public override string AbilityName => "Test ability";
    public override string Description => "This is a test";

    public override int Cooldown => 0;

    public override int CurrentCooldown { get; set; }
    public override TargetType TargetType => TargetType.Self;

    public override IEnumerator Execute(CombatEntity caster, CombatEntity primaryTarget, CombatManager combatManager, CombatHud hud)
    {

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
        primaryTarget.AddHealth(5);

        // Determine if target is Player or Enemy for HUD update
        if (primaryTarget is PlayerCombat) hud.UpdatePlayerHud(primaryTarget as PlayerCombat);
        else if (primaryTarget is Enemy) hud.UpdateEnemyHud(primaryTarget as Enemy);


        Coroutine postDamageMessages = hud.ProcessMessageQueue();
        if (postDamageMessages != null) yield return postDamageMessages;
    }
}
