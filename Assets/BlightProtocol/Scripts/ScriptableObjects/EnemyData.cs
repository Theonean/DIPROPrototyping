using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "EnemyData", order = 0)]
public class EnemyData : ScriptableObject
{
    public bool isOneShot;
    public int maxHealth = 100;
    public int damage;

}