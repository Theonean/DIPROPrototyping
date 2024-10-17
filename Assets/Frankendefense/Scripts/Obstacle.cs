using System.Linq;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public string[] destructiveTags;
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