using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;  // Import this to use coroutines

public class RocketAimController : MonoBehaviour
{
    public GameObject[] rocketBodies;
    public GameObject[] rocketPropulsions;
    public GameObject[] rocketFronts;

    public Rocket Rocket1; //northwest
    public Rocket Rocket2; //Northeast
    public Rocket Rocket3; //Southeast
    public Rocket Rocket4; //Southwest
    public CanvasGroup outOfLegsGroup; //UI element to quickly flash red when out of Rockets
    public AnimationCurve outOfLegsFlashCurve; //Curve for the out of Rockets flash
    public float outOfLegsFlashDuration; //Duration of the out of Rockets flash    
    private Rocket activeRocket; // The Rocket currently tracked and attached to the core

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

    private void Start()
    {
        playerCore = PlayerCore.Instance;
        frankenGameManager = FrankenGameManager.Instance;
        perspectiveSwitcher = PerspectiveSwitcher.Instance;
    }

    //Detect if a click happens, then call "LegClicked", when released call "LegReleased" on the same Rocket
    void Update()
    {
        if (playerCore.isDead || frankenGameManager.isPaused || perspectiveSwitcher.currentPerspective != CameraPerspective.DRONE)
        {
            return;
        }

        // 1. Track the Rocket with the lowest index that is attached to the core
        activeRocket = GetLowestIndexAttachedLeg();

        // Rotate the object towards the mouse, but only if allowed
        if (canRotate)
        {
            RotateTowardsMouse();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask)) return;

            Logger.Log("Hit tag" + hit.collider.gameObject.tag + " name " + hit.collider.name, LogLevel.INFO, LogType.ROCKETS);

            //Check if a leg was clicked, if yes return
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("RocketComponent"))
            {
                Rocket rocket = hit.collider.gameObject.GetComponentInParent<Rocket>();
                if (rocket.CanExplode())
                {
                    rocket.Explode();
                }
                return;
            }

            if (!activeRocket.IsUnityNull())
            {
                activeRocket.Shoot(hit.point);
                StartCoroutine(StartRotationDelay());
            }
            else
            {
                if (canvasFlashRoutine != null)
                    StopCoroutine(canvasFlashRoutine);

                canvasFlashRoutine = StartCoroutine(FlashOutOfRockets());
            }
        }
    }

    // Coroutine to introduce delay before core rotation
    private IEnumerator StartRotationDelay()
    {
        canRotate = false;  // Stop rotation temporarily
        yield return new WaitForSeconds(rotationDelay);  // Wait for the delay
        canRotate = true;  // Allow rotation again
    }

    // Function to track the Rocket with the lowest index that is attached to the core
    private Rocket GetLowestIndexAttachedLeg()
    {
        // Check Rockets in order of index to find the first attached Rocket
        if (Rocket1.state == RocketState.ATTACHED)
        {
            return Rocket1;
        }
        if (Rocket2.state == RocketState.ATTACHED)
        {
            return Rocket2;
        }
        if (Rocket3.state == RocketState.ATTACHED)
        {
            return Rocket3;
        }
        if (Rocket4.state == RocketState.ATTACHED)
        {
            return Rocket4;
        }

        // Return null if no Rockets are attached
        return null;
    }

    // Function to rotate the object so the tracked Rocket points towards the mouse
    private void RotateTowardsMouse()
    {
        // Raycast to find the mouse position in world space
        if (Camera.main == null)
        {
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

            // Get the rotation offset for the active Rocket's quadrant
            float quadrantRotationOffset;
            if (activeRocket == null)
                quadrantRotationOffset = NorthWestOffset;
            else
                quadrantRotationOffset = GetLegRotationOffset(activeRocket);

            // Apply the offset to ensure the correct Rocket quadrant faces the mouse
            targetRotation = Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y + quadrantRotationOffset, targetRotation.eulerAngles.z);

            // Smoothly interpolate between current and target rotations for the core
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    // Function to determine the rotation offset based on the active Rocket's quadrant
    private float GetLegRotationOffset(Rocket rocket)
    {
        if (rocket == Rocket1) return NorthWestOffset;  // Northwest Rocket
        if (rocket == Rocket2) return -NorthWestOffset;   // Northeast Rocket
        if (rocket == Rocket3) return -SouthWestOffset;  // Southeast Rocket
        if (rocket == Rocket4) return SouthWestOffset; // Southwest Rocket

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
