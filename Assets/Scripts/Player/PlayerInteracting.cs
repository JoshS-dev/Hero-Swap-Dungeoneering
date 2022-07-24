using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using static HSD_Utils;

public class PlayerInteracting : MonoBehaviour
{
    GameObject _GC;

    PartyManager _pm;

    CombatHandler _ch;

    [SerializeField]
    GameObject TEMP_NUM;

    // Start is called before the first frame update
    void Start()
    {
        _GC = GameObject.Find("/GameControl");
        
        _pm = GetComponent<PartyManager>();

        _ch = _GC.GetComponent<CombatHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.CompareTag("Pickup")) {
            PickupInstance pu = col.GetComponent<PickupInstance>();
            bool nonNullCanPickup;
            switch (pu.canPickup) {
                case null: nonNullCanPickup = false; break;
                default: nonNullCanPickup = (bool)pu.canPickup; break;
            }
            if (nonNullCanPickup) {
                if (pu.type.Equals("coin")) {
                    _pm.coinCount += (int)pu.content;
                    
                    Color coinColor;
                    switch ((int)pu.content) {
                        case 1:     coinColor = new Color(206f / 255f, 160f / 255f, 105f / 255f); break;
                        case 5:     coinColor = new Color(150f / 255f, 150f / 255f, 150f / 255f); break;
                        case 10:    coinColor = new Color(255f / 255f, 223f / 255f,   0f / 255f); break;
                        default:    coinColor = Color.white; break;
                    }
                    Destroy(col.gameObject);

                    GameObject newTempNum = Instantiate(TEMP_NUM, transform.Find("Canvas/TemporaryNumbers"));
                    newTempNum.GetComponent<TempNumberManager>().Init("+" + ((int)pu.content).ToString(), coinColor, 1.0f, false);
                }
                else if (pu.type.Equals("heart")) {
                    if(_pm.currentHero.currHealth != _pm.currentHero.maxHealth) {
                        int healthBefore = _pm.currentHero.currHealth;
                        _pm.currentHero.currHealth = Mathf.Min(_pm.currentHero.currHealth + (int)pu.content, _pm.currentHero.maxHealth);

                        GameObject newTempNum = Instantiate(TEMP_NUM, transform.Find("Canvas/TemporaryNumbers"));
                        newTempNum.GetComponent<TempNumberManager>().Init("+" + (_pm.currentHero.currHealth - healthBefore).ToString(), Color.green, 2.0f, true);

                        Destroy(col.gameObject);
                    }
                }
                else if (pu.type.Equals("weapon") && !_ch.isPlayerAttacking) {
                    Weapon targetWep = (Weapon)pu.content;
                    //Debug.Log(targetWep);
                    if (ValidWeapons[_pm.currentHero.hero.rpgClass].Contains(targetWep.wClass)) { // if weapon is a valid weapon, pick it up & replace
                        Weapon oldWep = _pm.currentHero.mainWep; // save old wep
                        _pm.currentHero.mainWep = targetWep; // replace old wep
                        Destroy(col.gameObject); // take the new wep off the ground
                        /*
                        Object pickupPrefabObject = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Pickups/Pickup (" + oldWep.wClass.ToString() + ").prefab",
                            typeof(GameObject));
                        */
                        Object pickupPrefabObject = Resources.Load("Pickups/Specifics/Pickup (" + oldWep.wClass.ToString() + ")");
                        GameObject pickupPrefab = (GameObject)Instantiate(pickupPrefabObject, transform.position, Quaternion.identity);
                        // find room to drop weapon in
                        foreach (Transform t in GameObject.Find("/Environment/Terrain").transform) {
                            if (t.CompareTag("Room")) {
                                pickupPrefab.transform.parent = t.Find("Pickups");
                                break;
                            }
                        }
                        PickupInstance oldPu = pickupPrefab.GetComponent<PickupInstance>();
                        oldPu.canPickup = null;
                        oldPu.Init(oldWep.baseDmgMod, oldWep.primary.ToString(), oldWep.secondary.ToString(), oldWep.primaryCooldown, oldWep.secondaryCooldown);
                        
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D col) {
        if (col.CompareTag("Pickup")) {
            PickupInstance _pu = col.GetComponent<PickupInstance>();
            if (_pu.canPickup == null) _pu.canPickup = true;
        }
    }
}
