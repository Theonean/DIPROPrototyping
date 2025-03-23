using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HarvesterHealth : MonoBehaviour
{
    public int maxHealth;
    public int health;
    private bool dead = false;

    public List<Slider> healthSliders = new List<Slider>();

    [SerializeField]
    private GameObject m_carrierBalloon;

    public UnityEvent<GameObject> tookDamage;
    public UnityEvent died;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;

        foreach (Slider slider in healthSliders)
        {
            slider.maxValue = maxHealth;
            slider.value = maxHealth;
        }
    }

    private void OnEnable()
    {
        Collectible.OnCollectiblePickedUp.AddListener(HealOnPickup);
    }

    private void OnDisable()
    {
        Collectible.OnCollectiblePickedUp.RemoveListener(HealOnPickup);
    }

    void OnTriggerEnter(Collider other)
    {
        if (dead) return;

        if (other.gameObject.CompareTag("Enemy"))
        {
            tookDamage.Invoke(other.gameObject);

            Modifyhealth(-1);
            other.gameObject.GetComponent<EnemyDamageHandler>().DestroyEnemy();
            //StartCoroutine(TakeDamageEffect());
        }
    }
    public bool IsAtFullHealth() { return health == maxHealth; }

    void Modifyhealth(int amount)
    {
        if (dead) return;

        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);

        foreach (Slider slider in healthSliders)
        {
            slider.value = health;
        }

        if (health <= 0)
        {
            died.Invoke();
            //Find Camera Tracker and tell it to track this
            CameraTracker.Instance.objectToTrack = this.gameObject;
            StartCoroutine(FlyAway());
        }
    }

    private void HealOnPickup(ECollectibleType type)
    {
        if (type == ECollectibleType.FullHealth)
        {
            Heal();
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

}
