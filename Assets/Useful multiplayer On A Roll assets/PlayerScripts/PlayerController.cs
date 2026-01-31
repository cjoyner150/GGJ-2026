using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerContext ctx;

    [SerializeField] IntEventSO deathEvent;

    public int playerIndex;
    private Rigidbody rb;

    private enum MoveState
    {
        Idle,
        Walk,
        Air,
        Dash,
        Attack
    }

    MoveState currentState = MoveState.Idle;

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

        HandleState();

    }

    void HandleState()
    {
        if (!ctx.grounded)
        {
            EnterState(MoveState.Air);
        }
        else if (ctx.moveDirection.magnitude > 0)
        {
            EnterState(MoveState.Walk);
        }
        else
        {
            EnterState(MoveState.Idle);
        }
    }

    void MovePlayer()
    {
        switch (currentState)
        {
            case MoveState.Walk:

                rb.AddForce(ctx.acceleration * Time.fixedDeltaTime * ctx.moveDirection);
                break;

            case MoveState.Idle:

                break;

            default:

                break;

        }
    }

    void EnterState(MoveState moveState)
    {
        currentState = moveState;

        switch (currentState)
        {
            case MoveState.Idle:
                break;
        }
    }

    bool CheckGrounded()
    {
        Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, .2f, ctx.whatIsJumpableGround);

        return (hit.collider != null);
    }

    private void Die()
    {
        deathEvent.RaiseEvent(playerIndex);

        Destroy(gameObject);
    }

}
