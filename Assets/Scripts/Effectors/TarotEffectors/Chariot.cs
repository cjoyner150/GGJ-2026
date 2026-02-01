using UnityEngine;

public class Chariot : Effector
{
    public Chariot()
    {
        //added +20 move speed, modify as needed
        modifiers.Add(
            new BasicStatModifier(
                StatType.MoveSpeed,
                duration: float.PositiveInfinity,
                operation: value => value + 20
                )
            );
    }

    public override void OnUpdate(float deltaTime)
    {
        // space for extra logic
    }
}
