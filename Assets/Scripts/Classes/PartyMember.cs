using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HSD_Utils;

// Beyond the hero, all the aspects of PartyMember are mutable and can change.
public class PartyMember
{
    public Hero hero;

    public int currHealth;
    public int maxHealth;

    public int stamina = 100;

    public Weapon mainWep;

    public PartyMember(Hero h) {
        hero = h;
        maxHealth = currHealth = EnduranceToHealth();
        mainWep = null;
    }

    public PartyMember(Hero h, Weapon w) {
        hero = h;
        maxHealth = currHealth = EnduranceToHealth();
        mainWep = w;
    }

    public void FullRestore() {
        currHealth = maxHealth;
        stamina = 100;
    }

    // 4 = base speed
    // 1 = half speed
    // 8 = 1.5x speed
    public readonly int BASESPEEDTHRESHOLD = 4;
    public float AgilityToSpeedScalar() {
        if (hero.agility <= BASESPEEDTHRESHOLD) {
            return ((hero.agility - 1f) / 6f) + 0.5f;
        }
        else { // agi > 4
            return ((hero.agility - 4f) / 8f) + 1f;
        }
    }

    public int EnduranceToHealth() {
        return 20 + (5 * (hero.endurance - 1));
    }

    // 1 = lowest possible
    public int BonusBaseDMGFromStats() {
        switch (mainWep.wClass) {
            case WeaponClass.Sword: return (hero.dexterity - 1) + (hero.strength - 1) - 2;
            case WeaponClass.Bow: return 2 * (hero.dexterity - 1) - 2;
            case WeaponClass.Hammer: return 2 * (hero.strength - 1) - 2;
            default: return 0;
        }
    }

    public override string ToString() {
        string returnStr = "";
        returnStr += hero.ToString() + "\t";
        returnStr += "Health: " + currHealth + "/" + maxHealth + "\t";
        returnStr += "Stamina: " + stamina + "/" + 100;
        return returnStr;
    }
}
