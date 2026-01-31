using UnityEngine;
using System;
using System.Collections.Generic;


//mediator that will sit between stat class and the stat modifiers
public class StatsMediator
{
    readonly LinkedList<StatModifier> modifiers = new();

    public event EventHandler<Query> Queries;
    public void PerformQuery(object sender, Query query) => Queries?.Invoke(sender, query);

    public void AddModifier(StatModifier modifier)
    {
        modifiers.AddLast(modifier);
        Queries += modifier.Handle;
    }

    public void Update(float deltaTime)
    {
        //update all modifiers with deltaTime
        var node = modifiers.First;
        while (node != null)
        {
            var modifier = node.Value;

            // CJ this line was part of the tutorial, but im not sure if we need it? might be referring to the timer components we aren't including
            //modifier.Update(deltaTime);

            node = node.Next;
        }

    }

}

public class Query
{
    public readonly StatType StatType;
    public int Value;

    public Query(StatType statType, int value)
    {
        StatType = statType;
        Value = value;
    }
}



