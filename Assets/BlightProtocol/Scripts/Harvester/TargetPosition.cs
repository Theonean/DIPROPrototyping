using UnityEngine;

public class TargetPosition : MonoBehaviour
{
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("ResourcePoint")) {
            transform.position = other.transform.position;
            Harvester.Instance.mover.SetDestination(transform.position);
        }
    }
}
