using System.Collections;
using UnityEngine;

public class JumpingElectricityBody : ACRocketBody
{
    public GameObject electricityPrefab;
    public int[] electricityJumpsPerLevel = { 1, 2, 3, 4, 5 };
    private int electricityJumpsBase;

    protected override void SetStatsToLevel()
    {
        electricityJumpsBase = electricityJumpsPerLevel[componentLevel - 1];

        Logger.Log($"Leveling up {DescriptiveName} to level {componentLevel + 1}. Explosion radius: {explosionRadius}", LogLevel.INFO, LogType.ROCKETS);
    }
    protected override void Explode()
    {
        //Instance electricity prefab which does the rest
        JumpingElectricity electricityAttack = Instantiate(electricityPrefab, rocketTransform.position, Quaternion.identity).GetComponent<JumpingElectricity>();
        electricityAttack.Activate(electricityJumpsBase, this);
    }

    public override string GetResearchDescription()
    {
        if (componentLevel == 0)
        {
            return "Unlock component";
        }
        else if (componentLevel == maxComponentLevel)
        {
            return upgradeDescription + " " + electricityJumpsBase;
        }
        else
        {
            return upgradeDescription + " " + electricityJumpsBase + " -> " + electricityJumpsPerLevel[componentLevel];
        }
    }
    public override string GetResearchDescription(int customLevel)
    {
        if (customLevel == 0)
        {
            return "Unlock component";
        }
        else if (customLevel == maxComponentLevel)
        {
            return upgradeDescription + " " + electricityJumpsPerLevel[customLevel - 1];
        }
        else
        {
            return upgradeDescription + " " + electricityJumpsPerLevel[customLevel - 1] + " -> " + electricityJumpsPerLevel[customLevel];
        }
    }
}