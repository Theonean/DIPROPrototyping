using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

public class HarvesterHealth : MonoBehaviour
{
    public int maxHealth;
    public int health;
    private bool dead = false;
    private Coroutine flyAwayRoutine;
    

    [SerializeField]
    private GameObject m_carrierBalloon;

    public UnityEvent<GameObject> tookDamage;
    public UnityEvent died;

    [Header("Visualization")]
    public List<Slider> healthSliders = new List<Slider>();
    [SerializeField] private ACScreenValueDisplayer healthDisplayer;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;

        foreach (Slider slider in healthSliders)
        {
            slider.maxValue = maxHealth;
            slider.value = maxHealth;
        }

        if (healthDisplayer)
        {
            healthDisplayer.SetValue(health);
            healthDisplayer.SetMaxValue(maxHealth);
        }
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

        if (healthDisplayer)
        {
            healthDisplayer.SetValue(health);
            healthDisplayer.Flash();
        }

        if (health <= 0)
        {
            StopAllCoroutines();

            dead = true;
            died.Invoke();
            //Find Camera Tracker and tell it to track this
            CameraTracker.Instance.objectToTrack = this.gameObject;
            GetComponent<NavMeshAgent>().enabled = false;
            flyAwayRoutine = StartCoroutine(FlyAway());
        }
    }

    public void Fullheal()
    {
        Heal(maxHealth);
    }

    public void Heal(int amount)
    {
        Modifyhealth(amount);
    }

    public void Reset() => StartCoroutine(ResetInternal());

    public IEnumerator ResetInternal()
    {
        dead = false;
        Heal(maxHealth);
        if (flyAwayRoutine != null) StopCoroutine(flyAwayRoutine);

        Animator balloonAnimator = m_carrierBalloon.GetComponent<Animator>();
        m_carrierBalloon.SetActive(true);

        balloonAnimator.Play("Connect_Reversed", 0, 0); // Play from the end of the animation
        
        //While the animator is still playing, wait, after that continue to the fly away code-animation
        while (balloonAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }
    }

    IEnumerator FlyAway()
    {
        m_carrierBalloon.SetActive(true);

        Animator balloonAnimator = m_carrierBalloon.GetComponent<Animator>();
        balloonAnimator.Play("Connect", 0, 0); // Play from the end of the animation

        //While the animator is still playing, wait, after that continue to the fly away code-animation
        while (balloonAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
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
    }
}
