using System.Collections;
using UnityEngine;

public class JumpingElectricity : MonoBehaviour
{
    public int jumpsLeft = 10;
    public float maxJumpDistance = 10f; // Maximum distance the electricity can jump to an enemy
    public float jumpInterval = 0.5f; // Time interval between jumps
    public float jumpDuration = 0.2f; // Duration of each jump
    private EnemyDamageHandler targetedEnemy; // The enemy collider that the electricity will jump to
    public bool active = false;

    public void Activate(int jumpsLeft)
    {
        this.jumpsLeft = jumpsLeft;
        active = true;
    }

    private void Update()
    {
        if (!active) return;
        FindEnemyForJump();
    }

    private void FindEnemyForJump()
    {
        // Check within physics sphere for colliders on layer enemies, select closest one to jump towards
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, maxJumpDistance, LayerMask.GetMask("PL_IsEnemy"));
        if (hitColliders.Length > 0)
        {
            // Find the closest enemy collider
            float closestDistance = float.MaxValue;
            foreach (var hitCollider in hitColliders)
            {
                if(hitCollider.CompareTag("EnemyArmor")) continue; // Enemy Armor cant be destroyed by electricity

                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetedEnemy = hitCollider.GetComponent<EnemyDamageHandler>();
                }

                // Jump to the closest enemy
                if (targetedEnemy != null && Time.time % jumpInterval < Time.deltaTime)
                {
                    JumpToEnemy(targetedEnemy);
                }
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void JumpToEnemy(EnemyDamageHandler enemy)
    {
        // Calculate the jump direction and distance
        Vector3 jumpDirection = (enemy.transform.position - transform.position).normalized;
        float jumpDistance = Mathf.Min(Vector3.Distance(transform.position, enemy.transform.position), maxJumpDistance);

        // Perform the jump effect
        StartCoroutine(JumpCoroutine(jumpDirection, jumpDistance));
    }

    private IEnumerator JumpCoroutine(Vector3 jumpDirection, float jumpDistance)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + jumpDirection * jumpDistance;

        float elapsedTime = 0f;
        while (elapsedTime < jumpDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / jumpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Optionally, add logic to deal damage or apply effects to the enemy here
        targetedEnemy.DestroyEnemy();
        jumpsLeft--;
        if (jumpsLeft <= 0)
        {
            Destroy(this.gameObject); // Destroy the electricity effect after all jumps are used
        }
        else
        {
            // Optionally, add logic to reset the position or effects here
            transform.position = targetPosition; // Move to the last jump position
            FindEnemyForJump(); // Find the next enemy to jump to

        }
    }
}
