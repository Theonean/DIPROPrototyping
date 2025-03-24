using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
public class SeismographVisualizer : MonoBehaviour
{
    public Slider vibrationSlider;

    private float currentVibration = 0f;

    void Start()
    {
        Seismograph.Instance.vibrationChanged.AddListener(UpdateVibration);
    }

    public void UpdateVibration()
    {
        StopAllCoroutines();
        StartCoroutine(AdjustVibrationDisplay());
    }

    private IEnumerator AdjustVibrationDisplay()
    {
        float newVibration = Seismograph.Instance.GetTotalVibration();
        float oldVibration = currentVibration;
        if (oldVibration < newVibration)
        {
            while (currentVibration < newVibration) {
                yield return null;
                currentVibration++;
                vibrationSlider.value = currentVibration;
            }
            vibrationSlider.value = newVibration;
        }
        else {
            while (currentVibration > newVibration) {
                yield return null;
                currentVibration--;
                vibrationSlider.value = currentVibration;
            }
            vibrationSlider.value = newVibration;
        }
    }
}
