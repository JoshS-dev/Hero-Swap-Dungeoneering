using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HSD_Utils;

[CreateAssetMenu(fileName = "New Enemy Attack", menuName = "Enemy Attack")]
public class EnemyAttack : ScriptableObject {
    [SerializeField]
    public Attack attack;
    public EnemyAttackRequirement requirement;
    public float requirementParam;
    [Range(0f,1f)]
    public float hesitance;
}
    
