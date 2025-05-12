using System.Collections;
using UnityEngine;

public class JumpingElectricityBody : ACRocketBody
{
    public GameObject electricityPrefab;
    public int[] electricityJumpsPerLevel = { 1, 2, 3, 4, 5 };
    private int electricityJumpsBase;

    protected override void SetStatsToLevel()
    {
        electricityJumpsBase = electricityJumpsPerLevel[componentLevel];

        Logger.Log($"Leveling up {DescriptiveName} to level {componentLevel + 1}. Explosion radius: {explosionRadius}", LogLevel.INFO, LogType.ROCKETS);
    }
    protected override void Explode()
    {
        //Instance electricity prefab which does the rest
        JumpingElectricity electricityAttack = Instantiate(electricityPrefab, rocketTransform.position, Quaternion.identity).GetComponent<JumpingElectricity>();
        electricityAttack.Activate(electricityJumpsBase, this);
    }
}