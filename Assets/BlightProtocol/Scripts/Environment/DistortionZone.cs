using UnityEngine;
using System;
using System.Collections.Generic;

public class DistortionZone : MonoBehaviour
{
    private float enemyMinDistanceToTargetBeforeRecalculation = 10f;
    private int enemiesToSpawnPerDifficultyRegion = 30;
    private List<ACEnemyMovementBehaviour> enemyBehaviours = new List<ACEnemyMovementBehaviour>();
    private const float timeToRecalculateDestionations = 1f;
    private float recalculateDestionationsTimer = 0f;
    private bool harvesterEntered = false;

    private SphereCollider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<SphereCollider>();
    }

    private void Start()
    {
        int enemiesToSpawn = enemiesToSpawnPerDifficultyRegion; //TODO add GetDifficultyRegion(Vector3 fromPosition) to difficulty manager so that distortion zones scale

        for (int i = 0; i < enemiesToSpawnPerDifficultyRegion; i++)
        {
            Vector3 enemySpawnPosition = GetRandomPositionInZone();

            GameObject randomEnemy = WaveManager.Instance.GetRandomEnemyPrefab();
            GameObject spawnedEnemy = Instantiate(randomEnemy, enemySpawnPosition, Quaternion.identity);

            ACEnemyMovementBehaviour enemyMovement = spawnedEnemy.GetComponent<ACEnemyMovementBehaviour>();
            enemyMovement.SetMovementType(EnemyMovementType.REMOTECONTROLLED);

            enemyBehaviours.Add(enemyMovement);
        }
    }

    private void Update()
    {
        recalculateDestionationsTimer -= Time.deltaTime;
        if(recalculateDestionationsTimer < 0)
        {
            recalculateDestionationsTimer = timeToRecalculateDestionations;

            List<ACEnemyMovementBehaviour> behavioursToRemove = new List<ACEnemyMovementBehaviour>();
            foreach(ACEnemyMovementBehaviour enemyBehaviour in enemyBehaviours)
            {
                if(!enemyBehaviour)
                {
                    behavioursToRemove.Add(enemyBehaviour);
                    continue;
                }

                Vector3 targetPosition = enemyBehaviour.navMeshAgent.pathEndPosition;
                if(Vector3.Distance(enemyBehaviour.transform.position, targetPosition) < enemyMinDistanceToTargetBeforeRecalculation)
                {
                    enemyBehaviour.SetDestination(GetRandomPositionInZone());
                }
            }

            foreach(ACEnemyMovementBehaviour behaviourToRemove in behavioursToRemove)
            {
                enemyBehaviours.Remove(behaviourToRemove);
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (harvesterEntered) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("PL_IsHarvester"))
        {
            harvesterEntered = true;
            foreach (ACEnemyMovementBehaviour enemyBehaviour in enemyBehaviours)
            {
                enemyBehaviour.SetMovementType(EnemyMovementType.CUSTOM);
            }
        }

        //initial implementation went different route, we thought maybe distortion zone influences buildings spawned inside it???
        InfluencedByDistortionZone IBDZ = collision.GetComponentInParent<InfluencedByDistortionZone>();
        if (!IBDZ) return;
    }

    private Vector3 GetRandomPositionInZone()
    {
        Vector3 enemySpawnPosition = transform.position + UnityEngine.Random.insideUnitSphere * zoneCollider.radius;
        enemySpawnPosition.y = 0f;

        return enemySpawnPosition;
    }
}
