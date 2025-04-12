using UnityEngine;

public class FPVInputManager : MonoBehaviour
{
    public static FPVInputManager Instance { get; private set; }

    [Header("Settings")]
    public FPVLookMode lookMode = FPVLookMode.FREELOOK;
    public KeyCode freeLookToggle = KeyCode.E;

    [Header("References")]
    [SerializeField] private GameObject crosshair;
    private FPVPlayerCam fpvPlayerCam;

    public bool IsLooking { get; private set; }
    public bool IsInteracting { get; private set; }

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
        fpvPlayerCam = FPVPlayerCam.Instance;
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(InitialiseLookMode);
    }

    private void InitialiseLookMode()
    {
        if (PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.FPV)
        {
            IsLooking = true;
            switch (lookMode)
            {
                case FPVLookMode.FREELOOK:
                    SetCursorState(true);
                    break;
                case FPVLookMode.FREELOOK_TOGGLE:
                    SetCursorState(false);
                    break;
                case FPVLookMode.PARALLAX:
                    SetCursorState(false);
                    break;
            }
        }
        else
        {
            SetCursorState(false);
        }

    }

    public void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.Confined;
        Cursor.visible = !locked;

        if (crosshair != null)
        {
            crosshair.SetActive(locked);
        }
    }

    public void SetInteractionState(bool interacting)
    {
        IsInteracting = interacting;
        if (interacting)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            IsLooking = false;
            if (crosshair != null) crosshair.SetActive(false);
        }
        else
        {
            InitialiseLookMode();
        }
    }

    public void Update()
    {
        if (lookMode == FPVLookMode.FREELOOK_TOGGLE && Input.GetKeyDown(freeLookToggle))
        {
            SetCursorState(!IsLooking);
        }
        if (IsLooking)
        {
            if (lookMode == FPVLookMode.FREELOOK || lookMode == FPVLookMode.FREELOOK_TOGGLE)
            {
                fpvPlayerCam.UpdateCameraRotation(lookMode, GetLookInput());
            }
            else if (lookMode == FPVLookMode.PARALLAX)
            {
                fpvPlayerCam.UpdateCameraRotation(lookMode, GetLookInput(true));
            }
        }
    }

    public Vector2 GetLookInput(bool normalized = false)
    {
        if (normalized)
        {
            return new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
        );
        }
        return new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );
    }

    public Vector2 GetNormalizedMousePosition()
    {
        return new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
        );
    }
}