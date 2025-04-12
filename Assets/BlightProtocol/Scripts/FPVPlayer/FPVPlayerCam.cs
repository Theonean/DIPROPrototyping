using System.Collections;
using TMPro;
using UnityEngine;

public enum FPVLookMode
{
    FREELOOK_TOGGLE,
    FREELOOK,
    EDGELOOK
}
public class FPVPlayerCam : MonoBehaviour
{
    public static FPVPlayerCam Instance { get; private set; }
    public FPVLookMode lookMode = FPVLookMode.FREELOOK;
    [Header("Free Look")]
    private KeyCode freeLookToggle = KeyCode.E;
    public float freeSensX = 300f;
    public float freeSensY = 300f;
    public bool isLooking = false;
    [Header("Edge Look")]
    public float sensX;
    public float sensY;
    public float lookZoneX = 0.1f;
    public float lookZoneY = 0.1f;
    public Vector2 xRotationLimit = new Vector2(-30, 30);
    public Vector2 yRotationLimit = new Vector2(-45, 45);

    float xRotation;
    float yRotation;

    [Header("Lock")]
    public bool isLocked = false;
    private Quaternion lastRotation;
    private Vector3 lastPosition;
    public float transitionDuration = 1f;
    private bool isReparented = false;
    private GameObject defaultParent;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        defaultParent = transform.parent.gameObject;
    }

    void Start()
    {
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(OnPerspectiveSwitched);
    }

    void OnPerspectiveSwitched()
    {
        if (PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.FPV)
        {
            switch(lookMode) {
                case FPVLookMode.FREELOOK:
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = true;
                    break;
                case FPVLookMode.FREELOOK_TOGGLE:
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                    break;
                case FPVLookMode.EDGELOOK:
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                    break;
            }
        }
    }

    private void Update()
    {
        if (isLocked)
        {
            return;
        }

        switch (lookMode)
        {
            case FPVLookMode.FREELOOK:
                FreeLook();
                break;

            case FPVLookMode.FREELOOK_TOGGLE:
                if (Input.GetKeyDown(freeLookToggle))
                {
                    if (isLooking)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        isLooking = false;
                    }
                    else
                    {
                        Cursor.lockState = CursorLockMode.Confined;
                        Cursor.visible = true;
                        isLooking = true;
                    }
                }
                FreeLook();
                break;

            case FPVLookMode.EDGELOOK:
                EdgeLook();
                break;
        }
    }

    private void FreeLook()
    {
        if (isLooking)
        {
            float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * freeSensX;
            float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * freeSensY;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
    }

    private void EdgeLook()
    {
        Vector2 mousePos = new Vector2(
                    Input.mousePosition.x / Screen.width,
                    Input.mousePosition.y / Screen.height
                );

        float xLookAmount = 0f;
        float yLookAmount = 0f;

        if (mousePos.x < lookZoneX)
        {
            xLookAmount = 1f - (mousePos.x / lookZoneX);
        }
        else if (mousePos.x > 1f - lookZoneX)
        {
            xLookAmount = (mousePos.x - (1f - lookZoneX)) / lookZoneX;
        }

        if (mousePos.y < lookZoneY)
        {
            yLookAmount = 1f - (mousePos.y / lookZoneY);
        }
        else if (mousePos.y > 1f - lookZoneY)
        {
            yLookAmount = (mousePos.y - (1f - lookZoneY)) / lookZoneY;
        }

        float targetXRotation = Mathf.Lerp(
            -yRotationLimit.x,
            yRotationLimit.x,
            (mousePos.x - lookZoneX) / (1f - 2f * lookZoneX)
        );

        float targetYRotation = Mathf.Lerp(
            -xRotationLimit.x,
            xRotationLimit.x,
            (mousePos.y - lookZoneY) / (1f - 2f * lookZoneY)
        );

        if (xLookAmount > 0)
        {
            xRotation = Mathf.Lerp(xRotation, targetXRotation, sensY * Time.deltaTime * xLookAmount);
        }

        if (yLookAmount > 0)
        {
            yRotation = Mathf.Lerp(yRotation, targetYRotation, sensY * Time.deltaTime * yLookAmount);
        }

        xRotation = Mathf.Clamp(xRotation, yRotationLimit.x, yRotationLimit.y);
        yRotation = Mathf.Clamp(yRotation, xRotationLimit.x, xRotationLimit.y);

        transform.localRotation = Quaternion.Euler(yRotation, -xRotation, 0);
    }

    public void LockToPosition(Transform targetTransform, bool useInitPos = false, bool reparent = false)
    {
        isLocked = true;
        lastRotation = transform.localRotation;
        lastPosition = transform.localPosition;
        if (useInitPos) return;

        StopAllCoroutines();

        if (reparent)
        {
            transform.parent = targetTransform;
            isReparented = true;

            StartCoroutine(SmoothMove(Quaternion.identity, Vector3.zero));
        }
        else
        {
            Quaternion localTargetRot = transform.parent != null
                ? Quaternion.Inverse(transform.parent.rotation) * targetTransform.rotation
                : targetTransform.rotation;

            Vector3 localTargetPos = transform.parent != null
                ? transform.parent.InverseTransformPoint(targetTransform.position)
                : targetTransform.position;

            StartCoroutine(SmoothMove(localTargetRot, localTargetPos));
        }
        Cursor.visible = false;
        FPVUI.Instance.ToggleFPVCrosshair(false);
        FPVUI.Instance.ToggleLookAtText(false);
    }

    public void UnlockPosition()
    {
        StopAllCoroutines();

        transform.parent = defaultParent.transform;
        isReparented = false;

        StartCoroutine(SmoothMove(lastRotation, lastPosition, true));
        FPVUI.Instance.ToggleFPVCrosshair(true);
        FPVUI.Instance.ToggleLookAtText(true);

        // CLear interaction Text
        FPVUI.Instance.ClearInteractText();
    }

    private IEnumerator SmoothMove(Quaternion targetRot, Vector3 targetPos, bool unlock = false)
    {
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration; // Normalize time
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            transform.localRotation = Quaternion.Lerp(startRot, targetRot, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPos;
        transform.localRotation = targetRot;

        if (unlock)
        {
            isLocked = false;
        }
    }

}
