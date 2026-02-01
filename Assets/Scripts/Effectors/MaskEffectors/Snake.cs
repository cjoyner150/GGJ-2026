using UnityEngine;

public class Snake : Effector
{

    private float newJumpMultiplier;
    public Snake()
    {
        //No jumping

    }

    public override void OnApply(PlayerContext context)
    {
        newJumpMultiplier = context.jumpMultiplier;
        context.jumpMultiplier = 0f;
    }

    public override void OnUpdate(float deltaTime)
    {
        // space for extra logic

    }
}
