using System.Collections;
using UnityEngine;

public enum LegState
{
    ATTACHED,
    CLICKED,
    FLYING,
    DETACHED,
    RETURNING,
    REGROWING
}

public class LegHandler : MonoBehaviour
{
    public LegState m_LegState = LegState.ATTACHED;
    private bool isSpinning = false; //If the leg is currently spinning (one form of attack).
    Vector3 m_StartingPosition; //position the leg started flying from
    Vector3 m_TargetPosition; //position the leg will fly to when in FLYING state.
    float m_LegFlySpeed = 50; //MaxSpeed of the leg when flying away.
    public AnimationCurve flySpeedCurve; //Curve for the speed of the leg when flying away.
    public Material explosionMaterial; //Material for the explosion effect when the leg explodes.
    Vector3 m_LegOriginalScale; //Original scale of the leg.
    float m_ScaleMultiplierToFly = 1.5f; //Scale multiplier for flying.
    private float m_LegRegrowSpeed = 1.5f; //Speed with which leg regrows to original scale
    public float explosionRadius = 10f;
    Camera m_Camera;
    PlayerCore core;

    Transform m_InitialTransform; // Transform to hold the initial position and rotation.

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

        // Create a new GameObject to hold the initial transform
        GameObject initialTransformHolder = new GameObject($"{gameObject.name}_InitialTransform");
        initialTransformHolder.transform.SetParent(core.transform);
        initialTransformHolder.transform.position = transform.position;
        initialTransformHolder.transform.rotation = transform.rotation;

        m_InitialTransform = initialTransformHolder.transform;

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
                float t = Vector3.Distance(transform.position, m_TargetPosition) / Vector3.Distance(m_StartingPosition, m_TargetPosition);
                transform.position = Vector3.MoveTowards(transform.position, m_TargetPosition, flySpeedCurve.Evaluate(t) * Time.deltaTime * m_LegFlySpeed);
                transform.localScale = Vector3.Lerp(transform.localScale, m_LegOriginalScale * m_ScaleMultiplierToFly, 0.1f * Time.deltaTime * m_LegFlySpeed);

                if (Vector3.Distance(transform.position, m_TargetPosition) < 0.1f)
                {
                    m_LegState = LegState.DETACHED;
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
                    transform.position = Vector3.MoveTowards(transform.position, updatedTargetPosition, flySpeedCurve.Evaluate(tReturn) * Time.deltaTime * m_LegFlySpeed * 2f);
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, updatedTargetPosition, Time.deltaTime * m_LegFlySpeed * 2f);
                }

                // Update the rotation of the leg to smoothly return to its initial rotation
                transform.rotation = Quaternion.RotateTowards(transform.rotation, m_InitialTransform.rotation, 1f * Time.deltaTime);

                if (distanceToUpdatedTarget < 0.03f)
                {
                    transform.position = updatedTargetPosition;
                    m_LegState = LegState.ATTACHED;
                    transform.SetParent(core.transform);
                    transform.rotation = m_InitialTransform.rotation;
                }

                // Gradually return the leg's scale back to its original size
                transform.localScale = Vector3.MoveTowards(transform.localScale, m_LegOriginalScale, 0.1f * Time.deltaTime * m_LegFlySpeed * 1.5f);
                break;
            case LegState.REGROWING:
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
            m_LegState = LegState.CLICKED;
        }
    }

    public void LegReleased()
    {
        if (m_LegState == LegState.CLICKED)
        {
            m_LegState = LegState.FLYING;
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit);
            m_TargetPosition = new Vector3(hit.point.x, gameObject.transform.position.y, hit.point.z); //Keep leg height
            m_StartingPosition = transform.position;
            transform.SetParent(null);
            transform.LookAt(m_TargetPosition);
        }
    }

    private void OnMouseDown()
    {
        switch (m_LegState)
        {
            case LegState.FLYING:
            case LegState.RETURNING:
            case LegState.DETACHED:
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(sphere.GetComponent<Collider>());
                sphere.transform.position = transform.position;
                sphere.transform.localScale = new Vector3(explosionRadius * 2f, explosionRadius * 2f, explosionRadius * 2f);
                sphere.GetComponent<Renderer>().material = explosionMaterial;

                StartCoroutine(ExplosionFadeOut(sphere));

                GameObject light = new GameObject("ExplosionLight");
                light.transform.position = transform.position;
                light.AddComponent<Light>().color = Color.red;
                light.GetComponent<Light>().intensity = 50f;
                light.GetComponent<Light>().range = explosionRadius * 2f;

                StartCoroutine(FadeOutLight(light.GetComponent<Light>()));

                Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject.CompareTag("Enemy"))
                    {
                        Destroy(hitCollider.gameObject);
                    }
                }

                isSpinning = false;
                transform.position = m_InitialTransform.position;
                transform.rotation = m_InitialTransform.rotation;
                transform.SetParent(core.transform);
                m_LegState = LegState.REGROWING;
                transform.localScale = Vector3.zero;
                break;
        }
    }

    public void ExplodeLeg() => OnMouseDown();

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
