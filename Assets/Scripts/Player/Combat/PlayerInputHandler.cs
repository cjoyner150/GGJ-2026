using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] PlayerContext ctx;
    [SerializeField] float delayInputForSeconds;

    PlayerController playerMover;
    PlayerConfig config;

    private Player controls;

    bool canInput = false;

    private void Awake()
    {
        playerMover = GetComponent<PlayerController>();
        controls = new Player();
    }

    private void Start()
    {
        Invoke(nameof(SetCanInput), delayInputForSeconds);
    }

    private void SetCanInput()
    {
        canInput = true;
    }

    public void InitializePlayer(PlayerConfig cfg)
    {
        config = cfg;

        config.Input.onActionTriggered += OnActionTriggered;

        playerMover.playerIndex = config.PlayerIndex;
        playerMover.ctx = ctx;
        ctx.currentHealth = ctx.maxHealth;

        GetComponentInChildren<MeshRenderer>().material.color = config.PlayerColor;
    }

    public void OnActionTriggered(InputAction.CallbackContext context)
    {
        if (!canInput) return;

        if (context.action.name == controls.PlayerCombat.Locomotion.name) OnMove(context);
        
        if (context.action.name == controls.PlayerCombat.Jump.name) OnJump(context);
        if (context.action.name == controls.PlayerCombat.Dash.name) OnDash(context);
        if (context.action.name == controls.PlayerCombat.Attack.name) OnAttack(context);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var moveVector = context.ReadValue<Vector2>();

        ctx.moveDirection = new Vector3(moveVector.x, 0, moveVector.y);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (playerMover.currentState == PlayerController.MoveState.Idle || playerMover.currentState == PlayerController.MoveState.Walk || playerMover.currentState == PlayerController.MoveState.Air)
        {
            ctx.jumpHasBeenPressed = true;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (playerMover.currentState == PlayerController.MoveState.Idle || playerMover.currentState == PlayerController.MoveState.Walk)
        {
            ctx.dashHasBeenPressed = true;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (playerMover.currentState == PlayerController.MoveState.Idle || playerMover.currentState == PlayerController.MoveState.Walk)
        {
            ctx.attackHasBeenPressed = true;
        }
    }

    private void OnDestroy()
    {
        config.Input.onActionTriggered -= OnActionTriggered;
    }


}
