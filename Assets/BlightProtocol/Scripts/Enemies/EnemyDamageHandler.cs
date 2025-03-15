using UnityEngine;
using UnityEngine.Events;

public class EnemyDamageHandler : MonoBehaviour
{
    [SerializeField] private EnemyData m_EnemyData;
    string m_EnemyTag = "Leg"; //Tag of the object that will destroy this object
    bool m_IsInLeg = false;
    private int health;
    LegHandler m_Leg;
    public GameObject m_Explosion_1;
    public GameObject m_Explosion_2;
    public UnityEvent enemyDestroyed;

    private void Awake() {
        health = m_EnemyData.maxHealth;
    }

    //When collision happens, check if object has the right tag and if it does, destroy this object
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == m_EnemyTag)
        {
            LegHandler leg = other.gameObject.GetComponent<LegHandler>();
            if (leg.isAttacking())
            {
                if (m_EnemyData.isOneShot)
                {
                    health = 0;
                }
                else
                {
                    RocketData rocketData = leg.GetRocketData();
                    health -= rocketData.damage;
                    Debug.Log("Enemy health: " + health); 
                }

                //Play the explosion and destroy enemy (visually)
                if (health <= 0) DestroyEnemy();
            }
            else
            {
                m_IsInLeg = true;
                m_Leg = leg;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == m_EnemyTag)
        {
            m_IsInLeg = false;
        }
    }

    void Update()
    {
        if (m_IsInLeg && m_Leg.isAttacking())
        {
            //Play the explosion and destroy enemy (visually)
            Destroy(gameObject);
        }
    }

    public void DestroyEnemy()
    {
        //Invoke the enemyDestroyed event
        enemyDestroyed.Invoke();

        //Instantiate explosion particle system and destroy after 4 seconds
        GameObject explosionEffect = GetComponentInParent<EnemyTypeDecider>().enemyType ? m_Explosion_1 : m_Explosion_2;
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Destroy(transform.parent.gameObject);
    }
}
