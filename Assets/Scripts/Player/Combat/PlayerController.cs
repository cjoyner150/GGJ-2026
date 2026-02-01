using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamageable
{
    [HideInInspector] public PlayerContext ctx;

    [SerializeField] IntEventSO deathEvent;
    [SerializeField] Transform modelTransform;
    [SerializeField] Transform attackLocation;

    public int playerIndex;
    private Rigidbody rb;

    float jumpCD = .2f;
    float jumpTimer = 0;
    float dashTimer = 0;
    float dashCDTimer = 0;
    float attackTimer = 0;
    float attackCDTimer = 0;
    float knockbackTimer = 0;
    float invulnerableTimer = 0;

    bool isJumping = false;
    bool isDashing = false;
    bool isAttacking = false;
    bool isTakingKnockback = false;

    bool dashOnCD = false;
    bool attackOnCD = false;

    float moveSpeed;
    float desiredMoveSpeed;

    int extraJumps;

    [Header("Feedbacks")]
    public MMF_Player JumpFeedback;
    public MMF_Player LandingFeedback;
    public MMF_Player DamageFeedback;
    public MMF_Player DashFeedback;
    public MMF_Player SlashFeedbackH;
    public MMF_Player SlashFeedbackV;
    public MMF_Player ParryFeedback;


    public enum MoveState
    {
        Idle,
        Walk,
        Air,
        Jump,
        Dash,
        Attack,
        Knockback
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

        if (ctx.grounded) extraJumps = ctx.jumps - 1;

        HandleInput();
        HandleCooldowns();
        HandleState();
        HandleRotation();
        HandleAnimations();
        LimitPlayerSpeed();

        rb.linearDamping = ctx.grounded ? ctx.groundDrag : 0;

        if (moveSpeed > desiredMoveSpeed)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, desiredMoveSpeed, Time.deltaTime * 4);
        }
        else moveSpeed = desiredMoveSpeed;
    }

    void HandleState()
    {
        if (!ctx.grounded && !isJumping && !isAttacking && !isDashing)
        {
            if (currentState != MoveState.Air) EnterState(MoveState.Air);

            desiredMoveSpeed = ctx.airMoveSpeed;
        }
        else if (isTakingKnockback)
        {
            if (currentState != MoveState.Knockback) EnterState(MoveState.Knockback);
        }
        else if (isDashing)
        {
            if (currentState != MoveState.Dash) EnterState(MoveState.Dash);

            desiredMoveSpeed = ctx.dashSpeed;
        }
        else if (isJumping)
        {
            if (currentState != MoveState.Jump) EnterState(MoveState.Jump);
        }
        else if (isAttacking)
        {
            if (currentState != MoveState.Attack) EnterState(MoveState.Attack);

            desiredMoveSpeed = ctx.attackMoveSpeed;

            RaycastHit[] hits = Physics.SphereCastAll(attackLocation.position, .75f, modelTransform.forward, 1f);

            foreach (RaycastHit hit in hits) {
                Collider col = hit.collider;

                if (col == null) continue;

                IDamageable damageable = col.GetComponent<IDamageable>();

                if (damageable != null && (object)damageable != this) 
                { 
                    damageable.Hit(ctx.attackDamage, out IDamageable.HitCallbackContext callbackContext, modelTransform.position); 
                    
                    switch (callbackContext)
                    {
                        case IDamageable.HitCallbackContext.success:
                            print("success");
                            break;
                        case IDamageable.HitCallbackContext.parried:
                            if (!isTakingKnockback)
                            {
                                TakeKnockback(20, .5f, ((PlayerController)damageable).modelTransform.position);
                                ParryFeedback?.PlayFeedbacks();
                            }
                            print("parried");
                            break;
                        case IDamageable.HitCallbackContext.invulnerable:
                            print("invulnerable");
                            break;
                    }
                }
            }
        }
        else if (ctx.moveDirection.magnitude > 0)
        {
            if (currentState != MoveState.Walk) EnterState(MoveState.Walk);

            desiredMoveSpeed = ctx.walkMoveSpeed;
        }
        else
        {
            if (currentState != MoveState.Idle) EnterState(MoveState.Idle);
        }
    }

    void HandleInput()
    {
        if ( isJumping 
            || isDashing 
            || isAttacking
            || isTakingKnockback) return;

        if (ctx.jumpHasBeenPressed && (ctx.grounded || extraJumps > 0))
        {
            if (!ctx.grounded) extraJumps--;

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

    void HandleAnimations()
    {
        ctx.anim.SetBool("Grounded", ctx.grounded);
        ctx.anim.SetBool("Dashing", isDashing);
        ctx.anim.SetBool("Attacking", isAttacking);
        ctx.anim.SetBool("Jumping", isJumping);
    }

    void LimitPlayerSpeed()
    {
        switch (currentState)
        {
            case MoveState.Walk:

                if (rb.linearVelocity.magnitude > moveSpeed)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
                }

                break;

            case MoveState.Air:

                Vector3 vel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

                if (vel.magnitude > moveSpeed)
                {
                    vel = vel.normalized * moveSpeed;

                    rb.linearVelocity = new Vector3(vel.x, rb.linearVelocity.y, vel.z);
                }

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

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0)
            {
                isTakingKnockback = false;
            }
        }

        if (invulnerableTimer > 0)
        {
            invulnerableTimer -= Time.deltaTime;

            if (invulnerableTimer <= 0)
            {
                ctx.invulnerable = false;
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
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.playJump(transform.position);
        JumpFeedback?.PlayFeedbacks();
    }

    void Dash()
    {
        isDashing = true;
        dashOnCD = true;
        dashTimer = ctx.dashLength;
        dashCDTimer = ctx.dashCD;
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.playDash(transform.position);
        DashFeedback?.PlayFeedbacks();
    }

    void Attack()
    {
        ctx.anim.SetFloat("Attack Speed", ctx.attackSpeed);

        isAttacking = true;
        attackOnCD = true;
        attackTimer = ctx.attackLength / ctx.attackSpeed;
        attackCDTimer = ctx.attackCD / ctx.attackSpeed;
        

        if (ctx.grounded)
        {
            SlashFeedbackH?.PlayFeedbacks();
        }
        else
        {
            SlashFeedbackV?.PlayFeedbacks();
        }
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.playAttack(transform.position);
    }

    void TakeKnockback(float speed, float length, Vector3 from)
    {
        isTakingKnockback = true;
        knockbackTimer = length;

        rb.linearVelocity = (new Vector3(modelTransform.position.x, 0, modelTransform.position.z) - new Vector3(from.x, 0, from.z)).normalized * speed;
    }

    public void Hit(float damage, out IDamageable.HitCallbackContext callbackContext, Vector3 fromPosition)
    {
        if (ctx.invulnerable)
        {
            callbackContext = IDamageable.HitCallbackContext.invulnerable;
            return;
        }
        else
        {
            ctx.invulnerable = true;
            invulnerableTimer = ctx.invulnerableTime;
        }
        
        if (isAttacking)
        {
            callbackContext = IDamageable.HitCallbackContext.parried;
            if (!isTakingKnockback)
            {
                TakeKnockback(20, .5f, fromPosition);
                ParryFeedback?.PlayFeedbacks();
            }
            return;
        }

        callbackContext = IDamageable.HitCallbackContext.success;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.playPunch(transform.position);
            AudioManager.Instance.voiceFight(transform.position);
        }
        ctx.currentHealth -= damage;

        if (ctx.currentHealth <= 0)
        {
            Die();
            return;
        }

        TakeKnockback(30, .5f, fromPosition);
        DamageFeedback?.PlayFeedbacks();
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
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(attackLocation.position, .75f);
    }

    private void Die()
    {
        deathEvent.RaiseEvent(playerIndex);

        Destroy(gameObject);
    }

}
