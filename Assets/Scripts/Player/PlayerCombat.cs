using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HSD_Utils;
using static GameStateManager;

public class PlayerCombat : MonoBehaviour
{
    Camera _mCam;
    PartyManager _pm;
    SpriteRenderer _sr;

    CombatHandler _ch;

    [SerializeField]
    GameObject indicator;

    //readonly int ENEMY_LAYER = 10;

    // Start is called before the first frame update
    void Start()
    {
        _mCam = Camera.main;
        _pm = GetComponent<PartyManager>();
        _sr = GetComponent<SpriteRenderer>();
        GameObject GC = GameObject.Find("/GameControl");
        _ch = GC.GetComponent<CombatHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        PartyMember current = _pm.currentHero;
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !_ch.isPlayerAttacking && CurrGamestate == GameState.Running) { // WEAPON
            if(current.mainWep != null) {
                if (Input.GetMouseButtonDown(0) && current.mainWep.primaryCooldown <= 0 && (current.stamina - current.mainWep.primary.staminaUse) >= 0) {
                    current.mainWep.primaryCooldown += current.mainWep.primary.GetTotalDuration() + current.mainWep.primary.cooldown;
                    current.stamina -= current.mainWep.primary.staminaUse;
                    StartCoroutine(_ch.PlayerAttackBootstrap(current.mainWep.primary, current.mainWep, current));
                    StartCoroutine(AttackIndicator(current.mainWep.primary.GetTotalDuration()));
                }
                else if(Input.GetMouseButtonDown(1) && current.mainWep.secondaryCooldown <= 0 && (current.stamina - current.mainWep.secondary.staminaUse) >= 0) {
                    current.mainWep.secondaryCooldown += current.mainWep.secondary.GetTotalDuration() + current.mainWep.secondary.cooldown;
                    current.stamina -= current.mainWep.secondary.staminaUse;
                    StartCoroutine(_ch.PlayerAttackBootstrap(current.mainWep.secondary, current.mainWep, current));
                    StartCoroutine(AttackIndicator(current.mainWep.secondary.GetTotalDuration()));
                }
            }
            else {
                Debug.Log("WEP NULL");
            }
        }

        if (current.mainWep != null) {
            if (current.mainWep.primaryCooldown > 0) current.mainWep.primaryCooldown -= Time.deltaTime;
            if (current.mainWep.secondaryCooldown > 0) current.mainWep.secondaryCooldown -= Time.deltaTime;
        }

        //Debug.Log(current.mainWep.primaryCooldown + " " + current.mainWep.secondaryCooldown);
    }

    private IEnumerator AttackIndicator(float duration) {
        Transform ind = Instantiate(indicator,transform).transform;
        ind.localScale = Vector3.one * 0.5f;
        ind.localPosition = Vector3.zero;
        yield return new WaitForSeconds(duration);
        Destroy(ind.gameObject);
    }

    private Vector3 GetMousePoint() {
        return _mCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
    }
}
