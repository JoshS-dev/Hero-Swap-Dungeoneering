using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HSD_Utils;

public class PickupInstance : MonoBehaviour
{
    [SerializeField]
    public string type, subtype;

    public object content;

    public int roomSeeded = -1;

    public bool? canPickup;

    SpriteRenderer _sr;
    GameObject _GC;
    RunManager _rm;
    private WeaponGenerator _wg;

    private readonly float ALPHAPERFLATMOD = 25.5f;

    private void Awake() {
        _sr = GetComponent<SpriteRenderer>();
        _GC = GameObject.Find("/GameControl");
        _rm = _GC.GetComponent<RunManager>();

        if(type.Equals("")) { GetRandomType(); }
        if(subtype.Equals("")) { GetRandomSubtypeFromType(); }

        if (type.Equals("weapon")) {
            _wg = _GC.GetComponent<WeaponGenerator>();
        }

        canPickup = true;
        Init();
    }

    private void GetRandomType() {
        if (roomSeeded != -1) type = ProbabilityDistributionGetter(_rm.NextRandomPoolVal(Pow(roomSeeded + 2, 2)), TypeBaseProbabilities);
        else type = ProbabilityDistributionGetter(_rm.NextRandomPoolVal(), TypeBaseProbabilities);
    }

    private void GetRandomSubtypeFromType() {
        List<(string, float)> targetProbabilities;
        switch (type) {
            case "coin":    targetProbabilities = CoinSubtypeBaseProbabilities; break;
            case "heart":   targetProbabilities = HeartSubtypeBaseProbabilities; break;
            case "weapon":  targetProbabilities = WeaponSubtypeBaseProbabilities; break;
            default:        subtype = ""; return;
        }

        if (roomSeeded != -1) subtype = ProbabilityDistributionGetter(_rm.NextRandomPoolVal(Pow(roomSeeded + 2, 3)), targetProbabilities);
        else subtype = ProbabilityDistributionGetter(_rm.NextRandomPoolVal(), targetProbabilities);

        _sr.sprite = Resources.Load<Sprite>("Pickups/Sprites/" + type + "/" + type + "_" + subtype);
    }

    /*
     * weapon: parameters = int baseDmgMod, string pri, string sec, float priCooldown, float secCooldown
     * 
     */
    public void Init(params object[] args) {
        if(type.Equals("coin")) {
            switch (subtype) {
                case "copper": content = 1; break;
                case "silver": content = 5; break;
                case "gold": content = 10; break;
            }
        }
        else if (type.Equals("heart")) {
            switch (subtype) {
                case "small": content = 5; break;
                case "medium": content = 15; break;
                case "large": content = 99; break;
            }
        }
        else if (type.Equals("weapon")) {
            WeaponClass wcls = StringToWepClass[subtype];
            int baseDmgMod;
            string desiredPri, desiredSec;
            float priCooldown, secCooldown;

            try { baseDmgMod = (int)args[0]; }
            catch { baseDmgMod = 0; }

            try { desiredPri = (string)args[1]; }
            catch { desiredPri = null; }

            try { desiredSec = (string)args[2]; }
            catch { desiredSec = null; }

            try { priCooldown = (float)args[3]; }
            catch { priCooldown = -1f; }

            try { secCooldown = (float)args[4]; }
            catch { secCooldown = -1f; }

            float method = 0f;
            if(desiredPri == null || desiredSec == null) {
                method = _rm.NextRandomPoolVal();
            }

            Weapon wep = _wg.GenerateWeapon(_rm.RUN_SEED, wcls, method, baseDmgMod, desiredPri, desiredSec);
            if (priCooldown != -1f) wep.primaryCooldown = priCooldown;
            if (secCooldown != -1f) wep.secondaryCooldown = secCooldown;

            content = wep;

            SpriteRenderer auraSprite = transform.Find("RarityAura").gameObject.GetComponent<SpriteRenderer>();
            if (baseDmgMod > 0) {
                auraSprite.color = ChangeColorAlpha(auraSprite.color, Mathf.Clamp01(((baseDmgMod * ALPHAPERFLATMOD) + ALPHAPERFLATMOD) / 255f));
            }
            else {
                auraSprite.color = ChangeColorAlpha(auraSprite.color, 0f);
            }
        }
    }
}
