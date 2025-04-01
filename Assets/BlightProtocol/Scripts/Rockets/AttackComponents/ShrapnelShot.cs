using UnityEngine;

public class ShrapnelShot : MonoBehaviour
{
    private Vector3 moveDirection;
    public float speed = 10f;
    public float maxDistance = 10f; // Maximum distance the shrapnel can travel
    public AnimationCurve speedCurve; // Speed curve for shrapnel movement
    private float distanceTraveled = 0f;
    private bool active = false;

    public void Activate(Vector3 direction)
    {
        moveDirection = direction.normalized;
        distanceTraveled = 0f;
        active = true;
    }

    private void Update()
    {
        if (!active) return;


        transform.position += moveDirection * speed * Time.deltaTime * speedCurve.Evaluate(distanceTraveled / maxDistance);
        distanceTraveled += speed * Time.deltaTime;

        if (distanceTraveled >= maxDistance)
        {
            Destroy(gameObject); // Destroy the shrapnel after it has traveled the maximum distance
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // Assuming the enemy has a method to take damage
            EnemyDamageHandler enemyDamageHandler = other.GetComponent<EnemyDamageHandler>();
            if (enemyDamageHandler != null)
            {
                enemyDamageHandler.DestroyEnemy();
            }
            Destroy(gameObject); // Destroy the shrapnel after hitting an enemy
        }
    }
}
