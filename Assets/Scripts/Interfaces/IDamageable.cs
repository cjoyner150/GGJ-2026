using UnityEngine;

public interface IDamageable
{
    public enum HitCallbackContext
    {
        success,
        parried,
        invulnerable
    }
    public void Hit(float damage, out HitCallbackContext callbackContext, Vector3 fromPosition);
}
