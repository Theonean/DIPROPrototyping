using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class Obstacle : MonoBehaviour
{
    public string[] destructiveTags;
    public SkinnedMeshRenderer meshRenderer;

    public VisualEffect ExplosionEffect;
    public UnityEvent changedBlendshape;

    public GameObject navObstacleHolder;

    private void Awake()
    {
        RandomizeBlendWeights();
    }
    public void RandomizeBlendWeights()
    {
        meshRenderer.enabled = true;
        GetComponent<Collider>().enabled = true;
        navObstacleHolder.SetActive(true);
        gameObject.SetActive(true);

        //Change the Blendshape values to random
        meshRenderer.SetBlendShapeWeight(0, Random.Range(0f, 100f));
        meshRenderer.SetBlendShapeWeight(1, Random.Range(0f, 100f));
        meshRenderer.SetBlendShapeWeight(2, Random.Range(0f, 100f));
        meshRenderer.SetBlendShapeWeight(3, Random.Range(0f, 100f));

        //Reload the meshrenderer
        meshRenderer.UpdateGIMaterials();
        changedBlendshape.Invoke();
    }

    public void UpdateColor()
    {
        Color regionColor = ProceduralTileGenerator.Instance.biomeColor;

        // Use MaterialPropertyBlock to set color without affecting shared materials
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        if (meshRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
        {
            skinnedMeshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", regionColor);

            /*Color shadowColor1 = skinnedMeshRenderer.material.GetColor("_1st_ShadeColor");
            Color shadowColor2 = skinnedMeshRenderer.material.GetColor("_2nd_ShadeColor");

            propBlock.SetColor("_1st_ShadeColor", regionColor * shadowColor1);
            propBlock.SetColor("_2nd_ShadeColor", regionColor * shadowColor2);*/

            skinnedMeshRenderer.SetPropertyBlock(propBlock);

            // set explosionEffect Color
            ExplosionEffect.SetVector4("_ParticleColor", regionColor);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        HandleCollision(other.gameObject);
    }

    private void HandleCollision(GameObject other)
    {
        if (other.gameObject.tag == "Leg")
        {
            LegHandler leg = other.gameObject.GetComponent<LegHandler>();
            leg.ExplodeLeg();
        }
        else if (destructiveTags.Contains(other.gameObject.tag))
        {
            HandleExplosion();
        }
    }

    private void HandleExplosion()
    {
        ExplosionEffect.Play();
        StartCoroutine(DestroyAfterDelay(2f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(0.2f);
        meshRenderer.enabled = false;
        GetComponent<Collider>().enabled = false;
        navObstacleHolder.SetActive(false);
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}