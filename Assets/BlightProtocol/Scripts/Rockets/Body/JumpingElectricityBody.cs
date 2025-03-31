using System.Collections;
using UnityEngine;

public class JumpingElectricityBody : ACRocketBody
{
    public GameObject electricityPrefab;
    protected override void Explode()
    {
        //Instance electricity prefab which does the rest
        JumpingElectricity electricityAttack = Instantiate(electricityPrefab, rocketTransform.position, Quaternion.identity).GetComponent<JumpingElectricity>();
        electricityAttack.Activate(10);
    }
}