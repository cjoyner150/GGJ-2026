using UnityEngine;

public class Fool : Effector
{
    public Fool()
    {
        //added +50 move speed, modify as needed
        modifiers.Add(
            new BasicStatModifier(
                StatType.MoveSpeed,
                duration: float.PositiveInfinity,
                operation: value => value + 50
                )
            );
    }

    public override void OnUpdate(float deltaTime)
    {
        // space for extra logic

        //You can only attack once every 10 seconds but run SUPER fast
    }
}
