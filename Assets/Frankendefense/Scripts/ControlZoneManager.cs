using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum ZoneState
{
    MOVING, //Moving from resource point to resource point
    HARVESTING, //"Gathering" Resources and vulnerable to enemy attacks
    DIED //Dead, no longer doing anything
}

public class ControlZoneManager : MonoBehaviour
{
    [Tooltip("Hits needed until this object signals death")]
    public int health;
    public Slider healthSlider;
    public UnityEvent died;
    float m_WaveTime = 30f;
    float m_MoveSpeed = 10f;
    public GameObject MapBoundaries;
    Vector3[] m_BoundaryPositions;
    Vector3 m_TargetPosition;
    ZoneState m_ZoneState = ZoneState.MOVING;
    float m_m_WaveTimer = 0f;
    public Slider waveProgressSlider;
    public UnityEvent<ZoneState> changedState;
    Color m_OriginalColor;
    public GameObject resourcePoint;

    void Start()
    {
        healthSlider.maxValue = health;
        healthSlider.value = health;
        m_OriginalColor = GetComponent<Renderer>().material.color;

        m_m_WaveTimer = m_WaveTime;

        waveProgressSlider.maxValue = m_WaveTime;
        waveProgressSlider.value = m_WaveTime;


        //Load the two corners of the map boundaries
        m_BoundaryPositions = new Vector3[2];
        m_BoundaryPositions[0] = MapBoundaries.transform.GetChild(0).position;
        m_BoundaryPositions[1] = MapBoundaries.transform.GetChild(1).position;

        CalculateTargetPosition();
    }

    private void Update()
    {
        //Decrease timer until done, after that start moving again to a new resource point position
        if (m_ZoneState == ZoneState.HARVESTING)
        {
            //Show wave progress while harvesting
            waveProgressSlider.enabled = true;

            m_m_WaveTimer -= Time.deltaTime;
            waveProgressSlider.value = m_m_WaveTimer;
            if (m_m_WaveTimer <= 0f)
            {
                m_m_WaveTimer = m_WaveTime;
                m_ZoneState = ZoneState.MOVING;
                CalculateTargetPosition();
                changedState.Invoke(m_ZoneState);
                Debug.Log("Finished Harvesting, moving to new position");
            }
        }
        else if (m_ZoneState == ZoneState.MOVING)
        {
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
                m_ZoneState = ZoneState.HARVESTING;
                changedState.Invoke(m_ZoneState);
                Debug.Log("Arrived at position, starting to harvest");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            health--;
            healthSlider.value = health;
            Destroy(other.gameObject);

            StartCoroutine(TakeDamageEffect());
            if (health <= 0)
            {
                m_ZoneState = ZoneState.DIED;
                died.Invoke();
            }
        }
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
        //Move the zone to a random position within the map boundaries
        Vector3 newPosition = new Vector3(
            Random.Range(m_BoundaryPositions[0].x, m_BoundaryPositions[1].x),
            Random.Range(m_BoundaryPositions[0].y, m_BoundaryPositions[1].y),
            Random.Range(m_BoundaryPositions[0].z, m_BoundaryPositions[1].z)
        );

        m_TargetPosition = newPosition;
        resourcePoint.transform.position = m_TargetPosition;
    }
}