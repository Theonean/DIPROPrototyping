using System.Collections;
using UnityEngine;

public class FPVPlayerCam : MonoBehaviour
{
    public static FPVPlayerCam Instance { get; private set; }

    public bool isLocked;

    [Header("Drag Look")]
    [SerializeField] private float dragSens = 5f;
    [SerializeField] private Vector2 xRotationLimits;
    [SerializeField] private Vector2 yRotationLimits;

    private float currentXRotation = 0f;
    private float currentYRotation = 0f;
    private Vector2 lastMousePos;

    private Quaternion initialRot;

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

        initialRot = transform.localRotation;
    }

    void Start()
    {
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(OnPerspectiveSwitched);       
    }

    public void UpdateCameraRotation(Vector2 input)
    {
        if (isLocked) return;
        Vector2 mouseDelta = input - lastMousePos;
        lastMousePos = input;

        currentYRotation -= mouseDelta.x * dragSens;
        currentXRotation += mouseDelta.y * dragSens;

        currentXRotation = Mathf.Clamp(currentXRotation, xRotationLimits.x, xRotationLimits.y);
        currentYRotation = Mathf.Clamp(currentYRotation, yRotationLimits.x, yRotationLimits.y);

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

    private void OnPerspectiveSwitched() {
        switch(PerspectiveSwitcher.Instance.currentPerspective) {
            case CameraPerspective.SWITCHING:
                transform.localRotation = initialRot;
            break;
        }
    }

    public void ResetRotation(float duration) {
        isLocked = true;
        StartCoroutine(LerpRotation(initialRot, duration));
    }

    private IEnumerator LerpRotation(Quaternion targetRot, float duration) {
        Quaternion startRot = transform.localRotation;
        float elapsedTime = 0;
        while (elapsedTime < duration)  {
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = targetRot;
        isLocked = false;
    }
}