using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public enum LegState
{
    ATTACHED,
    CLICKED,
    FLYING,
    DETACHED,
    RETURNING,
    EXPLODING,
    REGROWING
}

public class LegHandler : MonoBehaviour
{
    [SerializeField] private RocketData m_rocketData;
    public LegState m_LegState = LegState.ATTACHED;
    public GameObject LegHandlerParent;
    public GameObject explosionPrefab; //The explosion prefab to spawn when the leg explodes.
    public bool debugExplosionSphere; //If true, a gizmo will be drawn to show the explosion radius.
    private bool isSpinning = false; //If the leg is currently spinning (one form of attack).
    Vector3 m_StartingPosition; //position the leg started flying from
    Vector3 m_TargetPosition; //position the leg will fly to when in FLYING state.
    public AnimationCurve flySpeedCurve; //Curve for the speed of the leg when flying away.
    public Material explosionMaterial; //Material for the explosion effect when the leg explodes.
    Vector3 m_LegOriginalScale; //Original scale of the leg.
    Camera m_Camera;
    PlayerCore core;

    Transform m_InitialTransform; // Transform to hold the initial position and rotation.

    [Header("VFX")]
    public VisualEffect vfxFlying;

    [Header("SFX")]
    public string rocketFlyAwaySFXPath = "event:/...";
    public string rocketCallbackSFXPath = "event:/...";


    void Awake()
    {
        //Find parent "core" object and subscribe to the return event
        core = FindObjectOfType<PlayerCore>();
        core.returnLegs.AddListener(() =>
        {
            if (m_LegState == LegState.DETACHED || m_LegState == LegState.FLYING)
            {
                isSpinning = false;
                m_LegState = LegState.RETURNING;
                m_StartingPosition = transform.position;

                FMODAudioManagement.instance.PlayOneShot(rocketCallbackSFXPath, gameObject);
            }
        });

        m_rocketData.explosionRadius = m_rocketData.explosionRadiusBase;
        m_rocketData.flySpeed = m_rocketData.flySpeedBase;

        m_Camera = Camera.main;

        // Create a new GameObject to hold the initial transform
        GameObject initialTransformHolder = new GameObject($"{gameObject.name}_InitialTransform");
        initialTransformHolder.transform.SetParent(LegHandlerParent.transform);
        initialTransformHolder.transform.position = transform.position;
        initialTransformHolder.transform.rotation = transform.rotation;

        m_InitialTransform = initialTransformHolder.transform;

        m_LegOriginalScale = transform.localScale;
    }

    private void OnEnable()
    {
        Collectible.OnCollectiblePickedUp.AddListener(m_rocketData.ApplyTemporaryBoost);
    }

    private void OnDisable()
    {
        Collectible.OnCollectiblePickedUp.RemoveListener(m_rocketData.ApplyTemporaryBoost);
    }

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = m_Camera.ScreenPointToRay(mousePosition);

