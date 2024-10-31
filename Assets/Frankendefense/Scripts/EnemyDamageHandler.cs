using UnityEngine;
using UnityEngine.Events;

public class EnemyDamageHandler : MonoBehaviour
{
    string m_EnemyTag = "Leg"; //Tag of the object that will destroy this object
    bool m_IsInLeg = false;
    LegHandler m_Leg;
    public ParticleSystem m_Explosion;
    public UnityEvent enemyDestroyed;

    //When collision happens, check if object has the right tag and if it does, destroy this object
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == m_EnemyTag)
        {
            LegHandler leg = other.gameObject.GetComponent<LegHandler>();
            if (leg.isAttacking())
            {
                //Play the explosion and destroy enemy (visually)
                DestroyEnemy();
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
        m_Explosion.Play();
        Destroy(m_Explosion, 4f);
        Destroy(transform.parent.gameObject, 4f);

        //Get the followplayer and navmeshagent component from parent and disable
        FollowPlayer followPlayer = transform.parent.gameObject.GetComponent<FollowPlayer>();
        UnityEngine.AI.NavMeshAgent navMeshAgent = transform.parent.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        followPlayer.enabled = false;
        navMeshAgent.enabled = false;
        Destroy(gameObject);
    }
}
