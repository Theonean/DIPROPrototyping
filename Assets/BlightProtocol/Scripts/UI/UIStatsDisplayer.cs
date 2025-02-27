using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIStatsDisplayer : MonoBehaviour
{
    public static UIStatsDisplayer Instance { get; private set; }
    public Slider healthSlider;
    public TextMeshProUGUI explosionRangeNumber;
    public Slider explosionRangeBuffTimer;
    public TextMeshProUGUI shotspeedNumber;
    public Slider shotspeedBuffTimer;
    public LegHandler legInstance;
    private RocketData rocketData;
    public UnityEvent explosionRangeBuffTimerFinished;
    public UnityEvent shotspeedBuffTimerFinished;

    private void Awake()
    {
        // Ensure there's only one instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        rocketData = legInstance.GetRocketData();

        //Set initial value for Shot speed and explosion range from leghandler
        explosionRangeNumber.text = rocketData.explosionRadius.ToString() + "m";
        shotspeedNumber.text = rocketData.flySpeed.ToString() + "m/s";

    }

    private void Update()
    {
        if (explosionRangeBuffTimer.value > 0)
        {
            explosionRangeBuffTimer.value -= Time.deltaTime;
            if (explosionRangeBuffTimer.value <= 0)
            {
                explosionRangeBuffTimerFinished.Invoke();
                UpdateUIExplosionRange();
            }
        }

        if (shotspeedBuffTimer.value > 0)
        {
            shotspeedBuffTimer.value -= Time.deltaTime;
            if (shotspeedBuffTimer.value <= 0)
            {
                shotspeedBuffTimerFinished.Invoke();
                UpdateUIShotSpeed();
            }
        }
    }

    public void UpdateUIExplosionRange()
    {
        string temp = rocketData.explosionRadius.ToString() + "m";
        explosionRangeNumber.text = temp;
    }

    public void UpdateUIShotSpeed()
    {
        string temp = rocketData.flySpeed.ToString() + "m/s";
        shotspeedNumber.text = temp;
    }

    public void RefreshExplosionRangeBuff(float value)
    {
        UpdateUIExplosionRange();
        explosionRangeBuffTimer.maxValue = value;
        explosionRangeBuffTimer.value = value;
    }

    public void RefreshShotSpeedBuff(float value)
    {
        UpdateUIShotSpeed();
        shotspeedBuffTimer.maxValue = value;
        shotspeedBuffTimer.value = value;
    }
}
