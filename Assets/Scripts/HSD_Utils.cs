using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//using System;

public static class HSD_Utils {
    private static System.Random SeedlessRandom = new System.Random();

    public enum GameState {
        Undefined,

        Running,
        Paused,
        Dead,
    }

    public enum RPGClass {
        None,

        Warrior,
        Archer,
    }

    public enum WeaponClass {
        None,

        Sword,
        Hammer,
        Bow,
    }

    public static Dictionary<WeaponClass, int> WeaponBaseDamages = new Dictionary<WeaponClass, int>() {
        {WeaponClass.None,0},
        {WeaponClass.Sword,6},
        {WeaponClass.Bow,4},
        {WeaponClass.Hammer,8},
    };

    public static Dictionary<RPGClass, List<WeaponClass>> ValidWeapons = new Dictionary<RPGClass, List<WeaponClass>>{
        {RPGClass.Warrior,new List<WeaponClass>{WeaponClass.Sword,WeaponClass.Hammer } },
        {RPGClass.Archer,new List<WeaponClass>{WeaponClass.Bow} },
    };

    public static string[] AllTypes = new string[] { "coin", "heart", "weapon" };
    public static List<(string, float)> TypeBaseProbabilities = new List<(string, float)>() {
        ( AllTypes[0], 0.6f ),
        ( AllTypes[1], 0.3f ),
        ( AllTypes[2], 0.1f ),
    };

    public static string[] coinSubtypes = new string[] { "copper", "silver", "gold" };
    public static List<(string, float)> CoinSubtypeBaseProbabilities = new List<(string, float)>() {
        ( coinSubtypes[0], 0.85f ),
        ( coinSubtypes[1], 0.125f ),
        ( coinSubtypes[2], 0.025f ),
    };

    public static string[] heartSubtypes = new string[] { "small", "medium", "large" };
    public static List<(string, float)> HeartSubtypeBaseProbabilities = new List<(string, float)>() {
        ( heartSubtypes[0], 0.70f ),
        ( heartSubtypes[1], 0.225f ),
        ( heartSubtypes[2], 0.075f ),
    };

    public static string[] weaponSubtypes = new string[] { "sword", "hammer", "bow" };
    public static List<(string, float)> WeaponSubtypeBaseProbabilities = new List<(string, float)>() {
        (weaponSubtypes[0], 1f ),
        (weaponSubtypes[1], 1f ),
        (weaponSubtypes[2], 1f ),
    };

    public static Dictionary<string, WeaponClass> StringToWepClass = new Dictionary<string, WeaponClass>{
        {weaponSubtypes[0], WeaponClass.Sword },
        {weaponSubtypes[1], WeaponClass.Hammer },
        {weaponSubtypes[2], WeaponClass.Bow },
    };

    public enum AttackHitbox {
        None,

        Triangle,
        Circle,
        Quad,
        Hex,
    }

    public enum KnockbackOrigin {
        Player,
        Hitbox,
        PlayerHitboxMid,
    }

    public enum AttackPosition {
        None,

        InfrontPlayer,
        OnPlayer,
        OnMouse,
        Projectile,
        Previous,
    }

    public enum KnockbackStrength {
        None,

        InterruptMovement,
        InterruptAttack,
        InterruptBoth,
    }

    public enum PlayerMoveContraints {
        None,

        CantMove, // CAN STILL ROLL
        CantRoll,
        CantEither,
    }

    public enum EnemyAttackRequirement {
        None,

        DistanceFrom,
        CardinalTo,
        VisibleTo,
    }

    public enum EnemyMovementType {
        Stationary,

        Chase,
        Scutter,
        Wander,
        Retreat,
    }

    public static RoomPosition[] AllRoomPositions = new RoomPosition[4] { RoomPosition.Up, RoomPosition.Right, RoomPosition.Down, RoomPosition.Left };
    public static List<RoomPosition> AllRoomPositionsL = new List<RoomPosition>(AllRoomPositions);
    public enum RoomPosition {
        None,

