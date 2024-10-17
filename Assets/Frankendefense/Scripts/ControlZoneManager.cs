using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

public enum ZoneState
{
    MOVING, //Moving from resource point to resource point
    START_HARVESTING, //Starting to harvest resources, animation
    HARVESTING, //"Gathering" Resources and vulnerable to enemy attacks
    END_HARVESTING, //Ending harvest, animation
    DIED //Dead, no longer doing anything
}

public class ControlZoneManager : MonoBehaviour
{

    public static ControlZoneManager Instance { get; private set; }

    [Tooltip("Hits needed until this object signals death")]
    public int health;
    public List<Slider> healthSliders = new List<Slider>();
    public UnityEvent died;
    public float waveTime = 30f;
    float m_MoveSpeed = 4f;
    public GameObject MapBoundaries;
    Vector3[] m_BoundaryPositions;
    Vector3 m_TargetPosition;
    ZoneState m_ZoneState = ZoneState.MOVING;
    float m_WaveTimer = 0f;
    public Slider waveProgressSlider;
    public UnityEvent<ZoneState> changedState;
    Color m_OriginalColor;
    public GameObject resourcePoint;
    public float minTravelTime = 20f;
    public float maxTravelTime = 30f;
    public float travelTimeLeft;
    LineRenderer m_LineRenderer;

    //Animator stuff
    private Animator m_HarvesterAnimator;
    private static readonly int m_StartHarvesting = Animator.StringToHash("Convoy|Start_Harvesting");
    private static readonly int m_Harvesting = Animator.StringToHash("Convoy|Harvesting");
    private static readonly int m_StopHarvesting = Animator.StringToHash("Convoy|Stop_Harvesting");
    private static readonly int m_Idle = Animator.StringToHash("Convoy|Idle");

    private void Awake()
    {
        // Ensure there's only one instance of the FrankenGameManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        foreach (Slider slider in healthSliders)
        {
            slider.maxValue = health;
            slider.value = health;
        }

        m_OriginalColor = GetComponent<Renderer>().material.color;
        m_LineRenderer = GetComponent<LineRenderer>();

        m_HarvesterAnimator = GetComponentInChildren<Animator>();

        m_WaveTimer = 0f;

        waveProgressSlider.maxValue = waveTime;
        waveProgressSlider.value = 0f;


        //Load the two corners of the map boundaries
        m_BoundaryPositions = new Vector3[2];
        m_BoundaryPositions[0] = MapBoundaries.transform.GetChild(0).position;
        m_BoundaryPositions[1] = MapBoundaries.transform.GetChild(1).position;

        CalculateTargetPosition();
        changedState.Invoke(m_ZoneState);
    }

    private void Update()
    {
        //Decrease timer until done, after that start moving again to a new resource point position
        if (m_ZoneState == ZoneState.HARVESTING)
        {
            //Show wave progress while harvesting
            waveProgressSlider.enabled = true;

            m_WaveTimer += Time.deltaTime;
            waveProgressSlider.value = m_WaveTimer;
            if (m_WaveTimer >= waveTime)
            {
                m_WaveTimer = 0f;
                m_ZoneState = ZoneState.END_HARVESTING;
                changedState.Invoke(m_ZoneState);
                m_HarvesterAnimator.Play(m_StopHarvesting, 0, 0f);
                Debug.Log("Finished Harvesting, starting end harvest animation");
            }
        }
        else if (m_ZoneState == ZoneState.MOVING)
        {
            travelTimeLeft -= Time.deltaTime;
            if (travelTimeLeft <= 0)
                Debug.Log("Harvester should have arrived?");


            DrawLineToTarget();

            //Hide wave progress while harvesting
            waveProgressSlider.enabled = false;

            // Calculate the sinus wave offset
            float sinOffset = Mathf.Sin(Time.time * 2f) * 0.5f;
            Vector3 offsetPosition = m_TargetPosition + transform.right * sinOffset;

            // Move towards the target position with sinus wave
            Vector3 newPosition = Vector3.MoveTowards(transform.position, offsetPosition, m_MoveSpeed * Time.deltaTime);

            // Calculate the movement direction
            Vector3 moveDirection = (newPosition - transform.position).normalized;

            // Update position
            transform.position = newPosition;

            // Rotate to face the movement direction
            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }

            //If we are close enough to the target position, change state to harvesting
            if (Vector3.Distance(transform.position, m_TargetPosition) < 0.5f)
            {
                m_ZoneState = ZoneState.START_HARVESTING;
                m_HarvesterAnimator.Play(m_StartHarvesting, 0, 0f);
                changedState.Invoke(m_ZoneState);
                Debug.Log("Arrived at position, starting to harvest");
            }
        }
        else if (m_ZoneState == ZoneState.START_HARVESTING)
        {
            if (m_HarvesterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                m_ZoneState = ZoneState.HARVESTING;
                CalculateTargetPosition();
                StartCoroutine(ReduceWaveTimerOverTimeIDontKnowHowToNameThis(1f));
                m_HarvesterAnimator.Play(m_Harvesting, 0, 0f);
                changedState.Invoke(m_ZoneState);
                Debug.Log("Started Harvesting");
            }
        }
        else if (m_ZoneState == ZoneState.END_HARVESTING)
        {
            if (m_HarvesterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                m_ZoneState = ZoneState.MOVING;
                m_HarvesterAnimator.Play(m_Idle, 0, 0f);
                changedState.Invoke(m_ZoneState);
                Debug.Log("Finished Harvesting, moving to new position");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            ModifyHealth(-1);
            Destroy(other.gameObject);
            StartCoroutine(TakeDamageEffect());
        }
    }

