using UnityEngine;

public class PenetrativeFront : ACRocketFront
{
    public override void ActivateAbility(Collider collider)
    {
        Logger.Log("Penetrative front activated " + collider.gameObject.name, LogLevel.INFO, LogType.ROCKETS);
    }
}
