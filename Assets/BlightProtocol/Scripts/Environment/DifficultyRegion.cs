using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyRegion", menuName = "DifficultyRegion", order = 0)]
public class DifficultyRegion : ScriptableObject
{
    public SpawnableEntity[] spawnableEntities;
    public Vector2 boundsZ;
}