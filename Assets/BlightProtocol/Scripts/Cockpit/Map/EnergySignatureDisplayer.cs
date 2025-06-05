using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnergySignatureDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject areaRing;

    public void DisplaySignature(EnergySignature signature)
    {
        signature.onDisable.AddListener(DestroyThis);
        signature.displayer = this;
        
        GameObject  instantiatedBase = Instantiate(signature.pingPrefab, transform);
        EnergySignatureBase baseComponent = instantiatedBase.GetComponent<EnergySignatureBase>();

        if (signature.eMagnitude > 0)
        {
            for (int i = 0; i < signature.eMagnitude; i++)
            {
                baseComponent.magnitudeSprites[i].SetActive(true);
            }
        }

        areaRing.transform.localScale = Vector3.one * signature.areaSize;
    }

    public void FlashSignature(float duration = 0.2f)
    {
        FlashSignature(Color.red, duration);
    }

    public void FlashSignature(Color color, float duration = 0.2f)
    {
        StartCoroutine(FlashSignatureAnimation(color, duration));
    }
    public void FlashAndSetSignature(Color color, float duration = 0.2f)
    {
        StartCoroutine(FlashSignatureAnimation(color, duration, false));
    }

    /// <summary>
    /// Finds all SpriteRenderers under this GameObject, then
    /// quickly flashes them to red and back over a single duration.
    /// </summary>
    /// <param name="flashDuration">Total duration of the flash (seconds).</param>
    public IEnumerator FlashSignatureAnimation(Color color, float flashDuration = 0.2f, bool returnToOriginalColor = true)
    {
        // grab every SpriteRenderer in this hierarchy
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0)
            yield break;

        float half = flashDuration * 0.5f;
        float t = 0f;

        // phase 1: lerp from original → targetColor
        while (t < half)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / half);
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].color = Color.Lerp(Color.white, color, f);
            yield return null;
        }

        if (returnToOriginalColor)
        {
            // phase 2: lerp from targetColor → original
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float f = Mathf.Clamp01(t / half);
                for (int i = 0; i < renderers.Length; i++)
                    renderers[i].color = Color.Lerp(Color.red, Color.white, f);
                yield return null;
            }

            // ensure we end exactly on the original color
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].color = Color.white;
        }
    }


    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}
