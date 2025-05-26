using UnityEngine;
using UnityEngine.Events;

public class EnemyDamageHandler : MonoBehaviour
{
    [SerializeField] private EnemyData m_EnemyData;
    public GameObject m_Explosion_1;
    public UnityEvent enemyDestroyed;
    public static UnityEvent<EnemyType> enemyTypeDestroyed = new();
    public ItemDropper itemDropper;
    private bool dead = false;
    private EnemyType type;

    private void Awake()
    {
        type = GetComponentInParent<ACEnemyMovementBehaviour>().type;
    }

    void Start()
    {
        itemDropper = GetComponentInParent<ItemDropper>();
    }

    public void DestroyEnemy()
    {
        if (dead) return;
        dead = true;

        //Invoke the enemyDestroyed event
        enemyDestroyed.Invoke();
        enemyTypeDestroyed.Invoke(type);

        //Instantiate explosion particle system and destroy after 4 seconds
        Instantiate(m_Explosion_1, transform.position, Quaternion.identity);
        itemDropper.DropItems();
    }
}
