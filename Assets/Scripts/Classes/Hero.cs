//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

using static HSD_Utils;

// Hero stores constant values.
public class Hero
{
    public RPGClass rpgClass;
    public string name;

    public Color heroColor;

    public int strength;
    public int dexterity;
    public int endurance;
    public int agility;

    public Hero(RPGClass cl, Color col, int str = 1, int dex = 1, int end = 1, int agi = 1, string nm = "") {
        name = nm;
        rpgClass = cl;

        heroColor = col;

        strength = str;
        dexterity = dex;
        endurance = end;
        agility = agi;
        
    }

    public override string ToString() {
        string returnStr = "";
        returnStr += "Class: " + rpgClass + ", ";
        returnStr += "STR: " + strength + ", ";
        returnStr += "DEX: " + dexterity + ", ";
        returnStr += "END: " + endurance + ", ";
        returnStr += "AGI: " + agility + ".";
        return returnStr;
    }
}
