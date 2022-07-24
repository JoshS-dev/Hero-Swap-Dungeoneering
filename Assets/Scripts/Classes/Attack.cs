using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HSD_Utils;

[CreateAssetMenu(fileName = "New Attack", menuName = "Attack")]
public class Attack : ScriptableObject
{
    [Header("Overall")]
    public float cooldown;
    public int staminaUse;

    [Header("Hitboxes")]
    public AttackHitbox[] hitboxes = new AttackHitbox[1];
    public Vector2[] hitboxScales = new Vector2[1];
    public AttackPosition[] attackPosits = new AttackPosition[1];
    public float[] positParams = new float[1];
    public bool[] breaksOnEnv = new bool[1];

    [Header("Movement")]
    public Vector2[] playerMovements = new Vector2[1]; // X: forward-backward, Y: side-to-side
    public PlayerMoveContraints[] moveContraints = new PlayerMoveContraints[1];

    [Header("Knockbacks")]
    public float[] knockbacks = new float[1];
    public KnockbackOrigin[] knockbackOrigins = new KnockbackOrigin[1];
    public KnockbackStrength[] knockbackStrengths = new KnockbackStrength[1];

    [Header("Timing")]
    public float[] windups = new float[1];
    public float[] dmgDurations = new float[1];

    [Header("Damage")]
    public float[] flatDmgMods;
    public float[] multDmgMods;

    public float GetTotalDuration() {
        float sum = 0f;
        foreach (float f in windups) { sum += f; }
        //foreach (float f in dmgDurations) { sum += f; }
        return sum;
    }
}
