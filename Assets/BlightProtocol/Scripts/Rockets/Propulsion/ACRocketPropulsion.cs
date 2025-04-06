using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public abstract class ACRocketPropulsion : ACRocketComponent
{
    [Header("VFX")]
    public VisualEffect vfxFlying;

    [Header("SFX")]
    public string rocketFlyAwaySFXPath = "event:/SFX/Dron/Shoot";
    public string rocketCallbackSFXPath = "event:/SFX/Dron/Callingback_Missiles";
    public abstract IEnumerator FlyToTargetPosition(Vector3 targetPos);


    void OnEnable()
    {
        if (ParentRocket == null)
            return;
        ParentRocket.OnRocketStateChange.AddListener(RocketChangedState);
    }

    void OnDisable()
    {
        if (ParentRocket == null)
            return;
        ParentRocket.OnRocketStateChange.RemoveListener(RocketChangedState);
    }

    private void Update()
    {
        if (ParentRocket == null)
            return;

        if (ParentRocket.CanBeReturned())
        {
            if (Input.GetMouseButtonDown(1))
            {
                ParentRocket.SetState(RocketState.RETURNING);
                FMODAudioManagement.instance.PlayOneShot(rocketCallbackSFXPath, gameObject);
                StartCoroutine(ReturnToDrone());
            }
        }
    }

    public void Shoot(Vector3 target)
    {
        Vector3 correctedTarget = new Vector3(target.x, rocketTransform.position.y, target.z);

        rocketTransform.SetParent(null);
        rocketTransform.LookAt(correctedTarget);

        FMODAudioManagement.instance.PlayOneShot(rocketFlyAwaySFXPath, gameObject);

        StartCoroutine(Fly(correctedTarget));
    }

    private IEnumerator Fly(Vector3 target)
    {
        StartVFX();
        yield return StartCoroutine(FlyToTargetPosition(target));
        StopVFX();
    }

    private void RocketChangedState(RocketState state)
    {
        StopAllCoroutines();
        StopVFX();
    }

    protected IEnumerator ReturnToDrone()
    {
        if (ParentRocket.propulsionComponent.GetType() == typeof(StraightLinePropulsion)) rocketTransform.position = new Vector3(rocketTransform.position.x, ParentRocket.initialTransform.position.y, rocketTransform.position.z);

        while (true)
        {
            // Use the initial rocketTransform's position and rotation instead of separate variables
            Vector3 updatedTargetPosition = ParentRocket.initialTransform.position;

            // Calculate progress based on the updated target position
            float distanceToUpdatedTarget = Vector3.Distance(rocketTransform.position, updatedTargetPosition);
            float totalDistanceToReturn = Vector3.Distance(rocketTransform.position, updatedTargetPosition);
            float tReturn = 1f - (distanceToUpdatedTarget / totalDistanceToReturn);

            rocketTransform.LookAt(updatedTargetPosition);

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
            //rocketTransform.rotation = Quaternion.RotateTowards(rocketTransform.rotation, ParentRocket.initialTransform.rotation, 1f * Time.deltaTime);
            //rocketTransform.localScale = Vector3.MoveTowards(rocketTransform.localScale, rocketOriginalScale, 0.1f * Time.deltaTime * ParentRocket.settings.flySpeed * 1.5f);

            if (distanceToUpdatedTarget < 1f)
            {
                parentRocket.ReattachRocketToDrone();
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