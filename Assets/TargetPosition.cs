using UnityEngine;

public class TargetPosition : MonoBehaviour
{
    public bool isOnResourcePoint = false;
    public GameObject activeResourcePoint;
    private Rigidbody rb;

    void Start() {
        rb = GetComponentInChildren<Rigidbody>();
    }

    void OnTriggerEnter(Collider other) {
        Debug.Log(other.gameObject.name);
        if (other.CompareTag("ResourcePoint")) {
            isOnResourcePoint = true;
            transform.position = other.transform.position;
            Harvester.Instance.mover.SetDestination(transform.position);
            activeResourcePoint = other.transform.parent.gameObject;
        }
    }

    void OTriggerExit(Collider other)
    {
        if (other.CompareTag("ResourcePoint")) {
            isOnResourcePoint = false;
            Debug.Log("Exit Resource Point");
        }
    }
}
