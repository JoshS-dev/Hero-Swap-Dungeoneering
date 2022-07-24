using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HSD_Utils;
using static GameStateManager;

public class CombatHandler : MonoBehaviour
{
    [Header("Hitboxes")]
    [SerializeField]
    GameObject h_tri;
    [SerializeField]
    GameObject h_circ, h_quad, h_hex;

    Camera _mCam;

    PlayerMovement _pm;
    PartyManager _pr;
    
    public bool isPlayerAttacking;
    private GameObject player;

    private List<Vector3> prevDirections = new List<Vector3>();
    private List<AttackPosition> prevPositions = new List<AttackPosition>();
    private List<Quaternion> prevRotations = new List<Quaternion>();

    private void Start() {
        _mCam = Camera.main;

        isPlayerAttacking = false;
        player = GameObject.Find("/Player");

        _pm = player.GetComponent<PlayerMovement>();
        _pr = player.GetComponent<PartyManager>();
    }
    /*
    private void Update() {
        if(CurrGamestate == GameState.Dead) {
            StopAllCoroutines();
        }
    }
    */
    public IEnumerator PlayerAttackBootstrap(Attack attack, Weapon wep, PartyMember member, bool ignoreIsAttacking = false) {
        isPlayerAttacking = true;
        //Debug.Log("ATTACK START");
        prevDirections.Add(Vector3.zero);
        prevPositions.Add(AttackPosition.None);
        prevRotations.Add(Quaternion.identity);
        int id = prevDirections.Count - 1;
        for (int i = 0; i < attack.hitboxes.Length; i++) {
            AttackSequence currentSeq = new AttackSequence(attack, i);
            int damage = DmgFromSeq(currentSeq, wep, member);

            yield return new WaitForSeconds(currentSeq.windup);
            //Debug.Log("WINDUP DONE");

            if (currentSeq.moveContraint == PlayerMoveContraints.CantRoll || currentSeq.moveContraint == PlayerMoveContraints.CantEither) _pm.ableToRoll = false;
            if (currentSeq.moveContraint == PlayerMoveContraints.CantMove || currentSeq.moveContraint == PlayerMoveContraints.CantEither) _pm.ableToMove = false;

            StartCoroutine(HitboxManager(player, id, "Player", currentSeq, currentSeq.dmgDuration, damage));
            //yield return new WaitForSeconds(currentSeq.dmgDuration);
            //Debug.Log("DMG DONE");
            //Debug.Log("Dmg dealt: " + damage);
        }
        isPlayerAttacking = false;
        //Debug.Log("ATTACK END");
    }

    public IEnumerator EnemyAttackBootstrap(GameObject enemy, Attack attack, int attackMod) {
        EnemyManager _em = enemy.GetComponent<EnemyManager>();
        prevDirections.Add(Vector3.zero);
        prevPositions.Add(AttackPosition.None);
        prevRotations.Add(Quaternion.identity);
        int id = prevDirections.Count - 1;
        for (int i = 0; i < attack.hitboxes.Length; i++) {
            AttackSequence currentSeq = new AttackSequence(attack, i);
            int damage = Mathf.Max(0,DmgFromSeq(currentSeq) + attackMod);

            if (currentSeq.moveContraint == PlayerMoveContraints.CantMove || currentSeq.moveContraint == PlayerMoveContraints.CantEither) _em.canMove = false;

            yield return new WaitForSeconds(currentSeq.windup);

            StartCoroutine(HitboxManager(enemy, id, "Enemy", currentSeq, currentSeq.dmgDuration, damage, player.transform.position));
        }
    }

