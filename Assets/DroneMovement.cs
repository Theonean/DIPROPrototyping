using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.VFX;

public class DroneMovement : MonoBehaviour
{
    public static DroneMovement Instance { get; private set; }
    public bool isDashing = false;
    public float dashKnockback = 150f;
    float dashTime = 0.3f;
    float dashSpeed = 60f;
    float dashCooldown = 1f;
    float dashCooldownTimer;
    private bool isStandingStill = true;
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
            if (!isDashing)
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
                    if (isStandingStill)
                    {
                        isStandingStill = false;
                        movementSFXInstance.setParameterByName("Movement", 1f);
                    }
                }
                else
                {
                    moveDirection = Vector3.zero;
                    m_AccelerationTime = 0f;
                    isStandingStill = true;
                    movementSFXInstance.setParameterByName("Movement", 0f);
                }
            }
        }
        else
        {
            moveDirection = Vector3.zero;
            m_AccelerationTime = 0f;
            isStandingStill = true;
            movementSFXInstance.setParameterByName("Movement", 0f);
        }
    }


    void FixedUpdate()
    {
        if (!isDashing)
        {
            currentSpeed = moveSpeed * accelerationCurve.Evaluate(m_AccelerationTime);

            //If current speed is 0, start applying drag
            if (currentSpeed == 0)
            {
                m_CurrentVelocity = Vector3.MoveTowards(m_CurrentVelocity, Vector3.zero, 0.3f);
            }
            //Otherwise move towards the target velocity based on the current speed
            else
            {
                Vector3 targetVelocity = moveDirection * currentSpeed;
                m_CurrentVelocity = Vector3.MoveTowards(m_CurrentVelocity, targetVelocity, currentSpeed);
            }
            transform.position += m_CurrentVelocity * Time.fixedDeltaTime;
        }
    }


    private void Dash()
    {
        if (isDashing || moveDirection == Vector3.zero) return; // Prevent multiple dashes at the same time

        dashCooldownTimer = dashCooldown;
        isDashing = true;

        movementSFXInstance.setParameterByName("Dash", 1f);

        //Check if there are alreay enemies inside thewd players collider, knockback or damage them
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                Debug.Log("Knocked back enemy");
                StartCoroutine(hitCollider.GetComponentInParent<ACEnemyMovementBehaviour>().ApplyKnockback(moveDirection, dashKnockback));
            }
        }

        StartCoroutine(DashMovement());

        //Trigger Dash Effect here
        StartDashVFX();
    }

    private IEnumerator DashMovement()
    {
        float elapsedTime = 0f;

        while (elapsedTime < dashTime)
        {
            // Move the player in the current move direction at the specified dash speed
            transform.position += moveDirection * dashSpeed * Time.deltaTime * dashCurve.Evaluate(elapsedTime / dashTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        movementSFXInstance.setParameterByName("Dash", 0f);
        isDashing = false;
        StopDashVFX();
    }


    private void StartDashVFX()
    {
        dashEffect.SetVector3("PlayerPos", transform.position);
        dashEffect.Reinit();
        dashEffect.Play();
    }

    private void StopDashVFX()
    {
        dashEffect.Stop();
    }

}
