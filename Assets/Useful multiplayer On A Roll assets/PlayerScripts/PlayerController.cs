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
        rb.AddForce(ctx.acceleration * Time.fixedDeltaTime * ctx.moveDirection);
    }

    private void Update()
    {
        ctx.grounded = CheckGrounded();

        if (!ctx.grounded)
        {

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
