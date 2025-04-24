using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private Vector3 _originalPos;
    private Coroutine _shakeCoroutine;
    private bool _isShaking = false;

    void Awake()
    {
        _originalPos = transform.localPosition; // Initialize in Awake
    }

    public void StartShake(float magnitude, float fadeInTime = 0f)
    {
        if (_isShaking) return; // Prevent overlapping shakes

        _isShaking = true;
        if (_shakeCoroutine != null)
            StopCoroutine(_shakeCoroutine); // Safely stop previous shake
        
        _shakeCoroutine = StartCoroutine(Shake(magnitude, fadeInTime));
    }

    public void StopShake(float fadeOutTime = 0.5f)
    {
        if (!_isShaking) return;

        if (_shakeCoroutine != null)
            StopCoroutine(_shakeCoroutine);
        
        StartCoroutine(FadeOutShake(fadeOutTime));
    }

    private IEnumerator Shake(float magnitude, float fadeInTime)
    {
        float elapsed = 0f;

        // Fade in
        while (elapsed < fadeInTime)
        {
            float currentMagnitude = Mathf.Lerp(0f, magnitude, elapsed / fadeInTime);
            ApplyShake(currentMagnitude);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Infinite shake at full magnitude
        while (true)
        {
            ApplyShake(magnitude);
            yield return null;
        }
    }

    private IEnumerator FadeOutShake(float fadeOutTime)
    {
        float elapsed = 0f;
        Vector3 initialOffset = transform.localPosition - _originalPos;
        float initialMagnitude = initialOffset.magnitude;

        while (elapsed < fadeOutTime)
        {
            float currentMagnitude = Mathf.Lerp(initialMagnitude, 0f, elapsed / fadeOutTime);
            ApplyShake(currentMagnitude);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originalPos;
        _isShaking = false;
    }

    private void ApplyShake(float magnitude)
    {
        // Smoother shake using Perlin noise
        float x = (Mathf.PerlinNoise(Time.time * 10f, 0f) - 0.5f) * 2f * magnitude;
        float y = (Mathf.PerlinNoise(0f, Time.time * 10f) - 0.5f) * 2f * magnitude;
        transform.localPosition = _originalPos + new Vector3(x, y, 0f);
    }
}