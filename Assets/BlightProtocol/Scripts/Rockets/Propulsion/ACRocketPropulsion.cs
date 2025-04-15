using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public abstract class ACRocketPropulsion : ACRocketComponent
{
    private Vector3 targetPosition;
    public Vector3 TargetPosition
    {
        get
        {
            return targetPosition;
        }
        set
        {
            targetPosition = new Vector3(value.x, ParentRocket.transform.position.y, value.z);
        }
    }

    [SerializeField] protected int[] degreeMoveToMousePerLevel = new int[5] { 0, 10, 20, 30, 40 };
    private int degreeMoveToMouseBase;
    [Header("VFX")]
    public VisualEffect vfxFlying;

    [Header("SFX")]
    public string rocketFlyAwaySFXPath = "event:/SFX/Dron/Shoot";
    public string rocketCallbackSFXPath = "event:/SFX/Dron/Callingback_Missiles";
    public abstract IEnumerator FlyToTargetPosition();


    protected override void OnEnable()
    {
        base.OnEnable();
        if (ParentRocket == null)
            return;
        ParentRocket.OnRocketStateChange.AddListener(RocketChangedState);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (ParentRocket == null)
            return;
        ParentRocket.OnRocketStateChange.RemoveListener(RocketChangedState);
    }

    protected override void SetStatsToLevel()
    {
        degreeMoveToMouseBase = degreeMoveToMousePerLevel[componentLevel];
        Logger.Log($"Leveling up {DescriptiveName} to level {componentLevel + 1}. Degree move to mouse: {degreeMoveToMouseBase}", LogLevel.INFO, LogType.ROCKETS);
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
            else if (componentLevel >= 1)
            {
                MoveTargetPositionToMouse();
            }
        }
    }
    private void MoveTargetPositionToMouse()
    {
        // Get the mouse position in world space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 mousePosition = hit.point;

            // Calculate the direction to the mouse position
            Vector3 directionToMouse = (mousePosition - rocketTransform.position).normalized;

            // Calculate the new target position with a limited degree step
            Quaternion currentRotation = Quaternion.LookRotation(rocketTransform.forward);
            Quaternion targetRotation = Quaternion.LookRotation(directionToMouse);
            Quaternion limitedRotation = Quaternion.RotateTowards(currentRotation, targetRotation, degreeMoveToMouseBase * Time.deltaTime);

            // Update the target position based on the limited rotation
            Vector3 newDirection = limitedRotation * Vector3.forward;
            TargetPosition = rocketTransform.position + newDirection * Vector3.Distance(rocketTransform.position, TargetPosition);
        }
    }

    public void Shoot(Vector3 target)
    {
        TargetPosition = target;

        rocketTransform.SetParent(null);
        rocketTransform.LookAt(TargetPosition);

        FMODAudioManagement.instance.PlayOneShot(rocketFlyAwaySFXPath, gameObject);

        StartCoroutine(Fly());
    }

    private IEnumerator Fly()
    {
        StartVFX();
        yield return StartCoroutine(FlyToTargetPosition());
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