using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

using static HSD_Utils;
using static GameStateManager;


public class EnemyManager : MonoBehaviour
{
    // MIN XY, MAX XY
    readonly Vector2[] ROOM_BOUNDS = new Vector2[2] { new Vector2(0.5f, 0.5f), new Vector2(10.5f, -9.5f) };
    
    [SerializeField]
    public string enemyName;
    
    readonly float KNOCKBACK_DURATION = 0.25f;
    float currKnockbackTimer;
    Vector3 currKnockbackVector;
    
    readonly float MOVEINT_DURATION = 1f;
    readonly float ATKINT_DURATION = 1f;
    public float moveInterruptTimer, attackInterruptTimer;

    string lasthitSource;

    [SerializeField]
    GameObject TEMP_NUM;
    GameObject player;

    CombatHandler _ch;
    RunManager _rm;

    [SerializeField]
    GameObject indicator;

    private Coroutine currentAttackCoroutine;
    public bool canAttack;
    private int noAttackInstances;
    public bool canMove;

    public bool isBoss;

    public int attackMod;
    public int maxHealth;
    public int currHealth;

    private List<int> prevHitboxes = new List<int>();

    Image healthBarInner;

    NavMeshAgent _agent;
    [Header("AI")]
    public EnemyAttack currAtk;
    [SerializeField] EnemyAttack[] attacks;
    [SerializeField] float attackChangeTime;
    private float nextAttackChange;
    [SerializeField] EnemyMovementType tempMove;
    [SerializeField] float pathUpdateRate;
    [SerializeField] float moveSpeed;
    float nextMoveTime;
    [SerializeField] float moveParam;

    public void Init(int mH) {
        maxHealth = currHealth = mH;
    }

    void Awake() {
        //currKnockbackTimer = 0;
        Init(maxHealth);
        player = GameObject.Find("/Player");
        healthBarInner = transform.Find("Canvas/Health Bar/Inner").gameObject.GetComponent<Image>();
        transform.Find("Canvas/Name").gameObject.GetComponent<TextMeshProUGUI>().text = enemyName;

        canAttack = canMove = true;

        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _ch = GameObject.Find("/GameControl").GetComponent<CombatHandler>();
        _rm = GameObject.Find("/GameControl").GetComponent<RunManager>();

        nextAttackChange += attackChangeTime;
        if (attacks.Length == 1) currAtk = attacks[0];
        else currAtk = randFromList(attacks, _rm.RUN_SEED);
    }

