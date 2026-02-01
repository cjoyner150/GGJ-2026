using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerStatManager : MonoBehaviour
{
    [SerializeField] private PlayerContext player;

    public void Initialize(PlayerConfig pconfig)
    {
        MaskObject mask = pconfig.Mask;
        Effector effector = mask.maskEffector;

        // Loop through all modifiers in the effector
        foreach (var mod in effector.Modifiers)
        {
            ApplyModifier(mod);
        }
    }
    private void ApplyModifier(StatModifier mod)
    {
        if (mod is BasicStatModifier basicMod)
        {
            var typeField = typeof(BasicStatModifier).GetField("type",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var type = (StatType)typeField.GetValue(basicMod);

            switch (type)
            {
                case StatType.Health:
                    player.maxHealth = basicModOperation(basicMod, player.maxHealth);
                    break;
                case StatType.MoveSpeed:
                    player.walkMoveSpeed = basicModOperation(basicMod, player.walkMoveSpeed);
                    break;
                case StatType.AttackSpeed:
                    player.attackSpeed = basicModOperation(basicMod, player.attackSpeed);
                    break;
                case StatType.Power:
                    player.attackDamage = basicModOperation(basicMod, player.attackDamage);
                    break;
                case StatType.Scale:
                    player.scale = basicModOperation(basicMod, player.scale);
                    break;
                default:
                    Debug.LogWarning("Unhandled StatType in PlayerStatManager: " + type);
                    break;
            }
        }
    }
    private float basicModOperation(BasicStatModifier mod, float currentValue)
    {
        var opField = typeof(BasicStatModifier).GetField("operation",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var operation = (Func<int, int>)opField.GetValue(mod);

        // PlayerContext uses floats, so convert to int for the operation and back
        int result = operation(Mathf.RoundToInt(currentValue));
        return (float)result;
    }

}

