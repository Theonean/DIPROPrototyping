using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class EnergySignatureDisplayer : MonoBehaviour
{
    [SerializeField] private List<EnergySignatureBase> classSprites;
    [SerializeField] private List<GameObject> frequencySprites;
    [SerializeField] private SpriteRenderer frequencySprite;
    [SerializeField] private List<GameObject> magnitudeSprites;

    [SerializeField] private float fadeDelay = 5f;
    [SerializeField] private float fadeDuration = 5f;
    [SerializeField] private Color baseActiveColor = Color.white;
    [SerializeField] private Color frequencyActiveColor = Color.black;
    [SerializeField] private Color fadedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private SpriteRenderer activeClass;
    private SpriteRenderer activeFrequency;
    private SpriteRenderer[] activeMagnitudes;
    [SerializeField] float lifeTime = 30f; 
    public LineRenderer lineRenderer;

    public void DisplaySignature(EnergySignature signature)
    {
        classSprites[signature.eClass].gameObject.SetActive(true);
        activeClass = classSprites[signature.eClass].GetComponent<SpriteRenderer>();

        frequencySprites[signature.eFrequency].SetActive(true); ;
        activeFrequency = frequencySprites[signature.eFrequency].GetComponent<SpriteRenderer>();

        activeMagnitudes = new SpriteRenderer[signature.eMagnitude];
        for (int i = 0; i < signature.eMagnitude; i++)
        {
            magnitudeSprites[i].SetActive(true);
            magnitudeSprites[i].transform.position = classSprites[signature.eClass].magnitudePositions[i].position;
            activeMagnitudes[i] = magnitudeSprites[i].GetComponent<SpriteRenderer>();
        }
        ResetSignatureColor();
        if (!signature.isStatic) {
            Invoke(nameof(DestroySelf), lifeTime);
        }
    }

    private void DestroySelf() {
        Destroy(gameObject);
    }

    private IEnumerator FadeSignature()
    {
        yield return new WaitForSeconds(fadeDelay);

        float elapsedTime = 0f;
        Color startColor = activeClass.color;
        Color startFrequencyColor = activeFrequency.color;
        float startAlpha = startColor.a;

        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            Color newColor = Color.Lerp(startColor, fadedColor, t);
            float newAlpha = Mathf.Lerp(startAlpha, fadedColor.a, t);

            activeClass.color = newColor;
            lineRenderer.material.SetColor("_Color", newColor);
            activeFrequency.color = new Color(startFrequencyColor.r, startFrequencyColor.g, startFrequencyColor.b, newAlpha);

            for (int i = 0; i < activeMagnitudes.Length; i++)
            {
                activeMagnitudes[i].color = newColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final faded state
        activeClass.color = fadedColor;
        lineRenderer.material.SetColor("_Color", fadedColor);
        activeFrequency.color = new Color(startFrequencyColor.r, startFrequencyColor.g, startFrequencyColor.b, fadedColor.a);
        for (int i = 0; i < activeMagnitudes.Length; i++)
        {
            activeMagnitudes[i].color = fadedColor;
        }
    }
    public void ResetSignatureColor()
    {
        activeClass.color = baseActiveColor;
        activeFrequency.color = frequencyActiveColor;
        for (int i = 0; i < activeMagnitudes.Length; i++)
        {
            activeMagnitudes[i].color = baseActiveColor;
        }
        StartCoroutine(FadeSignature());
    }


}
