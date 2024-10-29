using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public enum ZoneState
{
    MOVING, //Moving from resource point to resource point
    START_HARVESTING, //Starting to harvest resources, animation
    HARVESTING, //"Gathering" Resources and vulnerable to enemy attacks
    END_HARVESTING, //Ending harvest, animation
    DIED //Dead, no longer doing anything
}

public enum Direction
{
    up,
    down,
    left,
    right
}

public class ControlZoneManager : MonoBehaviour
{

    public static ControlZoneManager Instance { get; private set; }

    [Tooltip("Hits needed until this object signals death")]
    public int maxHealth;
    private int m_Health;
    public List<Slider> m_HealthSliders = new List<Slider>();
    public UnityEvent died;
    public float waveTime = 30f;
    public float moveSpeed = 4f;
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
    public Direction pathAlignment = Direction.up;
    public int initialPathPositions = 20;
    public int pathAngle = 35;
    private int pathPositionsIndex = 0;
    public Vector3[] pathPositions;



    //Animator stuff
    private Animator m_HarvesterAnimator;
    private static readonly int m_StartHarvesting = Animator.StringToHash("Start_Harvesting");
    private static readonly int m_Harvesting = Animator.StringToHash("Harvesting");
    private static readonly int m_StopHarvesting = Animator.StringToHash("Stop_Harvesting");
    private static readonly int m_Idle = Animator.StringToHash("Idle");

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

        PopulatePathList(false);
    }

    void Start()
    {
        m_Health = maxHealth;
        foreach (Slider slider in m_HealthSliders)
        {
            slider.maxValue = maxHealth;
            slider.value = maxHealth;
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

        //CalculateTargetPosition();
        SetNextPathPosition();
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
            Vector3 newPosition = Vector3.MoveTowards(transform.position, offsetPosition, moveSpeed * Time.deltaTime);

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
                //CalculateTargetPosition();
                SetNextPathPosition();
                StartCoroutine(ReduceWaveTimerOverTimeIDontKnowHowToNameThis(1f));
                m_HarvesterAnimator.Play(m_Idle, 0, 0f);
                changedState.Invoke(m_ZoneState);
                Debug.Log("Finished Harvesting, moving to new position");
            }
        }
    }

    //Create a List of Vector3 which is the path the control zone is going to move from, wave to wave
    //Control zone can move in either of cardinal directions depending on setup direction in edtior ( or random)
    public void PopulatePathList(bool randomDirection)
    {
        pathPositions = new Vector3[initialPathPositions];
        pathPositions[0] = Vector3.zero;

        Vector3 pathDirection = Vector3.zero;

        //If random direction, calculate one within given allowed path angle
        if (randomDirection)
        {

        }
        //Go straight in the given direction otherwise
        else
        {
            switch (pathAlignment)
            {
                case Direction.up:
                    pathDirection = Vector3.forward;
                    break;

                case Direction.down:
                    pathDirection = Vector3.back;
                    break;

                case Direction.left:
                    pathDirection = Vector3.left;
                    break;

                case Direction.right:
                    pathDirection = Vector3.right;
                    break;
            }
        }

        Vector3 lastPathPosition = transform.position;
        for (int i = 1; i < initialPathPositions; i++)
        {
            //Each points direction has to be calculated
            Vector3 tempPos = CalculatePathPosition(lastPathPosition, pathDirection);
            pathPositions[i] = tempPos;
            lastPathPosition = tempPos;
        }

        //Debug path by printing it to console and drawing a debug gizmo on each of the points
        int d = 4;
        int id = 0;
        foreach (Vector3 position in pathPositions)
        {
            Debug.DrawLine(position, position + Vector3.up * 200f, id % d == 0 ? Color.black : Color.yellow, 1000f);
            id++;
        }

        // Debug path positions as a formatted string
        string pathPositionsString = string.Join(", ", pathPositions.Select(p => p.ToString()));
        Debug.Log($"Control Zone Path Positions: {pathPositionsString}");
    }

    private void SetNextPathPosition()
    {
        //Calculate the next path position based on the current position and the path direction
        Vector3 nextPathPosition = pathPositions[pathPositionsIndex];
        m_TargetPosition = nextPathPosition;
        resourcePoint.transform.position = nextPathPosition;

        //calculate travel time for the new position
        travelTimeLeft = Vector3.Distance(transform.position, nextPathPosition) / moveSpeed;

        pathPositionsIndex++;
    }

    public float getDistanceAlongPathFromPoint(Index pathIndex)
    {
        //If the direction is up or down, return the z distance
        if (pathAlignment == Direction.up || pathAlignment == Direction.down)
        {
            return pathPositions[pathIndex].z;
        }
        //If the direction is left or right, return the x distance
        else 
        {
            return pathPositions[pathIndex].x;
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Modifym_Health(-1);
            Destroy(other.gameObject);
            StartCoroutine(TakeDamageEffect());
        }
    }

    void Modifym_Health(int amount)
    {
        m_Health += amount;
        foreach (Slider slider in m_HealthSliders)
        {
            slider.value = m_Health;
        }

        if (m_Health <= 0)
        {
            m_ZoneState = ZoneState.DIED;
            died.Invoke();
            StartCoroutine(FlyAway());
        }
    }

    public void FullHeal()
    {
        int m_HealthNeededToFull = maxHealth - m_Health;
        Modifym_Health(m_HealthNeededToFull);
    }

    public void Die()
    {
        Modifym_Health(-m_Health);
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

    Vector3 CalculatePathPosition(Vector3 startPosition, Vector3 direction)
    {
        // Calculate a random distance between 20 and 30 seconds of travel
        float minDistance = moveSpeed * minTravelTime;
        float maxDistance = moveSpeed * maxTravelTime;
        float randomDistance = UnityEngine.Random.Range(minDistance, maxDistance);

        // Calculate the new position
        Vector3 newPosition = startPosition + (direction * randomDistance);

        return newPosition;
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

    public float getPathDistance()
    {
        return Vector3.Distance(pathPositions[0], pathPositions[pathPositions.Length - 1]);
    }
}
