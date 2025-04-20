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
    public bool BlockInteraction { get; set; }


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

    void OnEnable()
    {
        if (PerspectiveSwitcher.Instance != null)
        {
            PerspectiveSwitcher.Instance.onPerspectiveSwitched.RemoveListener(InitialiseLookMode);
            PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(InitialiseLookMode);
        }

    }

    void OnDisable()
    {
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.RemoveListener(InitialiseLookMode);
    }

    private void InitialiseLookMode()
    {
        if (PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.FPV)
        {
            IsLooking = true;
            switch (lookMode)
            {
                case FPVLookMode.FREELOOK:
                    SetCursorState(true, false);
                    break;
                case FPVLookMode.FREELOOK_TOGGLE:
                    SetCursorState(true, false);
                    break;
                case FPVLookMode.DRAG:
                    SetCursorState(false, true, true);
                    break;
                case FPVLookMode.PARALLAX:
                    SetCursorState(false, true);
                    break;
            }
        }
        else
        {
            SetCursorState(false, true);
        }

    }

    public void SetCursorState(bool locked, bool visible, bool hideCrosshair = false)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.Confined;
        Cursor.visible = visible;

        if (crosshair != null && !hideCrosshair)
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
            SetCursorState(!IsLooking, IsLooking);
        }
        if (IsLooking)
        {
            switch (lookMode)
            {
                case FPVLookMode.FREELOOK:
                case FPVLookMode.FREELOOK_TOGGLE:
                    fpvPlayerCam.UpdateCameraRotation(lookMode, GetLookInput());
                    break;

                case FPVLookMode.PARALLAX:
                case FPVLookMode.DRAG:
                    fpvPlayerCam.UpdateCameraRotation(lookMode, GetLookInput(true));
                    break;

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