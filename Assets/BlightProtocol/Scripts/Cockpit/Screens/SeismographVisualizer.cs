using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class SeismographVisualizer : ACScreenValueDisplayer
{
    [Header("Seismograph Settings")]
    [SerializeField] private float maxVibration = 30f;
    [SerializeField] private float adjustDuration = 0.5f;

    [Header("Line Renderer Settings")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform boundsStart;
    [SerializeField] private Transform boundsEnd;
    [SerializeField] private int pointAmount = 50;

    [Header("Wave Settings")]
    [SerializeField] private float vibrationFrequency = 20f;
    [SerializeField] private float overlay1Frequency = 45f;
    [SerializeField] private float noiseFrequency = 2f;

    [Header("Color")]
    [SerializeField] private Image background;
    [SerializeField] private Image triangle;

    private float width;
    private float height;
    private Queue<float> vibrationHistory = new Queue<float>();


    private void Start()
    {
        InitializeSeismograph();
        UpdateValue(Seismograph.Instance.GetTotalVibration());
        Seismograph.Instance.vibrationChanged.AddListener(OnVibrationChanged);
    }

    private void InitializeSeismograph()
    {
        useMaxValue = true;
        maxValue = maxVibration;

        width = boundsEnd.localPosition.x - boundsStart.localPosition.x;
        height = boundsEnd.localPosition.y - boundsStart.localPosition.y;

        for (int i = 0; i < pointAmount; i++)
        {
            vibrationHistory.Enqueue(0f);
        }
    }

    void OnEnable()
    {
        if (Seismograph.Instance != null)
        {
            Seismograph.Instance.vibrationChanged.RemoveListener(OnVibrationChanged);
            Seismograph.Instance.vibrationChanged.AddListener(OnVibrationChanged);
        }
    }
    void OnDisable()
    {
        Seismograph.Instance.vibrationChanged.RemoveListener(OnVibrationChanged);
    }


    private void Update()
    {
        UpdateVisualization();
    }

    private void OnVibrationChanged()
    {
        StopAllCoroutines();
        StartCoroutine(AdjustVibrationOverTime(adjustDuration));

        int dangerLevel = Seismograph.Instance.GetCurrentDangerLevel();
        background.color = Seismograph.Instance.vibrationDangerLevels[dangerLevel].color;
        triangle.color = Seismograph.Instance.vibrationDangerLevels[dangerLevel].color;
    }

    protected override void UpdateValue(float targetValue)
    {
        base.UpdateValue(targetValue);
    }

    private void UpdateVisualization()
    {
        float oscillation = Mathf.Sin(Time.time * vibrationFrequency);
        float oscillationMod = Mathf.Sin(Time.time * overlay1Frequency);
        float noise = Mathf.PerlinNoise(Time.time * noiseFrequency, Value * noiseFrequency);

        // Compute raw sum and scale it
        float rawSum = oscillation + oscillationMod + noise;
        float scaleFactor = 1f / (1f + Mathf.Abs(rawSum));
        float scaledOscillation = rawSum * scaleFactor * (Value / maxValue);

        // Update vibration history
        vibrationHistory.Enqueue(scaledOscillation);
        if (vibrationHistory.Count > pointAmount)
            vibrationHistory.Dequeue();

        // Update line renderer
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

    private Vector3 GetRelativeLinePos(int index, float value)
    {
        float xPosition = boundsEnd.localPosition.x - (index * (width / pointAmount));
        float yPosition = Mathf.Clamp(value / maxVibration * height, -maxVibration, maxVibration);
        return new Vector3(xPosition, yPosition * 10, 0) * 100;
    }

    private IEnumerator AdjustVibrationOverTime(float duration)
    {
        float targetVibration = Seismograph.Instance.GetTotalVibration();
        float startVibration = Value;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            UpdateValue(Mathf.Lerp(startVibration, targetVibration, t));
            yield return null;
        }

        UpdateValue(targetVibration);
    }

    private void OnDestroy()
    {
        if (Seismograph.Instance != null)
        {
            Seismograph.Instance.vibrationChanged.RemoveListener(OnVibrationChanged);
        }
    }
}