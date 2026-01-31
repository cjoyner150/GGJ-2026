using UnityEngine;

//defining all stats in an enum
public enum StatType {Health, Power, AttackSpeed, MoveSpeed, Scale}
public class Stats
{
    readonly BaseStats baseStats;
    readonly StatsMediator mediator;

    public StatsMediator Mediator => mediator;


    public int Health
    {
        get
        {
            //return value with modifiers applied
            var q = new Query(StatType.Health, baseStats.health);
            mediator.PerformQuery(sender: this, q);
            return q.Value;
        }
    }

    public int Power
    {
        get
        {
            //return value with modifiers applied
            var q = new Query(StatType.Power, baseStats.power);
            mediator.PerformQuery(sender: this, q);
            return q.Value;
        }
    }

    public int AttackSpeed
    {
        get
        {
            //return value with modifiers applied
            var q = new Query(StatType.AttackSpeed, baseStats.attackSpeed);
            mediator.PerformQuery(sender: this, q);
            return q.Value;
        }
    }

    public int MoveSpeed
    {
        get
        {
            //return value with modifiers applied
            var q = new Query(StatType.MoveSpeed, baseStats.moveSpeed);
            mediator.PerformQuery(sender: this, q);
            return q.Value;
        }
    }

    public int Scale
    {
        get
        {
            //return value with modifiers applied
            var q = new Query(StatType.Scale, baseStats.scale);
            mediator.PerformQuery(sender: this, q);
            return q.Value;
        }
    }

    public Stats(StatsMediator mediator, BaseStats baseStats)
    {
        this.mediator = mediator;
        this.baseStats = baseStats;
    }

    //debugging
    public override string ToString() => $"Attack: {Attack}, Defense: {Defense}, Health: {Health}, Power: {Power}, AttackSpeed: {AttackSpeed}, MoveSpeed: {MoveSpeed}, Scale: {Scale}";

}
