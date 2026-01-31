using System;
using UnityEngine;

[Serializable]
public class PlayerContext
{
    [Header("Movement")]
    public float moveSpeed;
    public float acceleration = 10f;
    public bool grounded;

    public float jumpMultiplier = 10f;
    public LayerMask whatIsJumpableGround;

    [Header("Input")]
    public Vector3 moveDirection;
    public bool jumpHasBeenPressed;
    public bool dashHasBeenPressed;
    public bool attackHasBeenPressed;


}
