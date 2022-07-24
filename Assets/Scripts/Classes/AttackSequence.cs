using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HSD_Utils;

public class AttackSequence
{
    public AttackHitbox hitbox;
    public Vector2 hitboxScale;
    public AttackPosition attackPosit;
    public float positParam;
    public bool breakOnEnv;

    public Vector2 playerMovement; // X: forward-backward, Y: side-to-side
    public PlayerMoveContraints moveContraint;

    public float knockback;
    public KnockbackOrigin knockbackOrigin;
    public KnockbackStrength knockbackStrength;

    public float windup;
    public float dmgDuration;

    public float flatDmgMod;
    public float multDmgMod;

    public AttackSequence(Attack atk, int idx) {
        hitbox = atk.hitboxes[idx];
        hitboxScale = atk.hitboxScales[idx];
        attackPosit = atk.attackPosits[idx];
        positParam = atk.positParams[idx];
        breakOnEnv = atk.breaksOnEnv[idx];

        playerMovement = atk.playerMovements[idx];
        moveContraint = atk.moveContraints[idx];

        knockback = atk.knockbacks[idx];
        knockbackOrigin = atk.knockbackOrigins[idx];
        knockbackStrength = atk.knockbackStrengths[idx];

        windup = atk.windups[idx];
        dmgDuration = atk.dmgDurations[idx];

        if (idx >= atk.flatDmgMods.Length) flatDmgMod = 0;
        else flatDmgMod = atk.flatDmgMods[idx];
        if (idx >= atk.multDmgMods.Length) multDmgMod = 1;
        else multDmgMod = atk.multDmgMods[idx];
    }
}
