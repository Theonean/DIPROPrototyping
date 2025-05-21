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
            targetPosition = new Vector3(value.x, 0.5f, value.z);
        }
    }

    [SerializeField] protected int[] targetMoveStepPerSecondPerLevel = new int[5] { 0, 10, 20, 30, 40 };
    private int targetMoveStep;
    private float distanceToTarget;
    [Header("VFX")]
    public VisualEffect vfxFlying;

    [Header("SFX")]
    public string rocketFlyAwaySFXPath = "event:/SFX/Dron/Shoot";
    public string rocketCallbackSFXPath = "event:/SFX/Dron/Callingback_Missiles";
    public abstract IEnumerator FlyToTargetPosition();

    private GameObject debugTargetPosition;

    protected override void Start()
    {
        base.Start();

        debugTargetPosition = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugTargetPosition.name = "DebugTargetSphere";
        debugTargetPosition.transform.position = Vector3.zero;
        debugTargetPosition.transform.localScale = Vector3.one * 0.5f; // Small sphere
    }


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
        targetMoveStep = targetMoveStepPerSecondPerLevel[componentLevel];
        Logger.Log($"Leveling up {DescriptiveName} to level {componentLevel}. Degree move to mouse: {targetMoveStep}", LogLevel.INFO, LogType.ROCKETS);
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

        if(parentRocket.state == RocketState.FLYING)
        {
            debugTargetPosition.transform.position = targetPosition;
        }
    }
    
    private void MoveTargetPositionToMouse()
    {
        // Get the mouse position in world space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 mousePosition = hit.point;

            Vector2 mapPosition = new Vector2(mousePosition.x, mousePosition.z);
            Vector2 targetPosition2D = new Vector2(TargetPosition.x, TargetPosition.z);
            Vector2 moveDirection = (mapPosition - targetPosition2D).normalized;

            Vector2 newTargetPosition = Vector2.Lerp(targetPosition2D, targetPosition2D + moveDirection * targetMoveStep, Time.deltaTime);
            TargetPosition = new Vector3(newTargetPosition.x, 0f, newTargetPosition.y);
        }
    }

    public void Shoot(Vector3 target)
    {
        TargetPosition = target;
        distanceToTarget = Vector3.Distance(rocketTransform.position, TargetPosition);

        rocketTransform.SetParent(null);
        rocketTransform.LookAt(TargetPosition);

        FMODAudioManagement.instance.PlayOneShot(rocketFlyAwaySFXPath, gameObject);

        StartCoroutine(Fly());
        StartCoroutine(TrackDistanceToTarget());
    }
    
    private IEnumerator TrackDistanceToTarget()
    {
        Vector2 lastPosition = new Vector2(rocketTransform.position.x, rocketTransform.position.z);
        while (distanceToTarget > 0f)
        {
            Vector2 currentPosition = new Vector2(rocketTransform.position.x, rocketTransform.position.z);
            distanceToTarget -= Vector3.Distance(currentPosition, lastPosition);
            lastPosition = new Vector2(rocketTransform.position.x, rocketTransform.position.z);
            yield return null;
        }
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

        RocketAimController.Instance.OnRocketRetract?.Invoke();

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
    public override string GetResearchDescription()
    {
        if (componentLevel == maxComponentLevel)
        {
            return upgradeDescription + " " + targetMoveStep;
        }
        else
        {
            return upgradeDescription + " " + targetMoveStep + " -> " + targetMoveStepPerSecondPerLevel[componentLevel+1];
        }
    }
}