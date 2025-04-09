using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> meshRenderers;
    public GameObject meshParent;

    public VisualEffect ExplosionEffect;

    public GameObject navObstacleHolder;

    public void UpdateColor()
    {
        Color regionColor = ProceduralTileGenerator.Instance.biomeColor;

        // Use MaterialPropertyBlock to set color without affecting shared materials
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", regionColor);

            /*Color shadowColor1 = skinnedMeshRenderer.material.GetColor("_1st_ShadeColor");
            Color shadowColor2 = skinnedMeshRenderer.material.GetColor("_2nd_ShadeColor");

            propBlock.SetColor("_1st_ShadeColor", regionColor * shadowColor1);
            propBlock.SetColor("_2nd_ShadeColor", regionColor * shadowColor2);*/

            meshRenderer.SetPropertyBlock(propBlock);

            // set explosionEffect Color
            ExplosionEffect.SetVector4("_ParticleColor", regionColor);
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(0.2f);
        GetComponentInChildren<Collider>().enabled = false;
        meshParent.SetActive(false);
        navObstacleHolder.SetActive(false);
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}