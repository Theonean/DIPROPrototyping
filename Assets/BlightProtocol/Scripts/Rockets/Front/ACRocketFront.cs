
using UnityEngine;

public abstract class ACRocketFront : ACRocketComponent
{
    public abstract void ActivateAbility(Collider collider);

    //When collision happens, check if object has the right tag and if it does, destroy this object
    private void OnTriggerEnter(Collider other)
    {
        Logger.Log("Rocket front collided with " + other.gameObject.name, LogLevel.INFO, LogType.ROCKETS);
        ActivateAbility(other);
    }
}