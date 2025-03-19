using UnityEngine;
using System.Collections;  // Import this to use coroutines

public class LegDirectionalClickHandler : MonoBehaviour
{
    public GameObject leg1; //northwest
    public GameObject leg2; //Northeast
    public GameObject leg3; //Southeast
    public GameObject leg4; //Southwest
    public CanvasGroup outOfLegsGroup; //UI element to quickly flash red when out of legs
    public AnimationCurve outOfLegsFlashCurve; //Curve for the out of legs flash
    public float outOfLegsFlashDuration; //Duration of the out of legs flash    
    private GameObject activeLeg; // The leg currently tracked and attached to the core
    GameObject lastLegClicked;

    [Header("Raycast")]
    public LayerMask raycastMask;
    private RaycastHit hit;

    [Header("Core Rotation")]
    public float NorthWestOffset;
    public float SouthWestOffset;
    public float rotationDelay = 0.5f; // Delay before rotation starts
    private bool canRotate = true;  // To control rotation after delay
    private Coroutine canvasFlashRoutine;
    private PlayerCore playerCore;
    private FrankenGameManager frankenGameManager;
    private PerspectiveSwitcher perspectiveSwitcher;

    private void Start() {
        playerCore = PlayerCore.Instance;
        frankenGameManager = FrankenGameManager.Instance;
        perspectiveSwitcher = PerspectiveSwitcher.Instance;
    }

    //Detect if a click happens, then call "LegClicked", when released call "LegReleased" on the same leg
    void Update()
    {
        if (playerCore.isDead || frankenGameManager.isPaused || perspectiveSwitcher.currentPerspective != CameraPerspective.DRONE)
        {
            return;
        }

        // 1. Track the leg with the lowest index that is attached to the core
        activeLeg = GetLowestIndexAttachedLeg();

        // Rotate the object towards the mouse, but only if allowed
        if (canRotate)
        {
            RotateTowardsMouse();
        }

        if (Input.GetMouseButtonDown(0))
        {
            //Raycast to find the position to fly to, and if nothing is hit return
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask)) return;
            //Check if a leg was clicked, if yes return
            if (hit.collider.gameObject.CompareTag("Leg"))
            {
                return;
            }
            //If the clickable part of the leg was clicked, explode the leg
            else if (hit.collider.gameObject.CompareTag("LegClickable"))
            {
                hit.collider.gameObject.GetComponentInParent<LegHandler>().ExplodeLeg();
                return;
            }

            if (activeLeg != null)
            {
                // Make the tracked leg the one to shoot
                lastLegClicked = activeLeg;
                lastLegClicked.GetComponent<LegHandler>().LegClicked();

                // Start the rotation delay coroutine
                StartCoroutine(StartRotationDelay());
            }
            else if (activeLeg == null)
            {
                if (canvasFlashRoutine != null)
                {
                    StopCoroutine(canvasFlashRoutine);
                }

                canvasFlashRoutine = StartCoroutine(FlashOutOfRockets());
            }
        }

        if (Input.GetMouseButtonUp(0) && lastLegClicked != null)
        {
            lastLegClicked.GetComponent<LegHandler>().LegReleased(hit);
            lastLegClicked = null;
        }
    }

    // Coroutine to introduce delay before core rotation
    private IEnumerator StartRotationDelay()
    {
        canRotate = false;  // Stop rotation temporarily
        yield return new WaitForSeconds(rotationDelay);  // Wait for the delay
        canRotate = true;  // Allow rotation again
    }

    // Function to track the leg with the lowest index that is attached to the core
    private GameObject GetLowestIndexAttachedLeg()
    {
        // Check legs in order of index to find the first attached leg
        if (leg1.GetComponent<LegHandler>().m_LegState == LegState.ATTACHED)
        {
            return leg1;
        }
        if (leg2.GetComponent<LegHandler>().m_LegState == LegState.ATTACHED)
        {
            return leg2;
        }
        if (leg3.GetComponent<LegHandler>().m_LegState == LegState.ATTACHED)
        {
            return leg3;
        }
        if (leg4.GetComponent<LegHandler>().m_LegState == LegState.ATTACHED)
        {
            return leg4;
        }

        // Return null if no legs are attached
        return null;
    }

    // Function to rotate the object so the tracked leg points towards the mouse
    private void RotateTowardsMouse()
    {
        // Raycast to find the mouse position in world space
        if (Camera.main == null) {
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 direction = hit.point - transform.position; // Direction from the core to the mouse
            direction.y = 0; // Keep rotation only on the XZ plane

            // Get the current rotation of the core gameobject
            Quaternion currentRotation = transform.rotation;

            // Calculate the desired rotation towards the mouse
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Preserve the original X rotation by overriding it
            targetRotation = Quaternion.Euler(currentRotation.eulerAngles.x, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);

            // Get the rotation offset for the active leg's quadrant
            float quadrantRotationOffset;
            if (activeLeg == null)
                quadrantRotationOffset = NorthWestOffset;
            else
                quadrantRotationOffset = GetLegRotationOffset(activeLeg);

            // Apply the offset to ensure the correct leg quadrant faces the mouse
            targetRotation = Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y + quadrantRotationOffset, targetRotation.eulerAngles.z);

            // Smoothly interpolate between current and target rotations for the core
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    // Function to determine the rotation offset based on the active leg's quadrant
    private float GetLegRotationOffset(GameObject leg)
    {
        if (leg == leg1) return NorthWestOffset;  // Northwest leg
        if (leg == leg2) return -NorthWestOffset;   // Northeast leg
        if (leg == leg3) return -SouthWestOffset;  // Southeast leg
        if (leg == leg4) return SouthWestOffset; // Southwest leg

        return 0f; // Default, no offset
    }

    private IEnumerator FlashOutOfRockets()
    {
        float t = 0;
        while (t < outOfLegsFlashDuration)
        {
            t += Time.deltaTime;
            outOfLegsGroup.alpha = outOfLegsFlashCurve.Evaluate(t / outOfLegsFlashDuration);
            yield return null;
        }
    }
}
