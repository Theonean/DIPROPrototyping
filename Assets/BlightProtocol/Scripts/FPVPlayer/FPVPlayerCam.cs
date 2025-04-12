using UnityEngine;

public enum FPVLookMode
{
    FREELOOK_TOGGLE,
    FREELOOK,
    PARALLAX
}

public class FPVPlayerCam : MonoBehaviour
{
    public static FPVPlayerCam Instance { get; private set; }

    [Header("Free Look")]
    public float freeSensX = 300f;
    public float freeSensY = 300f;

    [Header("Edge Look")]
    public float edgeSensX = 5f;
    public float edgeSensY = 5f;
    public float lookZoneX = 0.1f;
    public float lookZoneY = 0.1f;
    public Vector2 xRotationLimit = new Vector2(-30, 30); // Up/down limits
    public Vector2 yRotationLimit = new Vector2(-45, 45); // Left/right limits

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
        }
    }

    private void FreeLook(Vector2 input)
    {
        yRotation += input.x * freeSensX * Time.deltaTime;
        xRotation -= input.y * freeSensY * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    private void EdgeLook(Vector2 normalizedMousePos)
    {
        Vector2 screenCenter = new Vector2(0.5f, 0.5f);
        Vector2 mouseOffset = normalizedMousePos - screenCenter;
        float distanceFromCenter = mouseOffset.magnitude * 2f;

        if (distanceFromCenter > lookZoneX)
        {
            float edgeZoneProgress = Mathf.InverseLerp(lookZoneX, 0.5f, distanceFromCenter);
            Vector2 lookDirection = mouseOffset.normalized;

            float targetYRotation = lookDirection.x * yRotationLimit.y * edgeZoneProgress;
            float targetXRotation = -lookDirection.y * xRotationLimit.y * edgeZoneProgress;

            yRotation = Mathf.Lerp(yRotation, targetYRotation, edgeSensX * Time.deltaTime);
            xRotation = Mathf.Lerp(xRotation, targetXRotation, edgeSensY * Time.deltaTime);

            xRotation = Mathf.Clamp(xRotation, xRotationLimit.x, xRotationLimit.y);
            yRotation = Mathf.Clamp(yRotation, yRotationLimit.x, yRotationLimit.y);
        }
        else
        {
            yRotation = Mathf.Lerp(yRotation, 0f, edgeSensX * Time.deltaTime * 2f);
            xRotation = Mathf.Lerp(xRotation, 0f, edgeSensY * Time.deltaTime * 2f);
        }

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }
}