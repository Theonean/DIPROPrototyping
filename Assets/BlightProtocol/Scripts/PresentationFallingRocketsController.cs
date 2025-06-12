using UnityEngine;

public class PresentationFallingRocketsController : MonoBehaviour
{
    private Rigidbody[] rigidbodies;

    void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    public void MakeRocketsFall()
    {
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }
    }
}
