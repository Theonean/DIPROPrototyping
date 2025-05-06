using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    REGULAR,
    CHARGER,
    CRABTANK,
    NONE,
    ALL
}

[System.Serializable]
public class EnemySpawnPosition
{
    public Vector3 position;
    public EnemyType enemyType;

    public EnemySpawnPosition(Vector3 position, EnemyType enemyType)
    {
        this.position = position;
        this.enemyType = enemyType;
    }
}

public class SOEnemySpawnPattern : ScriptableObject
{
    public List<EnemySpawnPosition> spawnPositions = new List<EnemySpawnPosition>();
    public float spacing = 5f;
    public int maxEnemies = 10;
    public float spawnInterval = 1f;
    public float spawnRadius = 5f;
    public float spawnHeight = 1f;
    public bool isActive = true;

    // Add any additional properties or methods you need for your enemy spawn pattern here
}