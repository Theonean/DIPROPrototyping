using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.VFX;


public class DroneMovement : MonoBehaviour
{
    private enum DroneMovementState
    {
        Idle,
        Moving,
        DashingStart,
        ContinuousDash,
    }
    public static DroneMovement Instance { get; private set; }
    [Tooltip("Maximum degrees the drone can rotate per second while dashing.")]
    public float maxDegRotationInDash = 90f;
    public float distanceFromGround = 3f;
    public float dashTime = 0.3f;
    public float dashSpeed = 60f;
    public float continuousDashSpeed = 100f;
    float dashCooldown = 1f;
    float dashCooldownTimer;
    public float moveSpeed;
    public float currentSpeed;
    public Vector3 moveDirection;
    Vector3 m_CurrentVelocity;
    public AnimationCurve accelerationCurve;
    public AnimationCurve dashCurve;
    float m_AccelerationTime;
    private EventInstance movementSFXInstance;
    public string movementSFXPath; // FMOD event path
    public VisualEffect dashEffect;
    private PlayerCore playerCore;
    private PerspectiveSwitcher perspectiveSwitcher;
    [SerializeField] private Rigidbody rb;

    //State Management
    private DroneMovementState currentState = DroneMovementState.Idle;
    public bool IsIdle => currentState == DroneMovementState.Idle;
    public bool IsMoving => currentState == DroneMovementState.Moving;
    public bool IsDashing => currentState == DroneMovementState.DashingStart || currentState == DroneMovementState.ContinuousDash;
    public bool IsStartingDash => currentState == DroneMovementState.DashingStart;
    public bool IsInContinuousDash => currentState == DroneMovementState.ContinuousDash;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        FMODAudioManagement.instance.PlaySound(out movementSFXInstance, movementSFXPath, gameObject);
        playerCore = PlayerCore.Instance;
        perspectiveSwitcher = PerspectiveSwitcher.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        //No Input when dead or not in drone perspective
        if (!playerCore.isDead && perspectiveSwitcher.currentPerspective == CameraPerspective.DRONE)
        {
            if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0f)
            {
                Dash();
            }

            //Disallows steering while dashig
            if (!IsStartingDash)
            {
                Vector3 input = Vector3.zero;
                if (Input.GetKey(KeyCode.W)) input += Vector3.forward;
                if (Input.GetKey(KeyCode.S)) input += Vector3.back;
                if (Input.GetKey(KeyCode.A)) input += Vector3.left;
                if (Input.GetKey(KeyCode.D)) input += Vector3.right;

                if (input != Vector3.zero)
                {
                    moveDirection = input.normalized;
                    m_AccelerationTime += Time.deltaTime;
                    if (IsIdle)
                    {
                        currentState = DroneMovementState.Moving;
                        movementSFXInstance.setParameterByName("Movement", 1f);
                    }
                }
                else
                {
                    moveDirection = Vector3.zero;
                    rb.linearVelocity = Vector3.zero;
                    m_AccelerationTime = 0f;
                    currentState = DroneMovementState.Idle;
                    movementSFXInstance.setParameterByName("Movement", 0f);
                }
            }
        }
        else
        {
            moveDirection = Vector3.zero;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision with: " + collision.gameObject.name);
    }


    void FixedUpdate()
    {
        // Skip movement if idle
        if (IsIdle)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // Let coroutine handle velocity during dash
        if (IsDashing)
        {
            Vector3 dashVelocity = moveDirection * currentSpeed;
            rb.linearVelocity = dashVelocity;
            return;
        }

        // Normal movement logic
        currentSpeed = moveSpeed * accelerationCurve.Evaluate(m_AccelerationTime);

        if (currentSpeed == 0)
        {
            m_CurrentVelocity = Vector3.MoveTowards(m_CurrentVelocity, Vector3.zero, 0.3f);
        }
        else
        {
            Vector3 targetVelocity = moveDirection * currentSpeed;
            m_CurrentVelocity = Vector3.MoveTowards(m_CurrentVelocity, targetVelocity, currentSpeed);
        }

        rb.linearVelocity = m_CurrentVelocity;
    }

    private void Dash()
    {
        if (IsDashing || IsIdle) return; // Prevent multiple dashes at the same time
        currentState = DroneMovementState.DashingStart;

        dashCooldownTimer = dashCooldown;

        movementSFXInstance.setParameterByName("Dash", 1f);

        //Check if there are alreay enemies inside thewd players collider, knockback or damage them
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                Debug.Log("Knocked back enemy");
                StartCoroutine(hitCollider.GetComponentInParent<ACEnemyMovementBehaviour>().ApplyKnockback(moveDirection));
            }
        }

        StartCoroutine(DashMovement());
        StartDashVFX();
    }

    private IEnumerator DashMovement()
    {
        float elapsedTime = 0f;

        // Lock the dash direction
        Vector3 dashDirection = moveDirection;

        // Initial Dash
        while (elapsedTime < dashTime)
        {
            float curveValue = dashCurve.Evaluate(elapsedTime / dashTime);
            currentSpeed = dashSpeed * curveValue;
            moveDirection = dashDirection; // lock movement in that direction
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Optional: transition into Continuous Dash
        if (Input.GetKey(KeyCode.Space))
        {
            currentState = DroneMovementState.ContinuousDash;
            movementSFXInstance.setParameterByName("Dash", 1f);

            while (Input.GetKey(KeyCode.Space))
            {
                currentSpeed = continuousDashSpeed;
                yield return null;
            }
        }

        // End dash
        currentState = moveDirection != Vector3.zero ? DroneMovementState.Moving : DroneMovementState.Idle;
        movementSFXInstance.setParameterByName("Dash", 0f);
        dashEffect.Stop();
    }

    private void StartDashVFX()
    {
        dashEffect.SetVector3("PlayerPos", transform.position);
        dashEffect.Reinit();
        dashEffect.Play();
    }
}