        Up,
        Right,
        Down,
        Left,
    }
    public static bool AreDirectionsPerpendicular(RoomPosition a, RoomPosition b) {
        if ((RoomPosition)underOverflowCalc((int)a + 1, 1, 4) == b) return true;
        if ((RoomPosition)underOverflowCalc((int)a - 1, 1, 4) == b) return true;
        return false;
    }

    public enum RoomType {
        Undefined,

        Starting,
        Normal,
        Boss,
    }

    public enum MapFeature {
        Empty,
        None,
        Corner,
        DeadEnd,
        BranchOff,
        Junction,
    }

    public static int? LAST_KNOWN_SEED = null;
    public static void SetSeed(int seed, bool forceNew = false) {
        if (seed > -1) {
            if (seed != LAST_KNOWN_SEED || forceNew) {
                Random.InitState(seed);
                LAST_KNOWN_SEED = seed;
                Debug.Log("LAST_KNOWN_SEED = " + LAST_KNOWN_SEED.ToString());
            }
        }
        else { // generate a seed to use and record if one isnt present, if seed=-2, always generate new
            if(LAST_KNOWN_SEED == null || forceNew) {
                LAST_KNOWN_SEED = Random.Range(100000000, 1000000000); // Generates 9-digit long number
                Random.InitState((int)LAST_KNOWN_SEED);
                Debug.Log("LAST_KNOWN_SEED = " + LAST_KNOWN_SEED.ToString());
            }
        }
    }

    public static float StandardUniformToRange(float uniform, float min, float max) {
        return uniform * (max - min) + min;
    }
    public static int StandardUniformToRange(float uniform, int min, int max) {
        return Mathf.RoundToInt(uniform * (max - min) + min);
    }

    public static T ProbabilityDistributionGetter<T>(float uniform, List<(T, float)> probabilityDistribution) {
        uniform = StandardUniformToRange(uniform, 0f, probabilityDistribution.Sum(t => t.Item2));
        float cumulative = 0f;
        foreach ((T, float) t in probabilityDistribution) {
            cumulative += t.Item2;
            if(uniform <= cumulative) {
                return t.Item1;
            }
        }
        return probabilityDistribution[probabilityDistribution.Count - 1].Item1;
    }

    public static float SeedIndependantRange(float min = 0f, float max = 1f) {
        return StandardUniformToRange((float)SeedlessRandom.NextDouble(), min, max);
    }

    // method = 0, seeded alongside level generation
    // else, seeded with uniform random
    public static T randFromList<T>(T[] list,int seed, float method = 0f) {
        if (method == 0f) {
            SetSeed(seed);
            return list[Random.Range(0, list.Length)];
        }
        else {
            return list[StandardUniformToRange(method, 0, list.Length - 1)];
        }
    }

    public static T randFromList<T>(List<T> list, int seed, float method = 0f) {
        if (method == 0f) {
            SetSeed(seed);
            return list[Random.Range(0, list.Count)];
        }
        else {
            return list[StandardUniformToRange(method, 0, list.Count - 1)];
        }
    }

    public static string ListToString<T>(List<T> list) where T: struct {
        if (list.Count == 0) return "Empty List";
        string returnString = "";
        foreach (T item in list) {
            if (item.Equals(null)) returnString += "null, ";
            else returnString += item.ToString() + ", ";
        }
        return returnString.Substring(0, returnString.Length - 2);
    }

    public static bool AddIfNotIn<T>(List<T> list, T item) {
        if (!list.Contains(item)) {
            list.Add(item);
            return true;
        }
        return false;
    }

    public static T ListPop<T>(List<T> list, int? idx = null) {
        if(idx == null) { idx = list.Count - 1; }
        T item = list[(int)idx];
        list.RemoveAt((int)idx);
        return item;
    }

    public static List<T> CopyList<T>(List<T> list) {
        List<T> returnList = new List<T>();
        foreach(T t in list) {
            returnList.Add(t);
        }
        return returnList;
    }