    private IEnumerator HitboxManager(GameObject attacker, int id, string source, AttackSequence seq, 
                                      float duration, int damage, 
                                      Vector3? forcedMousePosition = null) {
        GameObject hitboxType;
        switch (seq.hitbox) {
            case AttackHitbox.Triangle:
                hitboxType = h_tri;
                break;
            case AttackHitbox.Circle:
                hitboxType = h_circ;
                break;
            case AttackHitbox.Quad:
                hitboxType = h_quad;
                break;
            default: // case AttackHitbox.Hex:
                hitboxType = h_hex;
                break;
        }
        GameObject hitbox = Instantiate(hitboxType);
        hitbox.transform.localScale = seq.hitboxScale;

        if(seq.knockbackOrigin == KnockbackOrigin.Player || seq.knockbackOrigin == KnockbackOrigin.Hitbox) { // single point origins
            GameObject singlePoint;
            if (seq.knockbackOrigin == KnockbackOrigin.Player) singlePoint = attacker;
            else singlePoint = hitbox; // seq.knockbackOrigin == KnockbackOrigin.Hitbox
            hitbox.GetComponent<HitboxInstance>().Init(damage, seq.knockback, singlePoint, seq.knockbackStrength, seq.breakOnEnv, source);
        }
        else { // two point origins
            GameObject firstPoint, secondPoint;
            // midpoint between player and hitbox
            firstPoint = attacker;
            secondPoint = hitbox;
            hitbox.GetComponent<HitboxInstance>().Init(damage, seq.knockback, firstPoint, secondPoint, seq.knockbackStrength, seq.breakOnEnv, source);
        }

        Vector3 mousePosition;
        if (forcedMousePosition != null) mousePosition = (Vector3)forcedMousePosition;
        else mousePosition = GetMousePoint();

        Vector3 targetPosition;
        Quaternion targetRotation;

        Vector3 lineToMouse = (mousePosition - attacker.transform.position).normalized;

        // SET UP ROTATION //
        Vector3 mouseDirectionVector = (mousePosition - attacker.transform.position).normalized;
        float directionRotation = Mathf.Atan2(mouseDirectionVector.y, mouseDirectionVector.x) * Mathf.Rad2Deg;
        //targetRotation = Quaternion.AngleAxis(directionRotation, Vector3.forward);
        targetRotation = Quaternion.Euler(0, 0, directionRotation);
        if (seq.hitbox == AttackHitbox.Triangle) {
            targetRotation *= Quaternion.Euler(0f, 0f, 90f);
        }
        if(seq.attackPosit != AttackPosition.Previous) {
            prevRotations[id] = targetRotation;
        }
        if (seq.hitbox == AttackHitbox.Triangle && seq.attackPosit == AttackPosition.Projectile) {
            /*
            Vector3 tempFlip = targetRotation.eulerAngles;
            tempFlip = new Vector3(tempFlip.x, tempFlip.y + 180, tempFlip.z);
            targetRotation = Quaternion.Euler(tempFlip);
            */
            targetRotation *= Quaternion.Euler(0f, 0f, 180f);
        }

        // SET UP MOVEMENT // ~ in direction of lineToMouse
        Vector3 forceMove = Vector3.zero;
        if (seq.playerMovement.magnitude != 0) {
            //_pm.ableToMove = false;
            forceMove = seq.playerMovement.x * lineToMouse.normalized;
            if(seq.playerMovement.y != 0) {
                forceMove += seq.playerMovement.y * (Vector3)Vector2.Perpendicular(lineToMouse).normalized;
            }
        }

        float scaleScalar;
        float attackerHalfScale = attacker.transform.localScale.x / 2f;
        if (seq.hitbox == AttackHitbox.Triangle) scaleScalar = seq.hitboxScale.y;
        else scaleScalar = seq.hitboxScale.x;

        if (seq.attackPosit == AttackPosition.Projectile) {
            hitbox.transform.position = attacker.transform.position + ((attackerHalfScale + scaleScalar / 2f) * lineToMouse);
        }

        Vector3? constantPos = null;

        while (duration > 0 && CurrGamestate != GameState.Dead) {
            mousePosition = GetMousePoint();
            if (attacker == null && seq.attackPosit != AttackPosition.Projectile) break;
            // SET UP DIRECTION // ~ attack position dependant
            switch (seq.attackPosit) {
                case AttackPosition.InfrontPlayer:
                    targetPosition = attacker.transform.position + (((attackerHalfScale + scaleScalar / 2f) + seq.positParam) * lineToMouse);
                    prevDirections[id] = lineToMouse;
                    break;
                case AttackPosition.OnPlayer:
                    targetPosition = attacker.transform.position;
                    break;
                case AttackPosition.OnMouse:
                    if (constantPos == null) {
                        if ((Vector3.Distance(attacker.transform.position, mousePosition) > seq.positParam) && seq.positParam != 0) {
                            constantPos = attacker.transform.position + (seq.positParam * lineToMouse);
                        }
                        else {
                            constantPos = mousePosition;
                        }
                    }
                    targetPosition = (Vector3)constantPos;
                    prevDirections[id] = (Vector3)constantPos;
                    break;
                case AttackPosition.Projectile:
                    //ProjectileManager(seq, hitbox, lineToMouse, seq.dmgDuration, seq.positParam);
                    if (hitbox != null) {
                        targetPosition = hitbox.transform.position + ((seq.positParam) * (attackerHalfScale + scaleScalar / 2f) * lineToMouse) * Time.deltaTime;
                        prevDirections[id] = hitbox.transform.position;
                    }
                    else {
                        targetPosition = player.transform.position;
                        prevDirections[id] = player.transform.position;
                    }
                    break;
                default: // case AttackPosition.Previous
                    if(prevPositions[id] == AttackPosition.InfrontPlayer) {
                        targetPosition = attacker.transform.position + (((attackerHalfScale + scaleScalar / 2f) + seq.positParam) * prevDirections[id]);
                    }
                    else if(prevPositions[id] == AttackPosition.OnPlayer) { // very hacky for Hammer/Sec/Explosive Strike, not planning on using onPlayer w/ prev otherwise
                        targetPosition = attacker.transform.position + (((attackerHalfScale + scaleScalar / 2f) + seq.positParam) * prevDirections[id]);
                    }
                    else {
                        targetPosition = prevDirections[id];
                    }
                    targetRotation = prevRotations[id];
                    break;
            }
            if (seq.attackPosit != AttackPosition.Previous) prevPositions[id] = seq.attackPosit;

            if(seq.playerMovement.magnitude != 0 && attacker != null) {
                Vector3 distanceToMove;
                float toTravelRatio = Time.deltaTime / duration;

                distanceToMove = toTravelRatio * forceMove;
                forceMove -= distanceToMove;

                attacker.transform.position += distanceToMove;
            }
            if (hitbox != null) {
                hitbox.transform.position = targetPosition;
                hitbox.transform.rotation = targetRotation;
            }

            duration -= Time.deltaTime;
            yield return null;
        }
        if (source.Equals("Player") && attacker != null) {
            if (seq.moveContraint == PlayerMoveContraints.CantRoll || seq.moveContraint == PlayerMoveContraints.CantEither) _pm.ableToRoll = true;
            if (seq.moveContraint == PlayerMoveContraints.CantMove || seq.moveContraint == PlayerMoveContraints.CantEither) _pm.ableToMove = true;
        }
        else if (source.Equals("Enemy") && attacker != null) {
            attacker.GetComponent<EnemyManager>().canMove = true;
        }

        if (hitbox != null) Destroy(hitbox);
    }

    private Vector3 GetMousePoint() {
        Vector3 returnVec = _mCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        returnVec.z = 0;
        return returnVec;
    }
}
