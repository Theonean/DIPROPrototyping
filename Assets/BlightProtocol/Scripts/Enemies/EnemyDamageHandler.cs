using UnityEngine;
using UnityEngine.Events;

public class EnemyDamageHandler : MonoBehaviour
{
    [SerializeField] private EnemyData m_EnemyData;
    public GameObject m_Explosion_1;
    public UnityEvent enemyDestroyed;
    private ItemDropper[] itemDroppers;

    void Start()
    {
        ItemDropper[] itemDroppers = GetComponents<ItemDropper>();
    }

    public void DestroyEnemy()
    {
        //Invoke the enemyDestroyed event
        enemyDestroyed.Invoke();

        //Instantiate explosion particle system and destroy after 4 seconds
        Instantiate(m_Explosion_1, transform.position, Quaternion.identity);
        foreach (ItemDropper dropper in itemDroppers) { dropper.DropItems(); }
    }
}
