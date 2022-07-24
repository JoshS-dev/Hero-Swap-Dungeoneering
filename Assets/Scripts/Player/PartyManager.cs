using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HSD_Utils;

public class PartyManager : MonoBehaviour
{
    GameObject _GC;
    SpriteRenderer _sr;
    WeaponGenerator _wg;
    CombatHandler _ch;
    RunManager _rm;
    
    public PartyMember[] heroes = new PartyMember[3];
    public PartyMember currentHero;
    public int currentIdx;

    public int coinCount;

    public bool isSwitching;
    private bool switchFrameProtect;

    private float nextStaminaRegenTime;
    private float staminaTickSpeed = 0.375f;

    // Start is called before the first frame update
    void Start()
    {
        _GC = GameObject.Find("/GameControl");

        _sr = GetComponent<SpriteRenderer>();
        
        _wg = _GC.GetComponent<WeaponGenerator>();
        _ch = _GC.GetComponent<CombatHandler>();
        _rm = _GC.GetComponent<RunManager>();

        // new Hero(class,color,str,dex,end,agi,[name])
        /*
        heroes[0] = new PartyMember(new Hero(RPGClass.Warrior, new Color(0f, 0f, 1f), 4, 4, 4, 2, "Lance"), _wg.GenerateWeapon(_rm.RUN_SEED, WeaponClass.Sword));
        heroes[1] = new PartyMember(new Hero(RPGClass.Archer, new Color(1f, 1f, 1f), 2, 5, 2, 5, "Robin"), _wg.GenerateWeapon(_rm.RUN_SEED, WeaponClass.Bow));
        heroes[2] = new PartyMember(new Hero(RPGClass.Warrior, new Color(0.5f, 0.5f, 0.5f), 5, 1, 6, 1, "Thor"), _wg.GenerateWeapon(_rm.RUN_SEED, WeaponClass.Hammer));
        */
        heroes[0] = new PartyMember(new Hero(RPGClass.Warrior, new Color(0f, 0f, 1f), 4, 4, 4, 2, "Lance"));
        heroes[1] = new PartyMember(new Hero(RPGClass.Archer, new Color(1f, 1f, 1f), 2, 5, 2, 5, "Robin"));
        heroes[2] = new PartyMember(new Hero(RPGClass.Warrior, new Color(0.5f, 0.5f, 0.5f), 5, 1, 6, 1, "Thor"));

        HeroSwitchTo(0, false);
        coinCount = 0;
        isSwitching = false;
        switchFrameProtect = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSwitching) {
            //Debug.Log("SWITCHING");
            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                isSwitching = false;
                switchFrameProtect = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1) && currentIdx != 0) {
                HeroSwitchTo(0);
                isSwitching = false;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && currentIdx != 1) {
                HeroSwitchTo(1);
                isSwitching = false;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) && currentIdx != 2) {
                HeroSwitchTo(2);
                isSwitching = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            if (switchFrameProtect) switchFrameProtect = false;
            else isSwitching = true;
        }

        if (_ch.isPlayerAttacking) {
            isSwitching = false;
            switchFrameProtect = false;
        }

        // UNUSED STAMINA REGEN, 1% per (staminaTickSpeed)s
        List<int> unusedHeroes = new List<int> { 0, 1, 2 };
        unusedHeroes.RemoveAt(currentIdx);
        if (Time.time > nextStaminaRegenTime) {
            nextStaminaRegenTime += staminaTickSpeed;
            foreach (int i in unusedHeroes) {
                if (heroes[i].stamina < 100) {
                    heroes[i].stamina += 1;
                }
            }
        }
        /*
        if (Input.GetKeyDown(KeyCode.Z)) {
            currentHero.mainWep = _wg.GenerateWeapon(_rm.RUN_SEED, currentHero.mainWep.wClass);
        }
        */
    }

    public void HeroSwitchTo(int heroIdx, bool doEffects = true) {
        currentIdx = heroIdx;
        currentHero = heroes[currentIdx];
        _sr.color = currentHero.hero.heroColor;
        //Debug.Log(currentHero);
    }

    public void RandomizePartyWeapons() {
        heroes[0].mainWep = _wg.GenerateWeapon(_rm.RUN_SEED, WeaponClass.Sword);
        heroes[1].mainWep = _wg.GenerateWeapon(_rm.RUN_SEED, WeaponClass.Bow);
        heroes[2].mainWep = _wg.GenerateWeapon(_rm.RUN_SEED, WeaponClass.Hammer);
    }

    public void FullHealParty() {
        foreach(PartyMember p in heroes) {
            p.FullRestore();
        }
    }
}
