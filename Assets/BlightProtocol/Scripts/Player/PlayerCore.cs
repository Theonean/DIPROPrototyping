using System.Collections;
using FMOD.Studio;
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
    private bool m_standingStill = true;
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
    public GameObject explosion;
    private ShieldVFX shieldVFX;
    [Header("SFX")]
    public string shieldSFXEventPath; // FMOD event path
    public string movementSFXPath; // FMOD event path
    private EventInstance shieldSFXInstance;
    private EventInstance movementSFXInstance;

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
        shieldVFX = shield.GetComponent<ShieldVFX>();
    }

    private void Start()
    {
        FMODAudioManagement.instance.PlaySound(out shieldSFXInstance, shieldSFXEventPath, gameObject);
        FMODAudioManagement.instance.PlaySound(out movementSFXInstance, movementSFXPath, gameObject);
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

                FMODAudioManagement.instance.PlaySound(out shieldSFXInstance, shieldSFXEventPath, gameObject);
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
                    if (m_standingStill)
                    {
                        m_standingStill = false;
                        movementSFXInstance.setParameterByName("Movement", 1f);
                    }
                }
                else
                {
                    moveDirection = Vector3.zero;
                    m_AccelerationTime = 0f;
                    m_standingStill = true;
                    movementSFXInstance.setParameterByName("Movement", 0f);
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

                shieldSFXInstance.setPaused(false);
                movementSFXInstance.setPaused(false);

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
        if (m_IsDashing || moveDirection == Vector3.zero) return; // Prevent multiple dashes at the same time

        m_DashCooldownTimer = m_DashCooldown;
        m_IsDashing = true;

        movementSFXInstance.setParameterByName("Dash", 1f);

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
        if (ControlZoneManager.Instance.GetZoneState().Equals(ZoneState.DIED))
        {
            return;
        }

        // Update the shield status
        if (amount > 0)
        {
            m_RegenShield = false;
            shieldVFX.ToggleShield(true);
        }
        //Break the shield
        else if (amount < 0)
        {
            m_RegenShield = true;
            shieldVFX.ToggleShield(false);
            m_ShieldRespawnTimer = shieldRespawnCooldown;

            //Play shield SFX
            shieldSFXInstance.keyOff();
        }

        //Change health
        m_Health += amount;
        m_Health = Mathf.Clamp(m_Health, 0, maxHealth);

        //Check if dead
        if (m_Health <= 0)
        {
            // Instantiate Explosion VFX
            Instantiate(explosion, transform.position, Quaternion.identity);

            //Pause Shield SFX When Dead
            shieldSFXInstance.setPaused(true);
            movementSFXInstance.setPaused(true);

            //Make drone invisible when dead
            transform.position = FindObjectOfType<ControlZoneManager>().transform.position;
            isDead = true;
            m_RespawnTimer = 0f;
            shieldVFX.ToggleShield(true);
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

        movementSFXInstance.setParameterByName("Dash", 0f);
        m_IsDashing = false;
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
