using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectibleType
{
    ExplosionRange,
    ShotSpeed,
    FullHealth
}

public class Collectible : MonoBehaviour
{
    [SerializeField]
    GameObject ExplosionRangeModel;
    [SerializeField]
    GameObject MoveSpeedModel;
    [SerializeField]
    GameObject FullHealthModel;

    public CollectibleType type;
    public float explosionRangeIncrease = 0.1f;
    public float shotSpeedIncrease = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        //Determine what type of collectible this is and enable the corresponding game object
        type = (CollectibleType)UnityEngine.Random.Range(0, 3);
        switch (type)
        {
            case CollectibleType.ExplosionRange:
                ExplosionRangeModel.SetActive(true);
                break;
            case CollectibleType.ShotSpeed:
                MoveSpeedModel.SetActive(true);
                break;
            case CollectibleType.FullHealth:
                FullHealthModel.SetActive(true);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            switch (type)
            {
                case CollectibleType.ExplosionRange:
                    other.gameObject.GetComponent<PlayerCore>().IncreaseLegExplosionRadius(explosionRangeIncrease);
                    UIStatsDisplayer.Instance.IncrementExplosionRange();
                    break;
                case CollectibleType.ShotSpeed:
                    other.gameObject.GetComponent<PlayerCore>().IncreaseLegShotSpeed(shotSpeedIncrease);
                    UIStatsDisplayer.Instance.IncrementShotSpeed();
                    break;
                case CollectibleType.FullHealth:
                    //Find the control zone manager and give the player full health
                    ControlZoneManager.Instance.FullHeal();
                    break;
            }
            Destroy(gameObject);
        }
    }
}