    /*
     * method
     * 0 = level-affecting seed
     * 1 = pure random seed
     * 2 = seed value-bank
     */
    public static T[] ShuffleArray<T>(T[] array, int method) {
        RunManager _rm = null;
        for(int i = 0; i < array.Length; i++) {
            int rnd;
            switch (method) {
                case 0: rnd = Random.Range(i, array.Length); break;
                case 1: rnd = SeedlessRandom.Next(i, array.Length); break;
                case 2: 
                    if(_rm == null) { _rm = GameObject.Find("/GameControl").GetComponent<RunManager>(); }
                    rnd = StandardUniformToRange(_rm.NextRandomPoolVal(), i, array.Length); break;
                default: return array;
            }
            SwapInArray(i, rnd, array);
        }
        return array;
    }
    public static List<T> ShuffleList<T>(List<T> list, int method) {
        RunManager _rm = null;
        for (int i = 0; i < list.Count; i++) {
            int rnd;
            switch (method) {
                case 0: rnd = Random.Range(i, list.Count); break;
                case 1: rnd = SeedlessRandom.Next(i, list.Count); break;
                case 2:
                    if (_rm == null) { _rm = GameObject.Find("/GameControl").GetComponent<RunManager>(); }
                    rnd = StandardUniformToRange(_rm.NextRandomPoolVal(), i, list.Count); break;
                default: return list;
            }
            SwapInList(i, rnd, list);
        }
        return list;
    }

    public static T[] SwapInArray<T>(int idx1, int idx2, T[] array) {
        T temp = array[idx1];
        array[idx1] = array[idx2];
        array[idx2] = temp;
        return array;
    }
    public static List<T> SwapInList<T>(int idx1, int idx2, List<T> list) {
        T temp = list[idx1];
        list[idx1] = list[idx2];
        list[idx2] = temp;
        return list;
    }

    public static bool IsBetweenInc(int value, int min, int max) {
        return (value >= min) && (value <= max);
    }
    public static bool IsBetweenInc(float value, float min, float max) {
        return (value >= min) && (value <= max);
    }

    public static int underOverflowCalc(int expression, int min, int max) {
        if (IsBetweenInc(expression, min, max)) return expression;
        int modResult = (expression - min) % (1 + max - min);
        if (expression > max) {
            return min + modResult;
        }
        //if (expression < min)
        if(modResult != 0) {
            return max + (modResult + 1);
        }
        return min;
    }

    public static Vector2Int Shift2IVector(Vector2Int v, int x, int y, int? forceX = null, int? forceY = null) {
        Vector2Int shift = new Vector2Int(x, y);
        if (forceX != null) v.x = (int)forceX;
        if (forceY != null) v.y = (int)forceY;
        return v + shift;
    }

    public static T MatrixAtCoords<T>(List<List<T>> matrix, Vector2Int coords) {
        if (!IsBetweenInc(coords.x, 0, matrix.Count - 1) || !IsBetweenInc(coords.y, 0, matrix[0].Count - 1)) return default(T);
        return matrix[coords.x][coords.y];
    }

    public static int DmgFromSeq(AttackSequence seq, Weapon wep = null, PartyMember member = null, bool ignoreStats = false) {
        if(wep == null) {
            return Mathf.Max(0, Mathf.FloorToInt(seq.flatDmgMod * seq.multDmgMod));
        }
        if (ignoreStats || member == null) {
            return Mathf.Max(0,Mathf.FloorToInt(((WeaponBaseDamages[wep.wClass] + wep.baseDmgMod) * seq.multDmgMod) + seq.flatDmgMod));
        }
        return Mathf.Max
            (0,
            Mathf.FloorToInt(
                ((WeaponBaseDamages[wep.wClass] + wep.baseDmgMod + member.BonusBaseDMGFromStats()) * seq.multDmgMod) + seq.flatDmgMod
            )
            );
    }

    public static Color ChangeColorAlpha(Color color, float alpha) {
        return new Color(color.r, color.g, color.b, alpha);
    }

    public static int Pow(int num, int exp) {
        return Mathf.RoundToInt(Mathf.Pow(num, exp));
    }
    public static float Pow(float num, float exp) {
        return Mathf.Pow(num, exp);
    }
}
