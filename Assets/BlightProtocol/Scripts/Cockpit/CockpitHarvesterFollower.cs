using UnityEngine;

public class FPVHarvesterFollower : MonoBehaviour
{
    public Transform target;

    void Start()
    {
        transform.position = target.position;
    }

    void Update()
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
