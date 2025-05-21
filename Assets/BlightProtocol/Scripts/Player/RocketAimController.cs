using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class RocketAimController : MonoBehaviour
{
    public static RocketAimController Instance;

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

    public UnityEvent OnRocketShot = new UnityEvent();
    public UnityEvent OnRocketExplode = new UnityEvent();
    public UnityEvent OnRocketRetract = new UnityEvent();
    
    private Rocket[] allRockets;
    private int currentRocketIndex = 0;


    private void Awake()
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

    private void Start()
    {
        playerCore = PlayerCore.Instance;
        frankenGameManager = FrankenGameManager.Instance;
        perspectiveSwitcher = PerspectiveSwitcher.Instance;

        allRockets = new Rocket[] { Rocket1, Rocket2, Rocket3, Rocket4 };

        //Set each rocket to it's coressponding setting from UISelectedRocketManager ->private (RocketComponentType componentType, GameObject newComponent) GetRocketSettingsFromPlayerPrefs
        //Check if it is not null
    }

    //Detect if a click happens, then call "LegClicked", when released call "LegReleased" on the same Rocket
    void Update()
    {
        if (playerCore.isDead || frankenGameManager.isPaused || perspectiveSwitcher.currentPerspective != CameraPerspective.DRONE)
        {
            return;
        }

        HandleRocketScrollInput();

        if(activeRocket == null || activeRocket.state != RocketState.ATTACHED)
        {
            activeRocket = GetLowestIndexAttachedRocket();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask)) return;

            Logger.Log("Hit tag" + hit.collider.gameObject.tag + " name " + hit.collider.name, LogLevel.INFO, LogType.ROCKETS);

            //Check if a Rocket was clicked, if yes return
            if (hit.collider.gameObject.CompareTag("Rocket"))
            {
                Rocket rocket = hit.collider.gameObject.GetComponentInParent<Rocket>();
                if (rocket.CanExplode())
                {
                    rocket.Explode();
                    OnRocketExplode?.Invoke();
                }
                return;
            }

            if (activeRocket != null)
            {
                activeRocket.Shoot(hit.point);
                SelectNextRocket(1);
                OnRocketShot?.Invoke();
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

    private void FixedUpdate()
    {
        // Rotate the object towards the mouse, but only if allowed
        if (canRotate)
        {
            RotateTowardsMouse();
        }
    }

    // Coroutine to introduce delay before core rotation
    private IEnumerator StartRotationDelay()
    {
        canRotate = false;  // Stop rotation temporarily
        yield return new WaitForSeconds(rotationDelay);  // Wait for the delay
        canRotate = true;  // Allow rotation again
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

            // Get the rotation offset for the active Rocket's quadrant
            float quadrantRotationOffset;
            if (activeRocket == null)
                quadrantRotationOffset = NorthWestOffset;
            else
                quadrantRotationOffset = GetLegRotationOffset(activeRocket);

            // Apply the offset to ensure the correct Rocket quadrant faces the mouse
            direction = Quaternion.Euler(0, quadrantRotationOffset, 0) * direction;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly interpolate between current and target rotations for the core
            float maxDegreesPerSecond = 360f; // you can tweak this for desired speed
            transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, maxDegreesPerSecond * Time.deltaTime);
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

    private void HandleRocketScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0) return;

        int direction = scroll > 0 ? 1 : -1;
        SelectNextRocket(direction);
    }

    private void SelectNextRocket(int indexDirection)
    {
        int startIndex = currentRocketIndex;

        for (int i = 0; i < allRockets.Length; i++)
        {
            currentRocketIndex = (currentRocketIndex + indexDirection + allRockets.Length) % allRockets.Length;

            if (allRockets[currentRocketIndex].state == RocketState.ATTACHED)
            {
                activeRocket = allRockets[currentRocketIndex];
                return;
            }
        }

        // No attached rocket found, reset to original index
        currentRocketIndex = startIndex;
    }
    private Rocket GetLowestIndexAttachedRocket()
    {
        for (int i = 0; i < allRockets.Length; i++)
        {
            if (allRockets[i].state == RocketState.ATTACHED)
            {
                currentRocketIndex = i;
                return allRockets[i];
            }
        }
        return null;
    }


}
