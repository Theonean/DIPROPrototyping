using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public abstract class ACRocketPropulsion : ACRocketComponent
{
    public VisualEffect vfxFlying;
    private Coroutine flightCoroutine;
    public abstract IEnumerator FlyToTargetPosition(Vector3 targetPos);


    void OnEnable()
    {
        ParentRocket.OnRocketStateChange.AddListener(AbortFlight);
    }

    void OnDisable()
    {
        ParentRocket.OnRocketStateChange.RemoveListener(AbortFlight);
    }

    private void Update()
    {
        if (ParentRocket.CanBeReturned())
        {
            if (Input.GetMouseButtonDown(1))
            {
                ParentRocket.SetState(RocketState.RETURNING);
                flightCoroutine = StartCoroutine(ReturnToDrone());
            }
        }
    }

    public void Shoot(Vector3 target)
    {
        Vector3 correctedTarget = new Vector3(target.x, rocketTransform.position.y, target.z);

        rocketTransform.SetParent(null);
        rocketTransform.LookAt(correctedTarget);

        StartCoroutine(Fly(correctedTarget));
    }

    private IEnumerator Fly(Vector3 target)
    {
        StartVFX();
        yield return StartCoroutine(FlyToTargetPosition(target));
        StopVFX();

        Logger.Log("Rocket reached target", LogLevel.INFO, LogType.ROCKETS);
        ParentRocket.SetState(RocketState.IDLE);
    }

    private void AbortFlight(RocketState state)
    {
        if (state != RocketState.FLYING)
        {
            StopAllCoroutines();
            StopVFX();
        }
    }

    private IEnumerator ReturnToDrone()
    {
        while (true)
        {
            // Use the initial rocketTransform's position and rotation instead of separate variables
            Vector3 updatedTargetPosition = ParentRocket.initialTransform.position;

            // Calculate progress based on the updated target position
            float distanceToUpdatedTarget = Vector3.Distance(rocketTransform.position, updatedTargetPosition);
            float totalDistanceToReturn = Vector3.Distance(rocketTransform.position, updatedTargetPosition);
            float tReturn = 1f - (distanceToUpdatedTarget / totalDistanceToReturn);

            // Move the rocket back to the initial position using animation curve
            if (tReturn < 0.90f)
            {
                rocketTransform.position = Vector3.MoveTowards(rocketTransform.position, updatedTargetPosition, ParentRocket.settings.flySpeedCurve.Evaluate(tReturn) * Time.deltaTime * ParentRocket.settings.flySpeed * 2f);
            }
            else
            {
                rocketTransform.position = Vector3.Lerp(rocketTransform.position, updatedTargetPosition, Time.deltaTime * ParentRocket.settings.flySpeed * 2f);
            }

            // Update the rotation of the rocket to smoothly return to its initial rotation
            rocketTransform.rotation = Quaternion.RotateTowards(rocketTransform.rotation, ParentRocket.initialTransform.rotation, 1f * Time.deltaTime);
            rocketTransform.localScale = Vector3.MoveTowards(rocketTransform.localScale, rocketOriginalScale, 0.1f * Time.deltaTime * ParentRocket.settings.flySpeed * 1.5f);

            if (distanceToUpdatedTarget < 1f)
            {
                rocketTransform.position = updatedTargetPosition;
                ParentRocket.SetState(RocketState.ATTACHED);
                rocketTransform.SetParent(parentRocket.initialTransform.parent);
                rocketTransform.rotation = ParentRocket.initialTransform.rotation;
                yield break;
            }

            yield return null;
        }
    }
    
    private void StartVFX()
    {
        vfxFlying.Reinit();
        vfxFlying.Play();
    }
    private void StopVFX()
    {
        // stop vfx is it is still playing
        if (vfxFlying.HasAnySystemAwake())
            vfxFlying.Stop();
    }
}