using UnityEngine;

public class RadarPing : MonoBehaviour
{
    public float fadeTimeMax = 1f;
    public float scaleMax = 5f;
    private float fadeTimer = 0f;
    public Color color = Color.red;
    public bool animateScale = false;

    private SpriteRenderer spriteRenderer;
    void Start()
    {
        /*spriteRenderer = GetComponent<SpriteRenderer>();
        if (animateScale) {
            transform.localScale = new Vector3 (2,2,2);
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        fadeTimer += Time.deltaTime;
        /*float fadeProgress = fadeTimer / fadeTimeMax;
        
        // Update sprite transparency
        spriteRenderer.color = new Color(color.r, color.g, color.b, 1 - fadeProgress);
        
        // Gradually scale up
        if (animateScale) {
            float currentScale = Mathf.Lerp(2, scaleMax, fadeProgress);
            transform.localScale = new Vector3(currentScale, currentScale, currentScale);
        }
        
        // Destroy after fade completes*/
        if (fadeTimer > fadeTimeMax) {
            Destroy(gameObject);
        }
    }
}