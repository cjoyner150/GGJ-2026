using System;
using UnityEngine;

public class BasicStatModifier : StatModifier
{
    readonly StatType type;
    readonly Func<int, int> operation;

    public BasicStatModifier(StatType type, Func<int, int> operation)
    {
        this.type = type;
        this.operation = operation;
    }

    public override void Handle(object sender, Query query)
    {
        if (query.StatType == type)
        {
            query.Value = operation(query.Value);
        }
    }
}

public abstract class StatModifier : IDisposable
{
    public event Action<StatModifier> OnDispose = delegate { };
    public abstract void Handle(object sender, Query query);

    public void Dispose()
    {
        OnDispose.Invoke(obj:this);
    }
}
