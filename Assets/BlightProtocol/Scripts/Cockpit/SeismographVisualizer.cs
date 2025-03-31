using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class SeismographVisualizer : MonoBehaviour
{
    public Slider vibrationSlider;

    public float maxVibration = 30f;

    public LineRenderer lineRenderer;
    public Transform boundsStart;
    public Transform boundsEnd;
    public int pointAmount = 50; // Number of points on the graph

    private float width;
    private float height;
    private float currentVibration = 0f;
    public float adjustDuration = 0.5f;

    public float vibrationFrequency = 20f; // Frequency of oscillation
    public float overlay1Frequency = 45f;
    public float noiseFrequency = 2f;

    private Queue<float> vibrationHistory = new Queue<float>();

    void Start()
    {
        Seismograph.Instance.vibrationChanged.AddListener(SetVibration);
        width = boundsEnd.localPosition.x - boundsStart.localPosition.x;
        height = boundsEnd.localPosition.y - boundsStart.localPosition.y;

        // Initialize history with zero values
        for (int i = 0; i < pointAmount; i++)
        {
            vibrationHistory.Enqueue(0f);
        }

        currentVibration = Seismograph.Instance.GetTotalVibration() / maxVibration;
    }

    void Update()
    {
        UpdateVibration();
    }

    void SetVibration()
    {
        StopAllCoroutines();
        StartCoroutine(AdjustVibrationOverTime(adjustDuration));
    }

    private void UpdateVibration()
    {
        float oscillation = Mathf.Sin(Time.time * vibrationFrequency);
        float oscillationMod = Mathf.Sin(Time.time * overlay1Frequency);
        float noise = Mathf.PerlinNoise(Time.time * noiseFrequency, currentVibration * noiseFrequency);

        // Compute raw sum
        float rawSum = oscillation + oscillationMod + noise;

        // Compute a smooth scaling factor (based on expected max range)
        float scaleFactor = 1f / (1f + Mathf.Abs(rawSum)); // Keeps values within a softer bound

        // Apply scaling
        float scaledOscillation = rawSum * scaleFactor;

        float finalOscillation = scaledOscillation * currentVibration;


        vibrationHistory.Enqueue(finalOscillation);

        if (vibrationHistory.Count > pointAmount)
            vibrationHistory.Dequeue();

        Vector3[] points = new Vector3[vibrationHistory.Count];
        int i = 0;
        foreach (float v in vibrationHistory)
        {
            points[i] = GetRelativeLinePos(i, v);
            i++;
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    Vector3 GetRelativeLinePos(int index, float value)
    {
        float xPosition = boundsEnd.localPosition.x - (index * (width / pointAmount));
        float yPosition = Mathf.Clamp(value / maxVibration * height, -maxVibration, maxVibration);

        return new Vector3(xPosition, yPosition * 10, 0) * 100;
    }

    private IEnumerator AdjustVibrationOverTime(float duration)
    {
        float newVibration = Seismograph.Instance.GetTotalVibration() / maxVibration;
        float oldVibration = currentVibration;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            currentVibration = Mathf.Lerp(oldVibration, newVibration, t);
            yield return null;
        }

        currentVibration = newVibration;
    }
}
