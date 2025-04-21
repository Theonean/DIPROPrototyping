using UnityEngine;

public class PersicopeCamera : MonoBehaviour
{
    public static PersicopeCamera Instance { get; private set; }

    [Header("Free Look")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private Vector2 yRotationBounds; // Min and max rotation angles in degrees

    private float currentYRotation = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void RotateCam(Vector3 direction)
    {
        // Only use the y component for rotation (left/right movement)
        float rotationAmount = direction.y * moveSpeed;
        
        // Update current rotation with clamping
        currentYRotation += rotationAmount;
        currentYRotation = Mathf.Clamp(currentYRotation, yRotationBounds.x, yRotationBounds.y);
        
        // Apply the rotation in local space
        transform.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);
    }

    void Update()
    {
        // Optional: You can add debug visualization here if needed
        Debug.DrawRay(transform.position, transform.forward * 10f, Color.green);
    }

    public void Reset()
    {
        currentYRotation = 0f;
        transform.localRotation = Quaternion.identity;
    }
}