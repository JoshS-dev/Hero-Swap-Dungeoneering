using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using static HSD_Utils;

public class HitboxInstance : MonoBehaviour
{
    public int damage;

    public float knockback;
    public KnockbackStrength knockbackStrength;
    public bool envBreak;
    private GameObject centreMassPoint1, centreMassPoint2;
    
    TilemapCollider2D objectColliders, wallColliders;
    Collider2D selfCollider;

    private bool hitBadCollider;
    
    public int id;
    public string parent;
    // "Player", "Enemy", "None"
    // None hurts all

    public static int nextId;

    public void Init(int dmg, float knk, GameObject cm1, GameObject cm2, KnockbackStrength kbs, bool eb, string pr) {
        damage = dmg;
        knockback = knk;
        centreMassPoint1 = cm1;
        centreMassPoint2 = cm2;
        knockbackStrength = kbs;
        envBreak = eb;
        parent = pr;

        id = nextId;
        nextId++;
    }
    public void Init(int dmg, float knk, GameObject point, KnockbackStrength kbs, bool eb, string pr) {
        Init(dmg, knk, point, point, kbs, eb, pr);
    }

    public Vector3 KnockbackPoint() {
        if (GameObject.ReferenceEquals(centreMassPoint1, centreMassPoint2)) {
            return centreMassPoint1.transform.position;
        }
        return (centreMassPoint1.transform.position + centreMassPoint2.transform.position) / 2f;
    }

    // Awake is called when created
    private void Awake() {
        
        selfCollider = GetComponent<Collider2D>();
        
    }

    private void Update() {
        
        if (hitBadCollider == false && envBreak) {
            objectColliders = GameObject.Find("/Environment/Terrain").transform.GetChild(0).Find("Objects").gameObject.GetComponent<TilemapCollider2D>();
            //objectColliders = GameObject.Find("/Environment/Terrain/Objects").gameObject.GetComponent<TilemapCollider2D>();
            wallColliders = GameObject.Find("/Environment/Terrain").transform.GetChild(1).gameObject.GetComponent<TilemapCollider2D>();
            Collider2D[] results = new Collider2D[16];
            ContactFilter2D filter = new ContactFilter2D();
            filter.NoFilter();
            Physics2D.OverlapCollider(selfCollider, filter, results);
            foreach (Collider2D c in results) {
                if (c != null) {
                    if (c == objectColliders || c == wallColliders) {
                        hitBadCollider = true;
                        break;
                    }
                }
            }
            if (hitBadCollider) {
                Destroy(gameObject);
                /*
                damage = 0;
                knockback = 0;
                GetComponent<SpriteRenderer>().color = ChangeColorAlpha(GetComponent<SpriteRenderer>().color, 0.15f);
                */
            }
        }
        
    }
}
