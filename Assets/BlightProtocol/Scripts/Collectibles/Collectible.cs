using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collectible : MonoBehaviour
{
    [SerializeField]
    GameObject ExplosionRangeModel;
    [SerializeField]
    GameObject MoveSpeedModel;
    [SerializeField]
    GameObject FullHealthModel;

    public ECollectibleType type;
    public static float buffDuration = 5f;
    public static float explosionRangeMultiplier = 2f;
    public static float shotSpeedMultiplier = 2f;
    public static UnityEvent<ECollectibleType> OnCollectiblePickedUp = new UnityEvent<ECollectibleType>();

    // Start is called before the first frame update
    void Start()
    {
        //When harvester is at full health, don't drop healing packs
        int topRange = ControlZoneManager.Instance.health == ControlZoneManager.Instance.maxHealth ? 2 : 3;
        int bottomRange = 0;

        //Determine what type of collectible this is and enable the corresponding game object
        type = (ECollectibleType)UnityEngine.Random.Range(bottomRange, topRange);
        switch (type)
        {
            case ECollectibleType.ExplosionRange:
                ExplosionRangeModel.SetActive(true);
                break;
            case ECollectibleType.ShotSpeed:
                MoveSpeedModel.SetActive(true);
                break;
            case ECollectibleType.FullHealth:
                FullHealthModel.SetActive(true);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Vector3 targetScreenPosition;
            OnCollectiblePickedUp.Invoke(type);

            switch (type)
            {
                case ECollectibleType.ExplosionRange:
                    //Apply Buff and refresh duration

                    UIStatsDisplayer.Instance.RefreshExplosionRangeBuff(buffDuration);

                    //Calculate target position on screen for flying dot
                    targetScreenPosition = UIStatsDisplayer.Instance.explosionRangeNumber.rectTransform.position
                        - new Vector3(UIStatsDisplayer.Instance.explosionRangeNumber.rectTransform.rect.width / 2, 0, 0);

                    FlyingDotController.CreateFlyingDot(transform.position, targetScreenPosition, type);
                    break;
                case ECollectibleType.ShotSpeed:
                    //Apply Buff and refresh duration
                    UIStatsDisplayer.Instance.RefreshShotSpeedBuff(buffDuration);

                    //Calculate target position on screen for flying dot
                    targetScreenPosition = UIStatsDisplayer.Instance.shotspeedNumber.rectTransform.position
                        - new Vector3(UIStatsDisplayer.Instance.shotspeedNumber.rectTransform.rect.width / 2, 0, 0);

                    FlyingDotController.CreateFlyingDot(transform.position, targetScreenPosition, type);
                    break;
                case ECollectibleType.FullHealth:
                    //Find the control zone manager and heal the harvester

                    targetScreenPosition = UIStatsDisplayer.Instance.healthSlider.GetComponent<RectTransform>().position;
                    FlyingDotController.CreateFlyingDot(transform.position, targetScreenPosition, type);
                    break;
            }
            Destroy(gameObject);
        }
    }
}
