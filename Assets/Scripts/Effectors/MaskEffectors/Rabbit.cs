using UnityEngine;

public class Rabbit : Effector
{
    public Rabbit()
    {
        //subtracted -20 scale, modify as needed
        modifiers.Add(
            new BasicStatModifier(
                StatType.Scale,
                duration: float.PositiveInfinity,
                operation: value => value - 20
                )
            );

        //subtracted -20 health, modify as needed
        modifiers.Add(
            new BasicStatModifier(
                StatType.Health,
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
