using UnityEngine;

public class FPVPlayerCam : MonoBehaviour
{
    public static FPVPlayerCam Instance { get; private set; }

    public bool isLocked;

    [Header("Drag Look")]
    public float dragSens = 5f;

    private float currentXRotation = 0f;
    private float currentYRotation = 0f;
    private Vector2 lastMousePos;

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

    public void UpdateCameraRotation(Vector2 input)
    {
        if (isLocked) return;
        Vector2 mouseDelta = input - lastMousePos;
        lastMousePos = input;

        currentYRotation -= mouseDelta.x * dragSens;
        currentXRotation += mouseDelta.y * dragSens;

        currentXRotation = Mathf.Clamp(currentXRotation, -45f, 45f);

        transform.localRotation = Quaternion.Euler(currentXRotation, currentYRotation, 0);
    }

    public void StartLook(Vector2 input)
    {
        lastMousePos = input;
        // Get current rotation and normalize it
        Vector3 currentEuler = transform.localEulerAngles;
        currentXRotation = NormalizeEulerAngle(currentEuler.x);
        currentYRotation = NormalizeEulerAngle(currentEuler.y);

        FPVInputManager.Instance.lookState = LookState.LOOKING;
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