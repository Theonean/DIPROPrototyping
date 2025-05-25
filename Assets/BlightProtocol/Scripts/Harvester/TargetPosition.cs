using UnityEngine;

public class TargetPosition : MonoBehaviour
{
    public static TargetPosition Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("ResourcePoint")) {
            transform.position = other.transform.position;
            Harvester.Instance.mover.SetDestination(transform.position);
        }
    }
}
