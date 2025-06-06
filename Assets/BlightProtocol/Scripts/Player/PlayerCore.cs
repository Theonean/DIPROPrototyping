using System.Collections;
using FMOD.Studio;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class PlayerCore : MonoBehaviour
{
    public static PlayerCore Instance { get; private set; }
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
    public bool isDead = false; //When dead, track the ControlZoneManager to respawn the drone

    [Header("VFX")]
    public GameObject explosion;
    private ShieldVFX shieldVFX;
    [Header("SFX")]
    public string shieldSFXEventPath; // FMOD event path
    private EventInstance shieldSFXInstance;

    private Renderer[] meshRenderers;

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

        shieldVFX = shield.GetComponent<ShieldVFX>();

        Harvester.Instance.health.died.AddListener(CommitSeppuku);
    }

    private void Start()
    {
        FMODAudioManagement.instance.PlaySound(out shieldSFXInstance, shieldSFXEventPath, gameObject);
    }

    void Update()
    {

        if (m_RegenShield)
        {
            m_ShieldRespawnTimer -= Time.deltaTime;
            if (m_ShieldRespawnTimer <= 0f)
            {
                ModifyHealth(+1);

                FMODAudioManagement.instance.PlaySound(out shieldSFXInstance, shieldSFXEventPath, gameObject);
            }
        }

        //Count down respawn timer and respawn drone when player has no health
        if (isDead)
        {
            m_RespawnTimer += Time.deltaTime;
            respawnTimerText.text = Mathf.Clamp(respawnTime - m_RespawnTimer, 0f, respawnTime).ToString("F2");

            if (m_RespawnTimer >= respawnTime)
            {
                m_RespawnTimer = 0f;
                m_Health = maxHealth;
                isDead = false;

                shieldSFXInstance.setPaused(false);

                StartCoroutine(FadeDroneDied(false));
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            returnLegs.Invoke();
        }
    }


    //When the player collides with an object, check if the object is an enemy and apply knockback or damage when dashing
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (DroneMovement.Instance.IsDashing)
            {
                // Knockback the enemy
                Debug.Log("Knocked back enemy");
                StartCoroutine(other.GetComponentInParent<ACEnemyMovementBehaviour>().ApplyKnockback(DroneMovement.Instance.moveDirection));
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

    private void CommitSeppuku()
    {
        // Instantiate Explosion VFX
        Instantiate(explosion, transform.position, Quaternion.identity);

        //Pause Shield SFX When Dead
        shieldSFXInstance.setPaused(true);

        isDead = true;
        m_RespawnTimer = 0f;
        shieldVFX.ToggleShield(true);

        RocketAimController.Instance.ExplodeUnattachedRockets();

        ToggleDisplayDrone(false);

        PerspectiveSwitcher.Instance.SetPerspective(CameraPerspective.SWITCHING);
    }

    public void ToggleDisplayDrone(bool isDroneVisible)
    {
        meshRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in meshRenderers)
        {
            if(renderer) renderer.enabled = isDroneVisible;
        }
    }

    public void ModifyHealth(int amount)
    {
        //When harvester has died, don't take damage
        if (Harvester.Instance.GetZoneState().Equals(HarvesterState.DIED))
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

            //Make drone invisible when dead
            isDead = true;
            m_RespawnTimer = 0f;
            shieldVFX.ToggleShield(true);
            StartCoroutine(FadeDroneDied(true));

            RocketAimController.Instance.ExplodeUnattachedRockets();

            PerspectiveSwitcher.Instance.SetPerspective(CameraPerspective.SWITCHING);

            ToggleDisplayDrone(false);
        }
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
