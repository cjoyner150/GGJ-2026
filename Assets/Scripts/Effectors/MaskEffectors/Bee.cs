using UnityEngine;

public class Bee : Effector
{
    public Bee()
    {
        //Everyone else gets stat buffs. This process needs to be handled in something like the game manager
        //added +20 health, modify as needed
        modifiers.Add(
            new BasicStatModifier(
                StatType.Health,
                duration: float.PositiveInfinity,
                operation: value => value + 20
                )
            );

        //added +20 power, modify as needed
        modifiers.Add(
            new BasicStatModifier(
                StatType.Power,
                duration: float.PositiveInfinity,
                operation: value => value + 20
                )
            );

        //added +20 attack speed, modify as needed
        modifiers.Add(
            new BasicStatModifier(
                StatType.AttackSpeed,
                duration: float.PositiveInfinity,
                operation: value => value + 20
                )
            );

        //added +20 move speed, modify as needed
        modifiers.Add(
            new BasicStatModifier(
                StatType.MoveSpeed,
                duration: float.PositiveInfinity,
                operation: value => value + 20
                )
            );

        //added +20 scale, modify as needed
        modifiers.Add(
            new BasicStatModifier(
                StatType.Scale,
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
