using System.Collections;
using UnityEngine;

public enum LegState
{
    ATTACHED, // The leg is attached to the player.
    CLICKED, // The leg is clicked on and is waiting for next click to fly away.
    FLYING, // The leg is flying away.
    DETACHED, // The leg is detached from the player and lying on the floor (could be clicked again for attack).
    RETURNING, // The leg is returning to the player.
    REGROWING
}

public class LegHandler : MonoBehaviour
{
    /*
    IDEA CORNER:
        maybe scale up leg when it's flying away so it has more effect or range
        maybe add rightclick on leg when the leg is on floor to make a leg manually return to player
    */

    public LegState m_LegState = LegState.ATTACHED;
    public GameObject explosionPrefab; //The explosion prefab to spawn when the leg explodes.
    private bool isSpinning = false; //If the leg is currently spinning (one form of attack). to see all forms of attack, go to public function isAttacking
    Vector3 m_DistanceToCore; //local position the leg spawned in, used for returning the leg to the player.
    Quaternion m_InitialRotation; //local rotation the leg spawned in, used for returning the leg to the player.
    Vector3 m_StartingPosition; //position the leg started flying from
    Vector3 m_TargetPosition; //position the leg will fly to when in FLYING state.
    public float legFlySpeed = 50; //MaxSpeed of the leg when flying away.
    public AnimationCurve flySpeedCurve; //Curve for the speed of the leg when flying away.
    public Material explosionMaterial; //Material for the explosion effect when the leg explodes.
    Vector3 m_LegOriginalScale; //Original scale of the leg.
    float m_ScaleMultiplierToFly = 1.5f; //Scale multiplier which slowly acts until the leg has reached the target position.
    private float m_LegRegrowSpeed = 1.5f; //Speed with which leg regrows to original scale
    public float colliderRadiusModifierOnFloor; //Scale modifier for the collider when the leg is on the floor, to make it easier clickable
    private float m_colliderOriginalRadius; //Original radius of the collider
    private SphereCollider m_Collider; //Collider of the leg
    public float explosionRadius = 10f;
    Camera m_Camera;
    PlayerCore core;
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
            }
        });

        m_Camera = Camera.main;
        m_Collider = GetComponent<SphereCollider>();
        m_colliderOriginalRadius = m_Collider.radius;


        m_DistanceToCore = transform.position - core.transform.position;
        m_InitialRotation = transform.rotation;
        m_LegOriginalScale = transform.localScale;
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
                //Calculate progress t of the leg flying away based on distance to target position
                float t = Vector3.Distance(transform.position, m_TargetPosition) / Vector3.Distance(m_StartingPosition, m_TargetPosition);

                //Update position to move towards target position using animation curve
                transform.position = Vector3.MoveTowards(transform.position, m_TargetPosition, flySpeedCurve.Evaluate(t) * Time.deltaTime * legFlySpeed);

                //Lerp scale by how close leg is to final position
                transform.localScale = Vector3.Lerp(transform.localScale, m_LegOriginalScale * m_ScaleMultiplierToFly, 0.1f * Time.deltaTime * legFlySpeed);

                //Lerp the collider scale to the modifier
                m_Collider.radius = Mathf.Lerp(m_Collider.radius, m_colliderOriginalRadius * colliderRadiusModifierOnFloor, t);

                //Check for arrival at target position
                if (Vector3.Distance(transform.position, m_TargetPosition) < 0.1f)
                {
                    m_LegState = LegState.DETACHED;
                }
                break;
            case LegState.DETACHED:
                break;
            case LegState.RETURNING:
                //Move the leg back to the player core using animation curve and t
                float distanceToTarget = Vector3.Distance(transform.position, core.transform.position + m_DistanceToCore);
                float totalDistance = Vector3.Distance(m_StartingPosition, core.transform.position + m_DistanceToCore);
                float tReturn = 1f - (distanceToTarget / totalDistance);

                if (tReturn < 0.90f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, core.transform.position + m_DistanceToCore, flySpeedCurve.Evaluate(tReturn) * Time.deltaTime * legFlySpeed * 2f);
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, core.transform.position + m_DistanceToCore, Time.deltaTime * legFlySpeed * 2f);
                }

                transform.rotation = Quaternion.RotateTowards(transform.rotation, m_InitialRotation, 1f * Time.deltaTime);

                if (distanceToTarget < 0.01f)
                {
                    transform.position = core.transform.position + m_DistanceToCore;
                    m_LegState = LegState.ATTACHED;
                }
                transform.localScale = Vector3.MoveTowards(transform.localScale, m_LegOriginalScale, 0.1f * Time.deltaTime * legFlySpeed * 1.5f);

                if (Vector3.Distance(transform.position, core.transform.position + m_DistanceToCore) < 0.1f)
                {
                    m_LegState = LegState.ATTACHED;
                    transform.SetParent(core.transform);
                    transform.rotation = m_InitialRotation;
                    transform.position = core.transform.position + m_DistanceToCore;
                    m_Collider.radius = m_colliderOriginalRadius;
                }
                break;
            case LegState.REGROWING:
                //Regrow the leg to original scale, when there change to attached state
                transform.localScale = Vector3.Lerp(transform.localScale, m_LegOriginalScale, m_LegRegrowSpeed * Time.deltaTime);
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
            //Leg starts waiting for mouse release to fly away and starts highlighting current mouse position, rightclick to cancel.
            m_LegState = LegState.CLICKED;
        }
    }

    public void LegReleased()
    {
        if (m_LegState == LegState.CLICKED)
        {
            //If a "safe" position is clicked while waiting for next click, start flying to the mouse position.
            m_LegState = LegState.FLYING;

            //Raycast to find the position to fly to
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit);
            m_TargetPosition = new Vector3(hit.point.x, gameObject.transform.position.y, hit.point.z); //Keep leg height
            m_StartingPosition = transform.position;

            //Reparent leg to be on the same level as the player so it doesn't move with it
            transform.SetParent(null);

            //Make the object look at the target
            transform.LookAt(m_TargetPosition);
        }
    }

    //If clicked when lying on floor or flying, explode the leg, create a sphere and destroy all enemies within the sphere
    //After that return the leg to the player and reattach it.
    private void OnMouseDown()
    {
        switch (m_LegState)
        {
            case LegState.FLYING:
            case LegState.RETURNING:
            case LegState.DETACHED:
                //Create explosion effect
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);

                //Create gizmo with explosionradius, for debugging
                GameObject gizmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                gizmo.transform.position = transform.position;
                gizmo.transform.localScale = new Vector3(explosionRadius * 2f, explosionRadius * 2f, explosionRadius * 2f);
                gizmo.GetComponent<Renderer>().material = explosionMaterial;
                gizmo.GetComponent<SphereCollider>().enabled = false;

                //Destroy after 5 seconds
                Destroy(gizmo, 5f);

                //Destroy all Enemies within the red sphere
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject.CompareTag("Enemy"))
                    {
                        Destroy(hitCollider.gameObject);
                    }
                }

                //Return the leg
                isSpinning = false;
                transform.position = core.transform.position + m_DistanceToCore;
                transform.SetParent(core.transform);
                m_LegState = LegState.REGROWING;
                transform.localScale = Vector3.zero;
                break;
        }
    }
    public void ExplodeLeg() => OnMouseDown();

    public bool isAttacking()
    {
        return isSpinning || m_LegState == LegState.FLYING || m_LegState == LegState.RETURNING;
    }
}
