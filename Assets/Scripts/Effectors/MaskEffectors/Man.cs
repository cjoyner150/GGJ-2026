using UnityEngine;

public class Man : Effector
{
    private float timeUntilDeath;
    private bool hasExploded;

    private IntEventSO playerDeathEvent;
    
    public Man()
    {
        //Explode and die immediately after a specific amount of time
        //setting timer to 20 seconds, change as needed
        timeUntilDeath = 20f;
        hasExploded = false;

    }

    public override void OnUpdate(float deltaTime)
    {
        if (hasExploded) 
            return;

        timeUntilDeath -= deltaTime;

        if (timeUntilDeath <= 0)
        {
            hasExploded = true;

            //trigger explosion animation here

            //trigger death event
            //i dont know the number/payload of this event,, put 0 by default
            playerDeathEvent.RaiseEvent(0);

        }
    }
}
