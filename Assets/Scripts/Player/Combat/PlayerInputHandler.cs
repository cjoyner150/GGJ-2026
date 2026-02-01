using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    static UnityEvent<int> beeEvent = new UnityEvent<int>();

    public PlayerContext ctx;
    [SerializeField] float delayInputForSeconds;
    [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;

    PlayerController playerMover;
    PlayerConfig config;

    private Player controls;

    bool canInput = false;

    private void OnEnable()
    {
        beeEvent.AddListener(OnBeeEvent);
    }

    private void OnDisable()
    {
        beeEvent.RemoveListener(OnBeeEvent);
    }

    private void OnBeeEvent(int playerIndex)
    {
        if (config.PlayerIndex != playerIndex)
        {
            ctx.maxHealth *= 1.2f;
            ctx.attackDamage *= 1.2f;
        }
    }

    void RaiseBeeEvent() => beeEvent.Invoke(config.PlayerIndex);

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
        cfg.Input.SwitchCurrentActionMap("PlayerCombat");
        config.Input.onActionTriggered += OnActionTriggered;

        playerMover.playerIndex = config.PlayerIndex;
        playerMover.ctx = ctx;
        ctx.currentHealth = ctx.maxHealth;

        skinnedMeshRenderer.material.color = config.PlayerColor;

        switch (cfg.Mask.type)
        {
            case MaskObject.maskType.Bear:
                ctx.scale *= 1.5f;
                ctx.walkMoveSpeed *= 0.75f;
                break;
            case MaskObject.maskType.Bee:
                Invoke(nameof(RaiseBeeEvent), 1f);
                break;
            case MaskObject.maskType.Butterfly:
                ctx.knockbackMultiplier *= 2f;
                break;
            case MaskObject.maskType.Crow:
                config.Tarots.RemoveAt(Random.Range(0, config.Tarots.Count));
                break;
            case MaskObject.maskType.Man:
                ctx.man = true;
                break;
            case MaskObject.maskType.Goddess:
                break;
            case MaskObject.maskType.Rabbit:
                ctx.scale *= .5f;
                ctx.maxHealth *= .5f;
                ctx.currentHealth = ctx.maxHealth;
                break;
            case MaskObject.maskType.Snake:
                ctx.jumps = 0;
                break;
            case MaskObject.maskType.Turtle:
                ctx.walkMoveSpeed *= .1f;
                break;

        }

        foreach (var tarot in cfg.Tarots)
        {
            switch (tarot.type)
            {
                case TarotObject.cardType.Chariot:
                    ctx.walkMoveSpeed *= 1.25f;
                    break;
                case TarotObject.cardType.Devil:
                    ctx.attackDamage *= 1.5f;
                    break;
                case TarotObject.cardType.Empress:
                    ctx.attackSpeed *= 1.5f;
                    break;
                case TarotObject.cardType.HighPriestess:
                    ctx.maxHealth *= 1.5f;
                    ctx.currentHealth = ctx.maxHealth;
                    break;
                case TarotObject.cardType.Magician:
                    ctx.jumps++;
                    break;
                case TarotObject.cardType.Moon:
                    ctx.groundDrag = 1;
                    break;
                case TarotObject.cardType.Star:
                    ctx.attackDamage *= 1.2f;
                    ctx.attackSpeed *= 1.2f;
                    ctx.walkMoveSpeed *= 1.1f;
                    ctx.maxHealth *= 1.2f;
                    ctx.currentHealth = ctx.maxHealth;
                    break;
                case TarotObject.cardType.Wheel:
                    
                    for (int i = 0; i < 2; i++)
                    {
                        int rand = Random.Range(0, 7);
                        switch ((TarotObject.cardType)rand)
                        {
                            case TarotObject.cardType.Chariot:
                                ctx.walkMoveSpeed *= 1.25f;
                                break;
                            case TarotObject.cardType.Devil:
                                ctx.attackDamage *= 1.5f;
                                break;
                            case TarotObject.cardType.Empress:
                                ctx.attackSpeed *= 1.5f;
                                break;
                            case TarotObject.cardType.HighPriestess:
                                ctx.maxHealth *= 1.5f;
                                ctx.currentHealth = ctx.maxHealth;
                                break;
                            case TarotObject.cardType.Magician:
                                ctx.jumps++;
                                break;
                            case TarotObject.cardType.Moon:
                                ctx.groundDrag = 0;
                                break;
                            case TarotObject.cardType.Star:
                                ctx.attackDamage *= 1.2f;
                                ctx.attackSpeed *= 1.2f;
                                ctx.walkMoveSpeed *= 1.1f;
                                ctx.maxHealth *= 1.2f;
                                ctx.currentHealth = ctx.maxHealth;
                                break;

                        }

                    }

                    break;
            }
        }
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
        if (playerMover.currentState == PlayerController.MoveState.Idle || playerMover.currentState == PlayerController.MoveState.Walk || playerMover.currentState == PlayerController.MoveState.Air)
        {
            ctx.dashHasBeenPressed = true;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (playerMover.currentState == PlayerController.MoveState.Idle || playerMover.currentState == PlayerController.MoveState.Walk || playerMover.currentState == PlayerController.MoveState.Air)
        {
            ctx.attackHasBeenPressed = true;
        }
    }

    private void OnDestroy()
    {
        config.Input.onActionTriggered -= OnActionTriggered;
    }


}
