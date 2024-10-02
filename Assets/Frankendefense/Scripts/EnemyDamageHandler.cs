using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyDamageHandler : MonoBehaviour
{
    string m_EnemyTag = "Leg"; //Tag of the object that will destroy this object
    bool m_IsInLeg = false;
    LegHandler m_Leg;
    public ParticleSystem m_Explosion;

    //When collision happens, check if object has the right tag and if it does, destroy this object
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == m_EnemyTag)
        {
            LegHandler leg = other.gameObject.GetComponent<LegHandler>();
            if (leg.isAttacking())
            {
                //Play the explosion and destroy enemy (visually)
                Destroy(gameObject);
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

    private void OnDestroy()
    {
        //Instantiate explosion particle system and destroy after 4 seconds
        m_Explosion.Play();
        Destroy(m_Explosion, 4f);
        Destroy(transform.parent.gameObject, 4f);

        //Get the followplayer component from parent and disable
        FollowPlayer followPlayer = transform.parent.gameObject.GetComponent<FollowPlayer>();
        followPlayer.enabled = false;
    }
}
