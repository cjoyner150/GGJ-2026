using System;
using UnityEngine;

[Serializable]
public class PlayerContext
{
    [Header("Movement")]
    public float walkMoveSpeed;
    public float walkForce = 10f;
    public bool grounded;
    public float groundDrag;

    public float jumpMultiplier = 10f;
    public int jumps = 1;
    public LayerMask whatIsJumpableGround;

    public float airMoveSpeed;

    public float dashLength;
    public float dashSpeed;
    public float dashCD;

    public float attackLength;
    public float attackCD;
    public float attackMoveSpeed;
    public float attackDamage;
    public float attackSpeed;

    [Header("References")]
    public Animator anim;

    [Header("Input")]
    public Vector3 moveDirection;
    public bool jumpHasBeenPressed;
    public bool dashHasBeenPressed;
    public bool attackHasBeenPressed;

    [Header("Health")]
    public float maxHealth;
    public float currentHealth;
    public bool invulnerable;
    public float invulnerableTime;

}
