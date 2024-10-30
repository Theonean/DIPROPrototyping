using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class PlayerCore : MonoBehaviour
{
    public bool DashDoesDamage; //true = dash damages enemies, false = dash knocks enemies back
    bool m_IsDashing = false;
    float m_DashKnockback = 150f;
    float m_DashTime = 0.3f;
    float m_DashSpeed = 60f;
    float m_DashCooldown = 1f;
    float m_DashCooldownTimer;
    public float moveSpeed;
    public float currentSpeed;
    public float maxHealth; //Number of hits drone can take until it dies
    float m_Health; //Number of hits drone can take until it dies
    public float respawnTime; //Time until drone respawns
    public GameObject shield;
    public float shieldRespawnCooldown;
    private float m_ShieldRespawnTimer;
    private bool m_RegenShield = false;

    float m_RespawnTimer;
    public UnityEvent returnLegs;
    public Vector3 moveDirection;
    Vector3 m_CurrentVelocity;
    public AnimationCurve accelerationCurve;
    public AnimationCurve dashCurve;
    float m_AccelerationTime;
    bool m_IsDead = false; //When dead, track the ControlZoneManager to respawn the drone

    MeshRenderer m_Renderer;
    private LegHandler[] m_Legs = new LegHandler[4];

    [Header("VFX")]
    public VisualEffect dashEffect;

    private void Awake()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        m_Renderer.material.color = Color.white;
        m_Health = maxHealth;
        m_RespawnTimer = 0f;

        m_Legs = GetComponentsInChildren<LegHandler>();
    }

    void Update()
    {
        if (m_DashCooldownTimer > 0f)
        {
            m_DashCooldownTimer -= Time.deltaTime;
        }

        if (m_RegenShield)
        {
            m_ShieldRespawnTimer -= Time.deltaTime;
            if (m_ShieldRespawnTimer <= 0f)
            {
                ModifyHealth(+1);
                Debug.Log("Shield regenerated");
            }
        }

        //No Input when dead
        if (!m_IsDead)
        {
            if (Input.GetMouseButtonDown(1))
            {
                returnLegs.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Space) && m_DashCooldownTimer <= 0f)
            {
                Dash();
            }

            //Disallows steering while dashig
            if (!m_IsDashing)
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
                }
                else
                {
                    moveDirection = Vector3.zero;
                    m_AccelerationTime = 0f;
                }
            }
        }

        //Count down respawn timer and respawn drone when player has no health
        if (m_IsDead)
        {
            m_RespawnTimer += Time.deltaTime;
            transform.position = FindObjectOfType<ControlZoneManager>().transform.position;

            if (m_RespawnTimer >= respawnTime)
            {
                m_RespawnTimer = 0f;
                m_Health = maxHealth;
                m_Renderer.material.color = Color.white;
                m_IsDead = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (!m_IsDashing)
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

    //When the player collides with an object, check if the object is an enemy and apply knockback or damage when dashing
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (m_IsDashing)
            {
                if (DashDoesDamage)
                {
                    // Damage the enemy
                    Debug.Log("Dealt damage to enemy");
                    Destroy(other.gameObject);
                }
                else
                {
                    // Knockback the enemy
                    Debug.Log("Knocked back enemy");
                    StartCoroutine(other.GetComponentInParent<FollowPlayer>().ApplyKnockback(moveDirection, m_DashKnockback));
                }
            }
            else if (!m_IsDead) //Only take damage when not dead
            {
                Destroy(other.gameObject);

                // Player is not dashing, so take damage
                ModifyHealth(-1);
            }
        }
    }

    private void Dash()
    {
        if (m_IsDashing) return; // Prevent multiple dashes at the same time

        m_DashCooldownTimer = m_DashCooldown;
        m_IsDashing = true;
        m_Renderer.material.color = Color.red; // Change color to indicate dash

        //Check if there are alreay enemies inside thewd players collider, knockback or damage them
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                if (DashDoesDamage)
                {
                    // Damage the enemy
                    Debug.Log("Dealt damage to enemy");
                    Destroy(hitCollider.gameObject);
                }
                else
                {
                    // Knockback the enemy
                    Debug.Log("Knocked back enemy");
                    StartCoroutine(hitCollider.GetComponentInParent<FollowPlayer>().ApplyKnockback(moveDirection, m_DashKnockback));
                }
            }
        }

        StartCoroutine(DashMovement());
        
        //Trigger Dash Effect here
        StartDashVFX();
    }

    public void ModifyHealth(int amount)
    {
        // Update the shield status
        if (amount > 0)
        {
            m_RegenShield = false;
            shield.GetComponent<ShieldVFX>().ToggleShield(true);
        }
        else if (amount < 0)
        {
            m_RegenShield = true;
            shield.GetComponent<ShieldVFX>().ToggleShield(false);
            m_ShieldRespawnTimer = shieldRespawnCooldown;
        }

        //Change health
        m_Health += amount;
        m_Health = Mathf.Clamp(m_Health, 0, maxHealth);

        //Check if dead
        if (m_Health <= 0)
        {
            //Make drone invisible when dead
            m_Renderer.material.color = new Color(1, 1, 1, 0);
            transform.position = FindObjectOfType<ControlZoneManager>().transform.position;
            m_IsDead = true;
            m_RespawnTimer = 0f;
        }

    }

    private IEnumerator DashMovement()
    {
        float elapsedTime = 0f;

        while (elapsedTime < m_DashTime)
        {
            // Move the player in the current move direction at the specified dash speed
            transform.position += moveDirection * m_DashSpeed * Time.deltaTime * dashCurve.Evaluate(elapsedTime / m_DashTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        m_IsDashing = false;
        m_Renderer.material.color = Color.white; // Reset color after dash
        StopDashVFX();
    }

    public void IncreaseLegExplosionRadius(float radiusIncrease)
    {
        //Increase explosion radius on all legs
        foreach (var leg in m_Legs)
        {
            leg.explosionRadius += radiusIncrease;
        }
    }

    public void IncreaseLegShotSpeed(float speedIncrease)
    {
        //Increase shot speed on all legs
        foreach (var leg in m_Legs)
        {
            leg.legFlySpeed += speedIncrease;
        }
    }

    private void StartDashVFX()
    {
        dashEffect.SetVector3("PlayerVelocity", moveDirection);
        dashEffect.Play();
    }

    private void StopDashVFX()
    {
        dashEffect.Stop();
    }
}
