using UnityEngine;

public class FPVHarvesterFollower : MonoBehaviour
{
    public static FPVHarvesterFollower Instance;
    public bool isRotating;
    [SerializeField] private float rotationDeltaThreshold = 1f;
    private float lastYRotation;
    public Transform target;

    void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(this);
        }
        else {
            Instance = this;
        }
    }


    void Start()
    {
        transform.position = target.position;
    }

    void Update()
    {
        transform.position = target.position;
        lastYRotation = transform.rotation.eulerAngles.y;
        if (Mathf.Abs(target.rotation.eulerAngles.y - lastYRotation) > rotationDeltaThreshold) isRotating = true;
        else isRotating = false;
        transform.rotation = target.rotation;
    }
}
