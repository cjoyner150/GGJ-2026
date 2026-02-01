using UnityEngine;

public class Turtle : Effector
{
    public Turtle()
    {
        //cutting movement speed to 10%
        modifiers.Add(
            new BasicStatModifier(
                StatType.MoveSpeed,
                duration: float.PositiveInfinity,
                operation: value => (int)(value * 0.1f)
                )
            );
    }

    public override void OnUpdate(float deltaTime)
    {
        // space for extra logic
    }
}
