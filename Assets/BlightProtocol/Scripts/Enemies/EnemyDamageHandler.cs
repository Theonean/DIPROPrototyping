using UnityEngine;
using UnityEngine.Events;

public class EnemyDamageHandler : MonoBehaviour
{
    [SerializeField] private EnemyData m_EnemyData;
    string enemyTag = "Rocket"; //Tag of the object that will destroy this object
    public GameObject m_Explosion_1;
    public GameObject m_Explosion_2;
    public UnityEvent enemyDestroyed;

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
