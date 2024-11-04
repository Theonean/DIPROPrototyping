using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class PlayerCore : MonoBehaviour
{
    public static PlayerCore Instance { get; private set; }
    bool m_IsDashing = false;
    float m_DashKnockback = 150f;
    float m_DashTime = 0.3f;
    float m_DashSpeed = 60f;
    float m_DashCooldown = 1f;
    float m_DashCooldownTimer;
    public float moveSpeed;
    public float currentSpeed;
    public float maxHealth; //Number of hits drone can take until it dies
    public CanvasGroup playerDiedGroup;
    public TextMeshProUGUI respawnTimerText;
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
    public bool isDead = false; //When dead, track the ControlZoneManager to respawn the drone

    private LegHandler[] m_Legs = new LegHandler[4];

    [Header("VFX")]
    public VisualEffect dashEffect;

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
        if (!isDead)
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
        if (isDead)
        {
            m_RespawnTimer += Time.deltaTime;
            transform.position = FindObjectOfType<ControlZoneManager>().transform.position;
            respawnTimerText.text = Mathf.Clamp(respawnTime - m_RespawnTimer, 0f, respawnTime).ToString("F2");

            if (m_RespawnTimer >= respawnTime)
            {
                m_RespawnTimer = 0f;
                m_Health = maxHealth;
                isDead = false;
                StartCoroutine(FadeDroneDied(false));
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
                // Knockback the enemy
                Debug.Log("Knocked back enemy");
                StartCoroutine(other.GetComponentInParent<FollowPlayer>().ApplyKnockback(moveDirection, m_DashKnockback));
            }
            else if (!isDead) //Only take damage when not dead
            {
                EnemyDamageHandler enemy = other.GetComponent<EnemyDamageHandler>();
                enemy.DestroyEnemy();

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

        //Check if there are alreay enemies inside thewd players collider, knockback or damage them
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                Debug.Log("Knocked back enemy");
                StartCoroutine(hitCollider.GetComponentInParent<FollowPlayer>().ApplyKnockback(moveDirection, m_DashKnockback));
            }
        }

        StartCoroutine(DashMovement());

        //Trigger Dash Effect here
        StartDashVFX();
    }

    public void ModifyHealth(int amount)
    {   
        //When harvester has died, don't take damage
        if(ControlZoneManager.Instance.GetZoneState().Equals(ZoneState.DIED))
        {
            return;
        }
        
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
            transform.position = FindObjectOfType<ControlZoneManager>().transform.position;
            isDead = true;
            m_RespawnTimer = 0f;
            StartCoroutine(FadeDroneDied(true));
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
        StopDashVFX();
    }

    public void IncreaseLegExplosionRadius(float multiplier)
    {
        //Increase explosion radius on all legs
        foreach (var leg in m_Legs)
        {
            leg.explosionRadius = Mathf.Floor(leg.explosionRadius * multiplier);
        }

        UIStatsDisplayer.Instance.explosionRangeBuffTimerFinished.AddListener(ResetLegExplosionRadius);
    }

    private void ResetLegExplosionRadius()
    {
        foreach (var leg in m_Legs)
        {
            leg.explosionRadius = LegHandler.explosionRadiusBase;
        }
    }

    public void IncreaseLegShotSpeed(float multiplier)
    {
        //Increase shot speed on all legs
        foreach (var leg in m_Legs)
        {
            leg.legFlySpeed = Mathf.Floor(leg.legFlySpeed * multiplier);
        }

        UIStatsDisplayer.Instance.shotspeedBuffTimerFinished.AddListener(ResetLegShotSpeed);
    }

    private void ResetLegShotSpeed()
    {
        foreach (var leg in m_Legs)
        {
            leg.legFlySpeed = LegHandler.legFlySpeedBase;
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
    private IEnumerator FadeDroneDied(bool fadeIn)
    {
        float time = 0f;
        float maxTime = 0.5f;
        while (time < maxTime)
        {
            time += Time.deltaTime;
            if (fadeIn)
                playerDiedGroup.alpha = Mathf.Lerp(0, 1, time / maxTime);
            else
                playerDiedGroup.alpha = Mathf.Lerp(1, 0, time / maxTime);
            yield return null;
        }
    }
}