    void ModifyHealth(int amount)
    {
        health += amount;
        foreach (Slider slider in healthSliders)
        {
            slider.value = health;
        }

        if (health <= 0)
        {
            m_ZoneState = ZoneState.DIED;
            died.Invoke();
            StartCoroutine(FlyAway());
        }
    }

    public void FullHeal()
    {
        int healthNeededToFull = 10 - health;
        ModifyHealth(healthNeededToFull);
    }

    IEnumerator FlyAway()
    {
        float timer = 0f;
        AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 2f, 1f);
        while (timer < 4f)
        {
            transform.position += Vector3.up * 10f * Time.deltaTime * curve.Evaluate(timer);
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    //Turn material red and quickly fade back to normal
    IEnumerator TakeDamageEffect()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = Color.red;
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            renderer.material.color = Color.Lerp(Color.red, m_OriginalColor, t);
            yield return null;
        }
        renderer.material.color = m_OriginalColor;
    }

    void CalculateTargetPosition()
    {
        bool legalPosition = false;

        while (!legalPosition)
        {
            // Calculate a random direction
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.y = 0; // Keep it on the same y-level
            randomDirection.Normalize();

            // Calculate a random distance between 20 and 30 seconds of travel
            float travelSpeed = m_MoveSpeed;
            float minDistance = travelSpeed * minTravelTime;
            float maxDistance = travelSpeed * maxTravelTime;
            float randomDistance = Random.Range(minDistance, maxDistance);

            // Calculate the new position
            Vector3 newPosition = transform.position + (randomDirection * randomDistance);

            //Check if position is reachable reachable for navmesh
            NavMesh.SamplePosition(newPosition, out NavMeshHit hit, 100f, NavMesh.AllAreas);
            if (hit.hit)
            {
                legalPosition = true;
            }

            //calculate travel time for the new position
            travelTimeLeft = Vector3.Distance(transform.position, newPosition) / travelSpeed;

            m_TargetPosition = newPosition;
        }

        resourcePoint.transform.position = m_TargetPosition;
    }

    IEnumerator ReduceWaveTimerOverTimeIDontKnowHowToNameThis(float time)
    {
        float timer = 0f;
        while (timer < time)
        {
            timer += Time.deltaTime;

            //Lerp wave timer slider to 0 over given time
            waveProgressSlider.value = Mathf.Lerp(waveTime, 0f, timer / time);
            yield return null;
        }
    }

    void DrawLineToTarget()
    {
        if (m_TargetPosition != Vector3.zero && m_LineRenderer != null)
        {
            m_LineRenderer.SetPosition(0, m_TargetPosition);
            m_LineRenderer.SetPosition(1, transform.position);
        }
    }
}
