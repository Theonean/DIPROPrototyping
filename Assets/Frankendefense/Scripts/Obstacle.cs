using System.Linq;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public string[] destructiveTags;
    void OnTriggerEnter(Collider other)
    {
        if (destructiveTags.Contains(other.gameObject.tag))
        {
            Destroy(gameObject);
        }
    }
}
