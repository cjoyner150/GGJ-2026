using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public PlayerContext ctx;

    [SerializeField] IntEventSO deathEvent;
    [SerializeField] Transform modelTransform;

    public int playerIndex;
    private Rigidbody rb;

    float jumpCD = .2f;
    float jumpTimer = 0;
    float dashTimer = 0;
    float dashCDTimer = 0;
    float attackTimer = 0;
    float attackCDTimer = 0;

    bool isJumping = false;
    bool isDashing = false;
    bool isAttacking = false;

    bool dashOnCD = false;
    bool attackOnCD = false;

    public enum MoveState
    {
        Idle,
        Walk,
        Air,
        Jump,
        Dash,
        Attack
    }

    public MoveState currentState = MoveState.Idle;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void Update()
    {
        ctx.grounded = CheckGrounded();

        HandleInput();
        HandleCooldowns();
        HandleState();
        HandleRotation();

        rb.linearDamping = ctx.grounded ? ctx.groundDrag : 0;
    }

    void HandleState()
    {
        if (!ctx.grounded && !isJumping)
        {
            if (currentState != MoveState.Air) EnterState(MoveState.Air);
        }
        else if (isDashing)
        {
            if (currentState != MoveState.Dash) EnterState(MoveState.Dash);
        }
        else if (isJumping)
        {
            if (currentState != MoveState.Jump) EnterState(MoveState.Jump);
        }
        else if (isAttacking)
        {
            if (currentState != MoveState.Attack) EnterState(MoveState.Attack);
        }
        else if (ctx.moveDirection.magnitude > 0)
        {
            if (currentState != MoveState.Walk) EnterState(MoveState.Walk);
        }
        else
        {
            if (currentState != MoveState.Idle) EnterState(MoveState.Idle);
        }
    }

    void HandleInput()
    {
        if (!ctx.grounded 
            || isJumping 
            || isDashing 
            || isAttacking) return;

        if (ctx.jumpHasBeenPressed)
        {
            Jump();
            EnterState(MoveState.Air);
        }
        else if (ctx.dashHasBeenPressed && !dashOnCD)
        {
            Dash();
            EnterState(MoveState.Dash);
        }
        else if (ctx.attackHasBeenPressed && !attackOnCD)
        {
            Attack();
            EnterState(MoveState.Attack);
        }

        ctx.jumpHasBeenPressed = false;
        ctx.dashHasBeenPressed = false;
        ctx.attackHasBeenPressed = false;
    }

    void MovePlayer()
    {
        switch (currentState)
        {
            case MoveState.Walk:

                rb.AddForce(ctx.walkForce * Time.fixedDeltaTime * ctx.moveDirection * 100, ForceMode.Force);
                break;

            case MoveState.Air:

                rb.AddForce(ctx.walkForce * Time.fixedDeltaTime * ctx.moveDirection * 40, ForceMode.Force);
                break;

            case MoveState.Dash:

                rb.linearVelocity = modelTransform.forward * ctx.dashSpeed;
                break;

            case MoveState.Attack:

                rb.linearVelocity = modelTransform.forward * ctx.attackMoveSpeed;
                break;

            default:

                break;

        }
    }

    void HandleCooldowns()
    {
        if (jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;

            if (jumpTimer <= 0)
            {
                ctx.jumpHasBeenPressed = false;
                isJumping = false;
            }
        }
        
        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
            {
                ctx.dashHasBeenPressed = false;
                isDashing = false;
            }
        }

        if (dashCDTimer > 0)
        {
            dashCDTimer -= Time.deltaTime;
            
            if (dashCDTimer <= 0)
            {
                dashOnCD = false;
            }
        }

        if (attackCDTimer > 0)
        {
            attackCDTimer -= Time.deltaTime;

            if (attackCDTimer <= 0)
            {
                attackOnCD = false;
            }
        }

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0)
            {
                ctx.attackHasBeenPressed = false;
                isAttacking = false;
            }
        }

    }

    void HandleRotation()
    {
        if (ctx.moveDirection.magnitude > 0)
        {
            modelTransform.forward = Vector3.Slerp(modelTransform.forward, ctx.moveDirection, Time.deltaTime * 10);
        }
    }

    void Jump()
    {
        jumpTimer = jumpCD;
        isJumping = true;
        rb.AddForce(ctx.jumpMultiplier * transform.up, ForceMode.Impulse);
        ctx.grounded = false;
    }

    void Dash()
    {
        isDashing = true;
        dashOnCD = true;
        dashTimer = ctx.dashLength;
        dashCDTimer = ctx.dashCD;
    }
    void Attack()
    {
        isAttacking = true;
        attackOnCD = true;
        attackTimer = ctx.attackLength;
        attackCDTimer = ctx.attackCD;
    }

    void EnterState(MoveState moveState)
    {
        currentState = moveState;
    }

    bool CheckGrounded()
    {
        Physics.Raycast(transform.position + (transform.up * .25f), -transform.up, out RaycastHit hit, .5f, ctx.whatIsJumpableGround);

        return (hit.collider != null);
    }

    private void OnDrawGizmosSelected()
    {
        Debug.DrawLine(transform.position + (transform.up * .25f), transform.position + (-transform.up * .5f), Color.green);
    }

    private void Die()
    {
        deathEvent.RaiseEvent(playerIndex);

        Destroy(gameObject);
    }

}
