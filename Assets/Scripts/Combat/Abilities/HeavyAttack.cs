using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HeavyAttack : AbilityBase
{
    public override string AbilityName => "Heavy Attack";
    public override string Description => "Massive physical attack with double the damage but inflicts recoil";

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

        // Determine if target is Player or Enemy for HUD update
        if (primaryTarget is PlayerCombat) hud.UpdatePlayerHud(primaryTarget as PlayerCombat);
        else if (primaryTarget is Enemy) hud.UpdateEnemyHud(primaryTarget as Enemy);


        // recoil damage
        float recoilPercent = 0.2f; // 20% of damage as recoil. change if needed
        int recoilDamage = Mathf.RoundToInt(damage * recoilPercent);

        caster.TakeDamage(recoilDamage);
        hud.QueueCombatMessage($"you take some recoil damage");

        if (caster is PlayerCombat)
            hud.UpdatePlayerHud(caster as PlayerCombat);


        Coroutine postDamageMessages = hud.ProcessMessageQueue();
        if (postDamageMessages != null) yield return postDamageMessages;
    }
}
