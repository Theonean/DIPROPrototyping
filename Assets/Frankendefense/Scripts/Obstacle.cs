using System.Linq;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public string[] destructiveTags;
    public SkinnedMeshRenderer meshRenderer;

    private void Start()
    {
        //Change the Blendshape values to random
        meshRenderer.SetBlendShapeWeight(0, Random.Range(0f, 100f));
        meshRenderer.SetBlendShapeWeight(1, Random.Range(0f, 100f));
        meshRenderer.SetBlendShapeWeight(2, Random.Range(0f, 100f));
        meshRenderer.SetBlendShapeWeight(3, Random.Range(0f, 100f));

        //Reload the meshrenderer
        meshRenderer.UpdateGIMaterials();
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
            Destroy(gameObject);
        }
    }
}