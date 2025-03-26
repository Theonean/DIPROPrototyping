using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class ACRocketBody : ACRocketComponent
{
    [Header("Debug")]
    public bool debugExplosionSphere; //If true, a gizmo will be drawn to show the explosion radius.
    [Header("Explosion Settings")]
    public Material explosionMaterial; //Material for the explosion effect when the leg explodes.

    public GameObject explosionPrefab; //The explosion prefab to spawn when the leg explodes.

    //Private settings
    protected abstract void Explode();
    public void TryExplode()
    {
        if (ParentRocket.CanExplode())
        {
            ParentRocket.SetState(RocketState.EXPLODING);
            Logger.Log("Rocket clicked", LogLevel.INFO, LogType.ROCKETS);
            Explode();

            rocketTransform.position = ParentRocket.initialTransform.position;
            rocketTransform.rotation = ParentRocket.initialTransform.rotation;
            rocketTransform.SetParent(parentRocket.initialTransform.parent);
            rocketTransform.localScale = Vector3.zero;

            ParentRocket.SetState(RocketState.REGROWING);
            StartCoroutine(RegrowRocket());
        }
    }

    private void OnMouseDown()
    {
        TryExplode();
    }

    private IEnumerator RegrowRocket()
    {
        float t = 0;
        Vector3 startScale = Vector3.zero;
        rocketTransform.localScale = startScale;

        while (Vector3.Distance(rocketTransform.localScale, rocketOriginalScale) > 0.1f)
        {
            t += Time.deltaTime;
            rocketTransform.localScale = Vector3.Lerp(startScale, rocketOriginalScale, t / ParentRocket.settings.regrowDurationAfterExplosion);
            yield return null;
        }

        rocketTransform.localScale = rocketOriginalScale;
        ParentRocket.SetState(RocketState.ATTACHED);
    }
}