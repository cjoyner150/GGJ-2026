using UnityEngine;

public class Bear : Effector
{
    public Bear()
    {
        //added +20 scale, modify as needed
        modifiers.Add(
            new BasicStatModifier(
                StatType.Scale,
                duration: float.PositiveInfinity,
                operation: value => value + 20
                )
            );

        //subtracted -20 move speed, modify as needed
        modifiers.Add(
            new BasicStatModifier(
                StatType.MoveSpeed,
                duration: float.PositiveInfinity,
                operation: value => value - 20
                )
            );

    }

    public override void OnUpdate(float deltaTime)
    {
        // space for extra logic
    }
}
