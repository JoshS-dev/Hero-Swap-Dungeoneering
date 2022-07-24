using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HSD_Utils;

public class WeaponGenerator : MonoBehaviour
{
    //RunManager _rm;

    private void Awake() {
        //_rm = GetComponent<RunManager>();
    }

    public Weapon GenerateWeapon(int seed, WeaponClass wepC, float method = 0f, int damageFlatMod = 0, string desiredPri = null, string desiredSec = null) {
        string wepPath = "";
        if (wepC == WeaponClass.Sword) wepPath = "Sword";
        else if (wepC == WeaponClass.Bow) wepPath = "Bow";
        else if (wepC == WeaponClass.Hammer) wepPath = "Hammer";

        Attack[] primaries = Resources.LoadAll<Attack>("Attacks/" + wepPath + "/pri");
        Attack[] secondaries = Resources.LoadAll<Attack>("Attacks/" + wepPath + "/sec");

        Attack primary = null, secondary = null;

        if (desiredPri != null) { primary = FindAttackByString(primaries, desiredPri); }
        if (desiredSec != null) { secondary = FindAttackByString(secondaries, desiredSec); }

        if (primary == null) { primary = randFromList(primaries, seed, method); }
        if (secondary == null) { secondary = randFromList(secondaries, seed, method); }

        return new Weapon(wepC, damageFlatMod, primary, secondary);
    }

    private Attack FindAttackByString(Attack[] list, string atk) {
        foreach(Attack a in list) {
            if (a.ToString().Equals(atk + " (Attack)") || a.ToString().Equals(atk)) return a;
        }
        return null;
    }
}
