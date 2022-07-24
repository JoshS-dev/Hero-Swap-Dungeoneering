//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

using static HSD_Utils;

public class Weapon
{
    public WeaponClass wClass;
    
    public int baseDmgMod;

    public Attack primary;
    public Attack secondary;

    public float primaryCooldown;
    public float secondaryCooldown;

    public Weapon(WeaponClass wc, int dmg, Attack pri, Attack sec) {
        wClass = wc;
        baseDmgMod = dmg;
        primary = pri;
        secondary = sec;
    }

    public override string ToString() {
        return "DMG:" + (WeaponBaseDamages[wClass] + baseDmgMod) + " Primary: " + primary + " Secondary: " + secondary;
    }
}
