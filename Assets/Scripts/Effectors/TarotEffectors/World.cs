using UnityEngine;

public class World : Effector
{
    public World()
    {
        //added +20 power, modify as needed
        modifiers.Add(
            new BasicStatModifier(
                StatType.Power,
                duration: float.PositiveInfinity,
                operation: value => value + 20
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
