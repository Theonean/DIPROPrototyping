using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyDamageHandler : MonoBehaviour
{
    string m_EnemyTag = "Leg"; //Tag of the object that will destroy this object
    bool m_IsInLeg = false;
    LegHandler m_Leg;

    //When collision happens, check if object has the right tag and if it does, destroy this object
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == m_EnemyTag)
        {
            LegHandler leg = other.gameObject.GetComponent<LegHandler>();
            if (leg.isAttacking())
            {
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
            LegHandler leg = other.gameObject.GetComponent<LegHandler>();
            m_IsInLeg = false;
        }
    }

    void Update()
    {
        if (m_IsInLeg && m_Leg.isAttacking())
        {
            Destroy(gameObject);
        }
    }
}