    // Update is called once per frame
    void Update() {
        if (currHealth <= 0) {
            Kill(lasthitSource);
        }

        canAttack = (noAttackInstances > 0) ? false : true;

        if (currKnockbackTimer > 0) {
            Vector3 distanceToMove;

            float toTravelRatio = Time.deltaTime / currKnockbackTimer;
            distanceToMove = currKnockbackVector * toTravelRatio;
            currKnockbackVector -= distanceToMove;

            transform.position += distanceToMove;

            currKnockbackTimer -= Time.deltaTime;
        }
        //else if (currKnockbackTimer <= 0) currKnockbackTimer = 0;

        if (moveInterruptTimer > 0) {
            moveInterruptTimer -= Time.deltaTime;
        }

        if (attackInterruptTimer > 0) {
            attackInterruptTimer -= Time.deltaTime;
        }

        // NAV AGENT WORK
        // MOVE
        if (moveInterruptTimer <= 0 && canMove && CurrGamestate == GameState.Running) {
            if (Time.time > nextMoveTime) {
                nextMoveTime += pathUpdateRate;
                _agent.speed = moveSpeed;
                switch (tempMove) {
                    case EnemyMovementType.Chase:
                        _agent.SetDestination(player.transform.position);
                        break;
                    case EnemyMovementType.Scutter:
                        Vector3 nearPlayerPosition = NearPositionInBounds(1.5f, 1.5f, player.transform.position);
                        _agent.SetDestination(transform.position + ((nearPlayerPosition - transform.position).normalized * moveParam));
                        break;
                    case EnemyMovementType.Wander:
                        _agent.SetDestination(NearPositionInBounds(moveParam / 2, moveParam / 2, transform.position));
                        break;
                    case EnemyMovementType.Retreat: // if cornered or far enough away from the player, wander
                        Vector3 enemyToPlayer = player.transform.position - transform.position;
                        if (enemyToPlayer.magnitude < moveParam + 0.5f) {
                            Vector3 directlyAway = transform.position + (((moveParam + 1) - enemyToPlayer.magnitude) * (transform.position - player.transform.position).normalized);
                            if (directlyAway.x >= ROOM_BOUNDS[0].x && directlyAway.x <= ROOM_BOUNDS[1].x &&
                               directlyAway.y >= ROOM_BOUNDS[1].y && directlyAway.y <= ROOM_BOUNDS[0].y) {
                                _agent.SetDestination(directlyAway);
                            }
                            else {
                                _agent.SetDestination(NearPositionInBounds(moveParam / 2, moveParam / 2, transform.position));
                            }
                        }
                        else _agent.SetDestination(NearPositionInBounds(moveParam / 3, moveParam / 3, transform.position));
                        break;
                    case EnemyMovementType.Stationary:
                        _agent.ResetPath();
                        break;
                }
            }
        }
        else _agent.ResetPath();

        // ATTACK
        if (Time.time > nextAttackChange) {
            nextAttackChange += attackChangeTime;
            if (attacks.Length == 1) currAtk = attacks[0];
            else currAtk = randFromList(attacks, _rm.RUN_SEED);
        }
        //Debug.Log(attackInterruptTimer);
        if (attackInterruptTimer <= 0 && (canAttack == true) && CurrGamestate == GameState.Running) {
            if (SeedIndependantRange() > currAtk.hesitance) {
                if (currAtk.requirement == EnemyAttackRequirement.DistanceFrom
                &&
                Vector3.Distance(transform.position, player.transform.position) <=
                currAtk.requirementParam + player.transform.localScale.x / 2f + transform.localScale.x / 2f) {
                    DoAttack(currAtk.attack);
                }
                else if (currAtk.requirement == EnemyAttackRequirement.VisibleTo) {
                    Vector3 lineToPlayer = player.transform.position - transform.position;
                    float distanceCheck;
                    if (currAtk.requirementParam > 0) distanceCheck = currAtk.requirementParam;
                    else distanceCheck = lineToPlayer.magnitude - 1f;

                    RaycastHit2D hit = Physics2D.Raycast(transform.position + ((transform.localScale.x + 0.1f) * lineToPlayer.normalized), lineToPlayer, distanceCheck);
                    if (hit.collider != null && hit.collider.gameObject == player) {
                        DoAttack(currAtk.attack);
                    }
                }
                else if (currAtk.requirement == EnemyAttackRequirement.None) {
                    DoAttack(currAtk.attack);
                }
            }
            else StartCoroutine(CanAttackCooldown(currAtk.attack.cooldown / 2f));
        }
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.CompareTag("Hitbox")) {
            HitboxInstance hitbox = col.gameObject.GetComponent<HitboxInstance>();
            if (hitbox.parent != "Enemy" && !prevHitboxes.Contains(hitbox.id)) {
                prevHitboxes.Add(hitbox.id);

                TakeDamage(hitbox.damage, hitbox.parent);

                lasthitSource = hitbox.parent;

                currKnockbackTimer = KNOCKBACK_DURATION;
                float knockbackScalar = hitbox.knockback;
                if (isBoss) knockbackScalar *= 0.5f;
                currKnockbackVector = (transform.position - hitbox.KnockbackPoint()).normalized * knockbackScalar;

                if (hitbox.knockbackStrength == KnockbackStrength.InterruptMovement || hitbox.knockbackStrength == KnockbackStrength.InterruptBoth) {
                    if (isBoss) moveInterruptTimer = MOVEINT_DURATION / 2f;
                    else moveInterruptTimer = MOVEINT_DURATION;
                }
                if (hitbox.knockbackStrength == KnockbackStrength.InterruptAttack || hitbox.knockbackStrength == KnockbackStrength.InterruptBoth) {
                    if (isBoss) attackInterruptTimer = ATKINT_DURATION / 2f;
                    else attackInterruptTimer = ATKINT_DURATION;

                    if(currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);
                    canMove = true;
                }
            }
        }
    }

    public void TakeDamage(int dmg, string source) {
        if (source != "Enemy") {
            currHealth -= dmg;
            healthBarInner.fillAmount = Mathf.Clamp01(1.0f * currHealth / maxHealth);

            GameObject newTempNum = Instantiate(TEMP_NUM, transform.Find("Canvas/TemporaryNumbers"));
            //newTempNum.transform.SetAsFirstSibling();

            newTempNum.GetComponent<TempNumberManager>().Init(dmg.ToString(), Color.red, 2.0f, true);
        }
    }

    public void Kill(string source) {
        Destroy(gameObject, 0.3f);
    }

    private void DoAttack(Attack atk) {
        currentAttackCoroutine = StartCoroutine(_ch.EnemyAttackBootstrap(gameObject, atk, attackMod));
        StartCoroutine(CanAttackCooldown(atk.GetTotalDuration() + atk.cooldown));
        StartCoroutine(AttackIndicator(atk.GetTotalDuration()));
    }

    private IEnumerator AttackIndicator(float duration) {
        Transform ind = Instantiate(indicator).transform;
        ind.parent = transform;
        ind.localScale = Vector3.one * 0.5f;
        ind.localPosition = Vector3.zero;
        yield return new WaitForSeconds(duration);
        Destroy(ind.gameObject);
    }

    private IEnumerator CanAttackCooldown(float duration) {
        //canAttack = false;
        noAttackInstances++;
        yield return new WaitForSeconds(duration);
        //canAttack = true;
        noAttackInstances--;
    }

    private Vector3 NearPositionInBounds(float xVar, float yVar, Vector3? pos = null) {
        Vector3 originPos;
        if (pos == null) originPos = transform.position;
        else originPos = (Vector3)pos;
        Vector3 returnVector = Vector3.zero;
        returnVector.x = Mathf.Clamp(SeedIndependantRange(originPos.x - xVar, originPos.x + xVar),
            ROOM_BOUNDS[0].x, ROOM_BOUNDS[1].x);
        
        returnVector.y = Mathf.Clamp(SeedIndependantRange(originPos.y + yVar, originPos.y - yVar),
            ROOM_BOUNDS[1].y, ROOM_BOUNDS[0].y);
        return returnVector;
    }
}
