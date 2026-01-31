using System;
using UnityEngine;

[Serializable]
public class PlayerContext
{
    [Header("Movement")]
    public float moveSpeed;
    public float walkForce = 10f;
    public bool grounded;
    public float groundDrag;

    public float jumpMultiplier = 10f;
    public LayerMask whatIsJumpableGround;

    public float dashLength;
    public float dashSpeed;

    [Header("Input")]
    public Vector3 moveDirection;
    public bool jumpHasBeenPressed;
    public bool dashHasBeenPressed;
    public bool attackHasBeenPressed;


}
