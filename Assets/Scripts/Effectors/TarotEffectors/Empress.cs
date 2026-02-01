using UnityEngine;

public class Empress : Effector
{
    public Empress()
    {
        //added +20 attack speed, modify as needed
        modifiers.Add(
            new BasicStatModifier(
                StatType.AttackSpeed,
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
