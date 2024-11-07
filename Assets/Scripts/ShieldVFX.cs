using System.Collections;
using UnityEngine;

public class ShieldVFX : MonoBehaviour
{
    Renderer _renderer;
    public float dissolveSpeed;
    private float startPos;
    private float endPos = 0.65f;
    private Coroutine dissolveCoroutine;

    private void Start()
    {
        _renderer = GetComponentInChildren<Renderer>();
        startPos = _renderer.material.GetFloat("_Dissolve");
    }

    public void ToggleShield(bool direction)
    {
        float target = direction ? startPos : endPos;

        // Stop any ongoing dissolve coroutine to switch directions
        if (dissolveCoroutine != null)
        {
            StopCoroutine(dissolveCoroutine);
        }

        // Start the coroutine with the new target direction
        dissolveCoroutine = StartCoroutine(DissolveShield(target));
    }

    private IEnumerator DissolveShield(float target)
    {
        float start = _renderer.material.GetFloat("_Dissolve");
        float lerp = 0f;

        // Continue until the dissolve value reaches the target
        while (!Mathf.Approximately(start, target))
        {
            lerp += Time.deltaTime * dissolveSpeed;

            // Interpolate between start and target using Mathf.Lerp
            float current = Mathf.Lerp(start, target, lerp);
            _renderer.material.SetFloat("_Dissolve", current);

            // Update start to the last value of current to handle direction changes smoothly
            start = _renderer.material.GetFloat("_Dissolve");

            yield return null;
        }
    }
}
