using System.Collections;
using UnityEngine;

public class DiscoveredDot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flashDuration;
    [SerializeField] private AnimationCurve flashCurve;
    [SerializeField] private Color flashColor;
    [SerializeField] private float scaleModifier;
    public bool wasDiscovered = false;

    public void OnDiscovered() {
        wasDiscovered = true;
        spriteRenderer.enabled = true;
        StartCoroutine(Flash());
    }

    private IEnumerator Flash() {
        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;
        Vector3 startScale = spriteRenderer.gameObject.transform.localScale;
        while (elapsedTime < flashDuration)  {
            float t = flashCurve.Evaluate(elapsedTime / flashDuration);
            spriteRenderer.color = Color.Lerp(startColor, flashColor, t);
            spriteRenderer.gameObject.transform.localScale = Vector3.Lerp(startScale, startScale * scaleModifier, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
