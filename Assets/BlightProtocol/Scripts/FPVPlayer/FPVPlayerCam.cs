using System.Collections;
using TMPro;
using UnityEngine;

public class FPVPlayerCam : MonoBehaviour
{
    public static FPVPlayerCam Instance { get; private set; }
    private KeyCode lookKey = KeyCode.Mouse1;
    public bool lookIsToggle = false;
    public bool isLooking = false;
    public float sensX;
    public float sensY;

    public Transform orientation;

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

    private void Update()
    {
        if (isLocked)
        {
            return;
        }
        if (Input.GetKeyDown(lookKey))
        {
            if (lookIsToggle && isLooking)
            {
                UnlockCursor();
                isLooking = false;
            }
            else
            {
                LockCursor();
                isLooking = true;
            }
                
        }
        else if (Input.GetKeyUp(lookKey))
        {
            if (!lookIsToggle)
            {
                UnlockCursor();
                isLooking = false;
            }
        }

        if (isLooking)
        {
            float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.localRotation = Quaternion.Euler(0, yRotation, 0);
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
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
        Cursor.lockState = CursorLockMode.Confined;
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
