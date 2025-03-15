using UnityEngine;
using UnityEngine.VFX;

public class VisualEffectHandler : MonoBehaviour
{
    private VisualEffect effect;

    public bool useTimeToDestroy;
    public float timeToDestroy;

    private float timer = 0f;
    public bool destroyParent = false;

    private void Start()
    {
        effect = GetComponent<VisualEffect>();
        if (effect == null )
            effect = GetComponentInChildren<VisualEffect>();
    }

    private void Update()
    {
        if (useTimeToDestroy)
        {
            if (timer > timeToDestroy)
            {
                DestroyThis();
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
        else
        {
            if (timer > 0.2f && !effect.HasAnySystemAwake())
            {
                DestroyThis();
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
    }

    private void DestroyThis()
    {
        if (destroyParent)
            Destroy(gameObject.transform.parent.gameObject);
        else
            Destroy(gameObject);
    }
}