        //Decide on action based on current leg state.
        switch (m_LegState)
        {
            case LegState.ATTACHED:
                //Do Nothing while simply attached to body
                break;
            case LegState.CLICKED:
                break;
            case LegState.FLYING:
                float t = Vector3.Distance(transform.position, m_TargetPosition) / Vector3.Distance(m_StartingPosition, m_TargetPosition);

                //Update position to move towards target position using animation curve
                transform.position = Vector3.MoveTowards(transform.position, m_TargetPosition, flySpeedCurve.Evaluate(t) * Time.deltaTime * m_rocketData.flySpeed);

                //Lerp scale by how close leg is to final position
                transform.localScale = Vector3.Lerp(transform.localScale, m_LegOriginalScale * m_rocketData.flyScaleMultiplier, 0.1f * Time.deltaTime * m_rocketData.flySpeed);

                if (Vector3.Distance(transform.position, m_TargetPosition) < 0.1f)
                {
                    m_LegState = LegState.DETACHED;

                    StopVFX();
                }
                break;
            case LegState.DETACHED:
                break;
            case LegState.RETURNING:
                // Use the initial transform's position and rotation instead of separate variables
                Vector3 updatedTargetPosition = m_InitialTransform.position;

                // Calculate progress based on the updated target position
                float distanceToUpdatedTarget = Vector3.Distance(transform.position, updatedTargetPosition);
                float totalDistanceToReturn = Vector3.Distance(m_StartingPosition, updatedTargetPosition);
                float tReturn = 1f - (distanceToUpdatedTarget / totalDistanceToReturn);

                // Move the leg back to the player core using animation curve
                if (tReturn < 0.90f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, updatedTargetPosition, flySpeedCurve.Evaluate(tReturn) * Time.deltaTime * m_rocketData.flySpeed * 2f);
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, updatedTargetPosition, Time.deltaTime * m_rocketData.flySpeed * 2f);
                }

                // Update the rotation of the leg to smoothly return to its initial rotation
                transform.rotation = Quaternion.RotateTowards(transform.rotation, m_InitialTransform.rotation, 1f * Time.deltaTime);
                transform.localScale = Vector3.MoveTowards(transform.localScale, m_LegOriginalScale, 0.1f * Time.deltaTime * m_rocketData.flySpeed * 1.5f);

                if (distanceToUpdatedTarget < 1f)
                {
                    transform.position = updatedTargetPosition;
                    m_LegState = LegState.ATTACHED;
                    transform.SetParent(LegHandlerParent.transform);
                    transform.rotation = m_InitialTransform.rotation;
                }

                StopVFX();

                break;
            case LegState.REGROWING:
                transform.localScale = Vector3.Lerp(transform.localScale, m_LegOriginalScale, m_rocketData.regrowDurationAfterExplosion * Time.deltaTime);
                if (Vector3.Distance(transform.localScale, m_LegOriginalScale) < 0.1f)
                {
                    m_LegState = LegState.ATTACHED;
                }
                break;
        }
    }

    public void LegClicked()
    {
        if (m_LegState == LegState.ATTACHED)
        {
            m_LegState = LegState.CLICKED;
        }
    }

    public void LegReleased(RaycastHit hit)
    {
        if (m_LegState == LegState.CLICKED)
        {
            m_LegState = LegState.FLYING;
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            m_TargetPosition = new Vector3(hit.point.x, gameObject.transform.position.y, hit.point.z); //Keep leg height
            m_StartingPosition = transform.position;
            transform.SetParent(null);
            transform.LookAt(m_TargetPosition);

            StartVFX();

            FMODAudioManagement.instance.PlayOneShot(rocketFlyAwaySFXPath, gameObject);
        }
    }

    public void ExplodeLeg()
    {
        switch (m_LegState)
        {
            case LegState.FLYING:
            case LegState.RETURNING:
            case LegState.DETACHED:
                m_LegState = LegState.EXPLODING;

                //Create explosion effect
                GameObject explosionEffect = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                explosionEffect.GetComponentInChildren<LegExplosionHandler>().SetExplosionRadius(m_rocketData.explosionRadius / 10);

                StopVFX();

                if (debugExplosionSphere)
                {
                    //Create gizmo with explosionradius, for debugging
                    GameObject gizmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    gizmo.transform.position = transform.position;
                    gizmo.transform.localScale = new Vector3(m_rocketData.explosionRadius * 2f, m_rocketData.explosionRadius * 2f, m_rocketData.explosionRadius * 2f);
                    gizmo.GetComponent<Renderer>().material = explosionMaterial;
                    gizmo.GetComponent<SphereCollider>().enabled = false;
                    //Destroy after 5 seconds
                    Destroy(gizmo, 5f);
                }

                Collider[] hitColliders = Physics.OverlapSphere(transform.position, m_rocketData.explosionRadius);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject.CompareTag("Enemy"))
                    {
                        hitCollider.gameObject.GetComponent<EnemyDamageHandler>().DestroyEnemy();
                    }
                    else if (hitCollider.gameObject.CompareTag("Leg"))
                    {
                        LegHandler leg = hitCollider.GetComponent<LegHandler>();

                        //Prevent back and forth loops
                        if (leg.IsRocketExplodable())
                        {
                            StartCoroutine(DaisyChainExplosion(leg));
                        }
                    }
                }

                isSpinning = false;
                transform.position = m_InitialTransform.position;
                transform.rotation = m_InitialTransform.rotation;
                transform.SetParent(LegHandlerParent.transform);
                m_LegState = LegState.REGROWING;
                transform.localScale = Vector3.zero;
                break;
        }
    }

    public bool IsRocketExplodable()
    {
        if (m_LegState == LegState.FLYING ||
            m_LegState == LegState.RETURNING ||
            m_LegState == LegState.DETACHED)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool isAttacking()
    {
        return isSpinning || m_LegState == LegState.FLYING || m_LegState == LegState.RETURNING;
    }

    public RocketData GetRocketData()
    {
        return m_rocketData;
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

    private IEnumerator DaisyChainExplosion(LegHandler leg)
    {
        yield return new WaitForSeconds(m_rocketData.explosionChainDelay);
        leg.ExplodeLeg();
    }
}
