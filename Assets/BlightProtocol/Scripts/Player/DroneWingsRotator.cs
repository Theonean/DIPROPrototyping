using UnityEngine;

public class DroneWingsRotator : MonoBehaviour
{
    [SerializeField]
    private PlayerCore m_playerCore; // Needed for moveDirection, currentSpeed and MaxSpeed
    public float maxAngle;

    public GameObject wingTop;
    public GameObject wingBottom;
    public GameObject wingLeft;
    public GameObject wingRight;

    private Quaternion originalRotationTop;
    private Quaternion originalRotationBottom;
    private Quaternion originalRotationLeft;
    private Quaternion originalRotationRight;

    void Start()
    {
        // Store the original rotations
        originalRotationTop = wingTop.transform.localRotation;
        originalRotationBottom = wingBottom.transform.localRotation;
        originalRotationLeft = wingLeft.transform.localRotation;
        originalRotationRight = wingRight.transform.localRotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float lerpRotation = Mathf.Lerp(0, maxAngle, DroneMovement.Instance.currentSpeed / DroneMovement.Instance.moveSpeed);

        // Rotate top and bottom wings for left-right movement
        if (DroneMovement.Instance.moveDirection.x != 0)
        {
            float rotationZ = DroneMovement.Instance.moveDirection.x < 0 ? -lerpRotation : lerpRotation;
            wingTop.transform.localRotation = originalRotationTop * Quaternion.Euler(0, 0, -rotationZ);
            wingBottom.transform.localRotation = originalRotationBottom * Quaternion.Euler(0, 0, rotationZ);
        }
        else
        {
            // Return to original rotation
            wingTop.transform.localRotation = Quaternion.Lerp(wingTop.transform.localRotation, originalRotationTop, Time.fixedDeltaTime * 2);
            wingBottom.transform.localRotation = Quaternion.Lerp(wingBottom.transform.localRotation, originalRotationBottom, Time.fixedDeltaTime * 2);
        }

        // Rotate left and right wings for up-down movement
        if (DroneMovement.Instance.moveDirection.z != 0)
        {
            float rotationZ = DroneMovement.Instance.moveDirection.z < 0 ? -lerpRotation : lerpRotation;
            wingLeft.transform.localRotation = originalRotationLeft * Quaternion.Euler(0, 0, -rotationZ);
            wingRight.transform.localRotation = originalRotationRight * Quaternion.Euler(0, 0, rotationZ);
        }
        else
        {
            // Return to original rotation
            wingLeft.transform.localRotation = Quaternion.Lerp(wingLeft.transform.localRotation, originalRotationLeft, Time.fixedDeltaTime * 2);
            wingRight.transform.localRotation = Quaternion.Lerp(wingRight.transform.localRotation, originalRotationRight, Time.fixedDeltaTime * 2);
        }
    }
}
