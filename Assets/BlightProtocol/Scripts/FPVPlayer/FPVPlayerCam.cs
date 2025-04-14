using UnityEngine;

public enum FPVLookMode
{
    FREELOOK_TOGGLE,
    FREELOOK,
    PARALLAX,
    DRAG,
}

public class FPVPlayerCam : MonoBehaviour
{
    public static FPVPlayerCam Instance { get; private set; }
    private FPVInteractionHandler interactionHandler;

    [Header("Free Look")]
    public float freeSens = 300f;

    [Header("Edge Look")]
    public float edgeSens = 5f;
    public float lookZoneX = 0.1f;
    public float lookZoneY = 0.1f;
    public Vector2 xRotationLimit = new Vector2(-30, 30); // Up/down limits
    public Vector2 yRotationLimit = new Vector2(-45, 45); // Left/right limits

    [Header("Drag Look")]
    public float dragSens = 5f;

    private float xRotation;
    private float yRotation;

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

    void Start()
    {
        interactionHandler = FPVInteractionHandler.Instance;
    }

    public void UpdateCameraRotation(FPVLookMode lookMode, Vector2 input)
    {
        switch (lookMode)
        {
            case FPVLookMode.FREELOOK:
            case FPVLookMode.FREELOOK_TOGGLE:
                FreeLook(input);
                break;
            case FPVLookMode.PARALLAX:
                EdgeLook(input);
                break;
            case FPVLookMode.DRAG:
                DragLook(input);
                break;
        }
    }

    private void FreeLook(Vector2 input)
    {
        yRotation += input.x * freeSens * Time.deltaTime;
        xRotation -= input.y * freeSens * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    private void EdgeLook(Vector2 normalizedMousePos)
    {
        Vector2 screenCenter = new Vector2(0.5f, 0.5f);
        Vector2 mouseOffset = normalizedMousePos - screenCenter;
        float distanceFromCenter = mouseOffset.magnitude;

        if (distanceFromCenter > lookZoneX)
        {
            float edgeZoneProgress = Mathf.InverseLerp(lookZoneX, 0.5f, distanceFromCenter);
            Vector2 lookDirection = mouseOffset.normalized;

            float targetYRotation = lookDirection.x * yRotationLimit.y * edgeZoneProgress;
            float targetXRotation = -lookDirection.y * xRotationLimit.y * edgeZoneProgress;

            yRotation = Mathf.Lerp(yRotation, targetYRotation, edgeSens * Time.deltaTime);
            xRotation = Mathf.Lerp(xRotation, targetXRotation, edgeSens * Time.deltaTime);

            xRotation = Mathf.Clamp(xRotation, xRotationLimit.x, xRotationLimit.y);
            yRotation = Mathf.Clamp(yRotation, yRotationLimit.x, yRotationLimit.y);
        }
        else
        {
            yRotation = Mathf.Lerp(yRotation, 0f, edgeSens * Time.deltaTime * 2f);
            xRotation = Mathf.Lerp(xRotation, 0f, edgeSens * Time.deltaTime * 2f);
        }

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    Vector2 lastMousePos;
    private float currentXRotation = 0f;
    private float currentYRotation = 0f;
    void DragLook(Vector2 input)
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePos = input;
            // Get current rotation and normalize it
            Vector3 currentEuler = transform.localEulerAngles;
            currentXRotation = NormalizeEulerAngle(currentEuler.x);
            currentYRotation = NormalizeEulerAngle(currentEuler.y);
            FPVInputManager.Instance.BlockInteraction = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            FPVInputManager.Instance.BlockInteraction = false;
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 mouseDelta = input - lastMousePos;
            lastMousePos = input;

            currentYRotation -= mouseDelta.x * dragSens;
            currentXRotation += mouseDelta.y * dragSens;

            currentXRotation = Mathf.Clamp(currentXRotation, -45f, 45f);

            transform.localRotation = Quaternion.Euler(currentXRotation, currentYRotation, 0);
        }
    }
    private float NormalizeEulerAngle(float angle)
    {
        angle = angle % 360;
        if (angle > 180)
        {
            angle -= 360;
        }
        return angle;
    }
}