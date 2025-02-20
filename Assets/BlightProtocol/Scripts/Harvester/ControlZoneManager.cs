using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.VFX;
using Unity.AI.Navigation;
using UnityEngine.ProBuilder.Shapes;
using FMOD.Studio;
using FMODUnity;

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
    public int maxHealth;
    public int health;
    public NavMeshSurface navMeshSurface;
    public List<Slider> healthSliders = new List<Slider>();
    public UnityEvent died;
    public float waveTime = 30f;
    public static float moveSpeed = 5f;
    Vector3 m_TargetPosition;
    ZoneState m_ZoneState = ZoneState.MOVING;
    float m_WaveTimer = 0f;
    public Slider waveProgressSlider;
    public UnityEvent<ZoneState> changedState;
    Color m_OriginalColor;
    public GameObject resourcePoint;
    public float travelTimeLeft;
    LineRenderer m_LineRenderer;
    public int pathPositionsIndex = 0;
    public Vector3[] pathPositions;
    [SerializeField]
    private GameObject m_carrierBalloon;



    //Animator stuff
    private Animator m_HarvesterAnimator;
    private static readonly int m_StartHarvesting = Animator.StringToHash("Start_Harvesting");
    private static readonly int m_Harvesting = Animator.StringToHash("Harvesting");
    private static readonly int m_StopHarvesting = Animator.StringToHash("Stop_Harvesting");
    private static readonly int m_Idle = Animator.StringToHash("Idle");

    // Resource Point Color
    public float resoucePointColorCorrection = 1f;

    // VFX
    public VisualEffect drillingVFX;

    [Header("SFX")]
    public string harvestingSFXPath = "event:/...";
    private EventInstance m_HarvestingSFX;
    public StudioEventEmitter movingSFX;
    public string enemyImpactSFXPath = "event:/...";
    public string takeoffSFXPath = "event:/...";

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
        health = maxHealth;
        pathPositions = ProceduralTileGenerator.Instance.GetPath();
        foreach (Slider slider in healthSliders)
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

                // stop VFX
                drillingVFX.Stop();

                //Set audio Cue for Harvester to pack up
                m_HarvestingSFX.keyOff();
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
                //Set Audio states
                FMODAudioManagement.instance.PlaySound(out m_HarvestingSFX, harvestingSFXPath, gameObject);
                movingSFX.EventInstance.setPaused(true);
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

                // start VFX
                drillingVFX.Play();
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

                //Set Audio states
                movingSFX.EventInstance.setPaused(false);
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            //Set Audio states
            FMODAudioManagement.instance.PlayOneShot(enemyImpactSFXPath, other.gameObject.transform.position);

            Modifyhealth(-1);
            other.gameObject.GetComponent<EnemyDamageHandler>().DestroyEnemy();
            StartCoroutine(TakeDamageEffect());
        }
    }

    private void SetNextPathPosition()
    {
        m_TargetPosition = pathPositions[pathPositionsIndex];
        resourcePoint.transform.position = m_TargetPosition;
        travelTimeLeft = Vector3.Distance(transform.position, m_TargetPosition) / moveSpeed;
        pathPositionsIndex = pathPositionsIndex + 1;

        //Move Navmesh transform forward by moving it along the path
        Debug.LogError("Fix this");
        //navMeshSurface.transform.position = new Vector3(0, 0, 0);// pathPositionsIndex * ProceduralTileGenerator.Instance.tileSize);

        // set resource point color
        SetMeshColoursToRegion();
    }

    public void SetMeshColoursToRegion()
    {
        // Calculate the color based on the obstacle's position along the path
        float zPosition = resourcePoint.transform.position.z;
        Color regionColor = ProceduralTileGenerator.Instance.GetColorForPosition(zPosition);

        // Use MaterialPropertyBlock to set color without affecting shared materials
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        MeshRenderer meshRenderer = resourcePoint.GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", regionColor * resoucePointColorCorrection);

            Color shadowColor1 = meshRenderer.material.GetColor("_1st_ShadeColor");
            Color shadowColor2 = meshRenderer.material.GetColor("_2nd_ShadeColor");

            propBlock.SetColor("_1st_ShadeColor", regionColor * shadowColor1);
            propBlock.SetColor("_2nd_ShadeColor", regionColor * shadowColor2);

            meshRenderer.SetPropertyBlock(propBlock);


        }
    }

    void Modifyhealth(int amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);

        foreach (Slider slider in healthSliders)
        {
            slider.value = health;
        }

        if (health <= 0 && m_ZoneState != ZoneState.DIED)
        {
            m_ZoneState = ZoneState.DIED;
            Debug.Log("Harvester Died");
            died.Invoke();
            //Find Camera Tracker and tell it to track this
            CameraTracker.Instance.objectToTrack = this.gameObject;
            StartCoroutine(FlyAway());

            //Set Audio states
            FMODAudioManagement.instance.PlayOneShot(takeoffSFXPath, transform.position);
        }
    }

    public void Heal()
    {
        Modifyhealth(1);
    }

    IEnumerator FlyAway()
    {
        Animator anim = m_carrierBalloon.GetComponent<Animator>();
        m_carrierBalloon.SetActive(true);

        //While the animator is still playing, wait, after that continue to the fly away code-animation
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        float timer = 0f;
        AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 2f, 1f);
        while (timer < 10f)
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

    public ZoneState GetZoneState()
    {
        return m_ZoneState;
    }
}
