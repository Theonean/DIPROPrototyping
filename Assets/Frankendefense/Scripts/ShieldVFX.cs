using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldVFX : MonoBehaviour
{
    Renderer _renderer;
    public float dissolveSpeed;
    private float startPos;
    private float endPos = 1.2f;

    private void Start()
    {
        _renderer = GetComponentInChildren<Renderer>();
        startPos = _renderer.material.GetFloat("_Dissolve");
    }

    public void ToggleShield(bool direction)
    {
        if (direction)
            StartCoroutine(DissolveShield(endPos, startPos));
        else
            StartCoroutine(DissolveShield(startPos, endPos));
    }

    private IEnumerator DissolveShield(float start, float target)
    {
        float lerp = 0;
        while (lerp < 1)
        {
            _renderer.material.SetFloat("_Dissolve", Mathf.Lerp(start, target, lerp));
            lerp += Time.deltaTime * dissolveSpeed;
            yield return null;
        }
    }
}
