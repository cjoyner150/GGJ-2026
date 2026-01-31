using UnityEngine;

[CreateAssetMenu(fileName = "BaseStats", menuName = "Scriptable Objects/BaseStats")]
public class BaseStats : ScriptableObject
{
    //listing the basic stats players start with before mods
    //literally made up random numbers, change as you wish
    public int health = 100;
    public int power = 200;
    public int attackSpeed = 50;
    public int moveSpeed = 85;
    public int scale = 100;
}
