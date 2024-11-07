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
    public float buffDuration = 5f;
    public float explosionRangeMultiplier = 2f;
    public float shotSpeedMultiplier = 2f;

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
            Vector3 targetScreenPosition;
            switch (type)
            {
                case CollectibleType.ExplosionRange:
                    //Apply Buff and refresh duration
                    other.gameObject.GetComponent<PlayerCore>().IncreaseLegExplosionRadius(explosionRangeMultiplier);
                    UIStatsDisplayer.Instance.RefreshExplosionRangeBuff(buffDuration);

                    //Calculate target position on screen for flying dot
                    targetScreenPosition = UIStatsDisplayer.Instance.explosionRangeNumber.rectTransform.position
                        - new Vector3(UIStatsDisplayer.Instance.explosionRangeNumber.rectTransform.rect.width / 2, 0, 0);

                    FlyingDotController.CreateFlyingDot(transform.position, targetScreenPosition, type);
                    break;
                case CollectibleType.ShotSpeed:
                    //Apply Buff and refresh duration
                    other.gameObject.GetComponent<PlayerCore>().IncreaseLegShotSpeed(shotSpeedMultiplier);
                    UIStatsDisplayer.Instance.RefreshShotSpeedBuff(buffDuration);

                    //Calculate target position on screen for flying dot
                    targetScreenPosition = UIStatsDisplayer.Instance.shotspeedNumber.rectTransform.position
                        - new Vector3(UIStatsDisplayer.Instance.shotspeedNumber.rectTransform.rect.width / 2, 0, 0);

                    FlyingDotController.CreateFlyingDot(transform.position, targetScreenPosition, type);
                    break;
                case CollectibleType.FullHealth:
                    //Find the control zone manager and heal the harvester
                    ControlZoneManager.Instance.Heal();

                    targetScreenPosition = UIStatsDisplayer.Instance.healthSlider.GetComponent<RectTransform>().position;
                    FlyingDotController.CreateFlyingDot(transform.position, targetScreenPosition, type);
                    break;
            }
            Destroy(gameObject);
        }
    }
}
