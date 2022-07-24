using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HSD_Utils;
using static GameStateManager;

public class PlayerMovement : MonoBehaviour
{
    Camera _mCam;
    PlayerStatus _ps;
    PartyManager _pm;
    SpriteRenderer _sr;

    private float BASEMOVESPEED = 5f;
    private float CAMERASPEED = 1.0f;

    readonly float CAMERAZ = -10f;

    public bool isRolling;
    public bool rollingOffCooldown;

    public bool ableToMove;
    public bool ableToRoll;

    // Start is called before the first frame update
    void Start()
    {
        _mCam = Camera.main;
        _ps = GetComponent<PlayerStatus>();
        _pm = GetComponent<PartyManager>();
        _sr = GetComponent<SpriteRenderer>();

        isRolling = false;
        rollingOffCooldown = true;
        ableToMove = true;
        ableToRoll = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if(moveVector != Vector2.zero && ableToMove && CurrGamestate == GameState.Running){
            float moveSpeed = BASEMOVESPEED;
            moveSpeed *= _pm.currentHero.AgilityToSpeedScalar();

            if (!isRolling) { Move(moveVector, moveSpeed); }

            if (Input.GetKeyDown(KeyCode.Space) && rollingOffCooldown && ableToRoll) {
                if (_pm.currentHero.hero.agility <= _pm.currentHero.BASESPEEDTHRESHOLD)
                    StartCoroutine(Roll(moveVector, moveSpeed, 0.3f, 1.5f, 2.5f));
                else
                    StartCoroutine(Roll(moveVector, moveSpeed, 0.3f, 1.5f, 1.5f, 1f));
            }
        }
        CameraFollow(2);
    }

    void Move(Vector2 direction, float speed) {
        direction.Normalize();
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    IEnumerator Roll(Vector2 direction, float speed, float duration, float cooldown, float rollSpeedScalar, float rollSpeedFlat = 0) {
        isRolling = true;
        rollingOffCooldown = false;
        _sr.color = ChangeColorAlpha(_sr.color, 0.5f);
        StartCoroutine(_ps.ApplyInvulerability(duration + 0.1f));
        while (true) {
            if (duration <= 0) { break; }

            Move(direction, speed * rollSpeedScalar + rollSpeedFlat);
            duration -= Time.deltaTime;
            yield return null;
        }
        isRolling = false;
        _sr.color = ChangeColorAlpha(_sr.color, 1f);
        yield return new WaitForSeconds(cooldown);
        rollingOffCooldown = true;
    }

    /* focusType 0: player
                 1: room centre
                 2: player&room centre midpoint
    */
    void CameraFollow(int focusType, bool smooth = true) {
        Vector3 playerPoint = new Vector3(transform.position.x, transform.position.y, CAMERAZ);
        Vector3 roomCentre = new Vector3(5.5f, -4.5f, CAMERAZ);
        Vector3 focusPoint;
        switch (focusType) {
            case 0: focusPoint = playerPoint; break;
            case 1: focusPoint = roomCentre; break;
            default: focusPoint = (playerPoint + roomCentre) / 2f; break;
        }

        if (smooth)
            _mCam.transform.position = Vector3.Lerp(_mCam.transform.position,
                                                    focusPoint,
                                                    Time.deltaTime * CAMERASPEED);
        else
            _mCam.transform.position = focusPoint;
    }
}
