using UnityEngine;

public class FPVMap : MonoBehaviour, IFPVInteractable
{
    public Camera mapCamera;
    private Ray ray;
    private RaycastHit hit;
    public LayerMask hitMask;
    public bool IsCurrentlyInteractable { get; set; } = true;

    public string lookAtText = "E"; // Backing field for Inspector
    public Transform cameraLockPos;
    public RectTransform uiTarget;
    public Vector2 mouseOffset;
    private bool isInFocus = false;

    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    void Update()
    {
        if (isInFocus)
        {
            Vector2 mousePos = Input.mousePosition;

            float confinedMouseX = Mathf.Clamp(mousePos.x-mouseOffset.x*0.5f, 0, mapCamera.pixelWidth);
            float confinedMouseY = Mathf.Clamp(mousePos.y-mouseOffset.y*0.5f, 0, mapCamera.pixelHeight);

            float centeredMouseX = confinedMouseX - (mapCamera.pixelWidth / 2);
            float centeredMouseY = confinedMouseY - (mapCamera.pixelHeight / 2);

            // Set the UI target position based on the centered and confined mouse position
            uiTarget.anchoredPosition = new Vector3(centeredMouseX, centeredMouseY);

            if (Input.GetMouseButtonDown(0))
            {
                SetTarget(new Vector2(confinedMouseX, confinedMouseY));
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                FPVPlayerCam.Instance.UnlockPosition();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                isInFocus = false;

                uiTarget.anchoredPosition = new Vector3(0, 0);
            }
        }
    }

    public void OnHover()
    {
        this.DefaultOnHover();
    }

    public void OnInteract()
    {
        if (FPVPlayerCam.Instance.isLocked)
        {
            FPVPlayerCam.Instance.UnlockPosition();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isInFocus = false;
        }
        else
        {
            FPVPlayerCam.Instance.LockToPosition(cameraLockPos);
            Cursor.lockState = CursorLockMode.Confined;
            isInFocus = true;

            mouseOffset = new Vector2(Mathf.Clamp(Input.mousePosition.x, 0, mapCamera.pixelWidth), Mathf.Clamp(Input.mousePosition.y, 0, mapCamera.pixelWidth));
        }

    }

    public void SetTarget(Vector2 pos)
    {
        ray = mapCamera.ScreenPointToRay(pos);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, hitMask))
        {
            Vector3 targetPos = new Vector3(hit.point.x, 0, hit.point.z);
            ControlZoneManager.Instance.SetNextPathPosition(targetPos);
        }
    }

}
