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
    private bool isSpinning = false; //If the leg is currently spinning (one form of attack). to see all forms of attack, go to public function isAttacking
    Vector3 m_DistanceToCore; //local position the leg spawned in, used for returning the leg to the player.
    Quaternion m_InitialRotation; //local rotation the leg spawned in, used for returning the leg to the player.
    Vector3 m_StartingPosition; //position the leg started flying from
    Vector3 m_TargetPosition; //position the leg will fly to when in FLYING state.
    GameObject m_mouseTarget; //Tracker for the mouse position when the leg is in clicked state to show the player where the leg will fly to.
    float m_LegFlySpeed = 50; //MaxSpeed of the leg when flying away.
    public AnimationCurve flySpeedCurve; //Curve for the speed of the leg when flying away.
    public Material explosionMaterial; //Material for the explosion effect when the leg explodes.
    Vector3 m_LegOriginalScale; //Original scale of the leg.
    float m_ScaleMultiplierToFly = 1.5f; //Scale multiplier which slowly acts until the leg has reached the target position.
    private float m_LegRegrowSpeed = 1.5f; //Speed with which leg regrows to original scale
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


        m_DistanceToCore = transform.position - core.transform.position;
        m_InitialRotation = transform.rotation;
        m_LegOriginalScale = transform.localScale;

        //Generate mouseTarget object
        m_mouseTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(m_mouseTarget.GetComponent<SphereCollider>());
        m_mouseTarget.GetComponent<MeshRenderer>().material.color = Color.red;
        m_mouseTarget.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        m_mouseTarget.SetActive(false);
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
                //track the mouse for the currently clicked leg with the red sphere
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    m_mouseTarget.transform.position = hit.point;
                }

                break;
            case LegState.FLYING:
                //Calculate progress t of the leg flying away based on distance to target position
                float t = Vector3.Distance(transform.position, m_TargetPosition) / Vector3.Distance(m_StartingPosition, m_TargetPosition);

                //Update position to move towards target position using animation curve
                transform.position = Vector3.MoveTowards(transform.position, m_TargetPosition, flySpeedCurve.Evaluate(t) * Time.deltaTime * m_LegFlySpeed);
                //transform.position = Vector3.MoveTowards(transform.position, m_TargetPosition, 0.1f * Time.deltaTime * m_LegFlySpeed));
                //Lerp scale by how close leg is to final position
                transform.localScale = Vector3.Lerp(transform.localScale, m_LegOriginalScale * m_ScaleMultiplierToFly, 0.1f * Time.deltaTime * m_LegFlySpeed);

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
                    transform.position = Vector3.MoveTowards(transform.position, core.transform.position + m_DistanceToCore, flySpeedCurve.Evaluate(tReturn) * Time.deltaTime * m_LegFlySpeed * 2f);
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, core.transform.position + m_DistanceToCore, Time.deltaTime * m_LegFlySpeed * 2f);
                }

                transform.rotation = Quaternion.RotateTowards(transform.rotation, m_InitialRotation, 1f * Time.deltaTime);

                if (distanceToTarget < 0.01f)
                {
                    transform.position = core.transform.position + m_DistanceToCore;
                    m_LegState = LegState.ATTACHED;
                }
                transform.localScale = Vector3.MoveTowards(transform.localScale, m_LegOriginalScale, 0.1f * Time.deltaTime * m_LegFlySpeed * 1.5f);

                if (Vector3.Distance(transform.position, core.transform.position + m_DistanceToCore) < 0.1f)
                {
                    m_LegState = LegState.ATTACHED;
                    transform.SetParent(core.transform);
                    transform.rotation = m_InitialRotation;
                    transform.position = core.transform.position + m_DistanceToCore;
                }
                break;
            case LegState.REGROWING:
                //Regrow the leg to original scale, when there change to attached state
                transform.localScale = Vector3.Lerp(transform.localScale, m_LegOriginalScale, m_LegRegrowSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.localScale, m_LegOriginalScale) < 0.1f)
                {
                    m_LegState = LegState.ATTACHED;
                    //Flash leg white for 0.2 sec after fully regrown
                    StartCoroutine(FlashLegWhite());
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
            m_mouseTarget.SetActive(true);

            //Colour mesh blue for selected
            GetComponent<MeshRenderer>().material.color = Color.blue;
        }
    }

    public void LegReleased()
    {
        if (m_LegState == LegState.CLICKED)
        {
            //Recolor to gray
            GetComponent<MeshRenderer>().material.color = Color.gray;

            //If a "safe" position is clicked while waiting for next click, start flying to the mouse position.
            m_LegState = LegState.FLYING;
            m_mouseTarget.SetActive(false);

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
            case LegState.DETACHED:
                //create a red sphere at explosion position which slowly fades away
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //Destroy collider to avoid collision with the enemies
                Destroy(sphere.GetComponent<Collider>());

                sphere.transform.position = transform.position;
                sphere.transform.localScale = new Vector3(explosionRadius * 2f, explosionRadius * 2f, explosionRadius * 2f);
                //set material to transparent
                sphere.GetComponent<Renderer>().material = explosionMaterial;

                StartCoroutine(ExplosionFadeOut(sphere));

                //Spawn a light at the explosion position
                GameObject light = new GameObject("ExplosionLight");
                light.transform.position = transform.position;
                light.AddComponent<Light>();
                light.GetComponent<Light>().color = Color.red;
                light.GetComponent<Light>().intensity = 50f;
                light.GetComponent<Light>().range = explosionRadius * 2f;

                StartCoroutine(FadeOutLight(light.GetComponent<Light>()));

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
    private IEnumerator FlashLegWhite()
    {
        float flashTime = 0.2f;
        float elapsedTime = 0f;
        Material material = GetComponent<MeshRenderer>().material;
        Color startColor = Color.white;
        Color endColor = material.color;
        Color currentColor = startColor;
        while (elapsedTime < flashTime)
        {
            float t = elapsedTime / flashTime;
            currentColor = Color.Lerp(startColor, endColor, t);
            material.color = currentColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        material.color = endColor;
        transform.localScale = m_LegOriginalScale;
    }

    private IEnumerator ExplosionFadeOut(GameObject sphere)
    {
        float fadeTime = 1f;
        float elapsedTime = 0f;
        Material material = sphere.GetComponent<Renderer>().material;
        Color startColor = new Color(1f, 0f, 0f, 1f);
        Color endColor = new Color(1f, 0f, 0f, 0f);

        while (elapsedTime < fadeTime)
        {
            float t = elapsedTime / fadeTime;
            material.color = Color.Lerp(startColor, endColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        material.color = endColor;
        Destroy(sphere);
    }

    private IEnumerator FadeOutLight(Light light)
    {
        float fadeTime = 0.2f;
        float elapsedTime = 0f;
        Color startColor = light.color;
        Color endColor = new Color(1f, 0.8f, 0f, 0f);

        while (elapsedTime < fadeTime)
        {
            float t = elapsedTime / fadeTime;
            light.color = Color.Lerp(startColor, endColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        light.color = endColor;
        Destroy(light.gameObject);
    }

    public bool isAttacking()
    {
        return isSpinning || m_LegState == LegState.FLYING || m_LegState == LegState.RETURNING;
    }
}
