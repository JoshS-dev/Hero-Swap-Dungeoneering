using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HSD_Utils;
using static GameStateManager;

public class PlayerStatus : MonoBehaviour
{
    PartyManager _pm;

    private int invulnInstances;
    /*
    string lasthitSource;

    readonly float KNOCKBACK_DURATION = 0.25f;
    
    [SerializeField]
    float currKnockbackTimer;
    Vector3 currKnockbackVector;

    readonly float MOVEINT_DURATION = 1f;
    readonly float ATKINT_DURATION = 1f;
    */
    private List<int> prevHitboxes = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        _pm = GetComponent<PartyManager>();
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.CompareTag("Hitbox")) {
            HitboxInstance hitbox = col.gameObject.GetComponent<HitboxInstance>();
            if (hitbox.parent != "Player" && !prevHitboxes.Contains(hitbox.id)) {
                prevHitboxes.Add(hitbox.id);

                TakeDamage(hitbox.damage, hitbox.parent);
                /*
                lasthitSource = hitbox.parent;
                
                currKnockbackTimer = KNOCKBACK_DURATION;
                currKnockbackVector = (transform.position - hitbox.KnockbackPoint()).normalized * hitbox.knockback;
                
                if (hitbox.knockbackStrength == KnockbackStrength.InterruptMovement || hitbox.knockbackStrength == KnockbackStrength.InterruptBoth) {
                    moveInterruptTimer = MOVEINT_DURATION;
                }
                if (hitbox.knockbackStrength == KnockbackStrength.InterruptAttack || hitbox.knockbackStrength == KnockbackStrength.InterruptBoth) {
                    attackInterruptTimer = ATKINT_DURATION;
                    if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);
                    canMove = true;
                }
                */
            }
        }
    }

    public void TakeDamage(int damage, string source) {
        if(invulnInstances == 0) {
            _pm.currentHero.currHealth -= damage;
            if(_pm.currentHero.currHealth <= 0) {
                Kill();
            }
        }
    }

    public void Kill() {
        ChangeState(GameState.Dead);
    }

    public IEnumerator ApplyInvulerability(float duration) {
        invulnInstances++;
        yield return new WaitForSeconds(duration);
        invulnInstances--;
    }
}
