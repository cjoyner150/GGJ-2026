using System.Collections.Generic;
using UnityEngine;

public abstract class Effector
{
    //protected list of all the modifiers
    protected List<StatModifier> modifiers => new List<StatModifier>();

    //stat system reads this
    public IReadOnlyList<StatModifier> Modifiers => modifiers;

    public virtual void OnUpdate(float deltaTime)
    {

    }

}
