using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageHandler : MonoBehaviour
{
    string m_EnemyTag = "Leg"; //Tag of the object that will destroy this object

    //When collision happens, check if object has the right tag and if it does, destroy this object
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == m_EnemyTag)
        {
            Debug.Log("Collision with: a leg which is attacking: " + other.gameObject.GetComponent<LegHandler>().isAttacking());
            LegHandler leg = other.gameObject.GetComponent<LegHandler>();
            if (leg.isAttacking())
            {
                Destroy(gameObject);
            }
        }
    }
}
