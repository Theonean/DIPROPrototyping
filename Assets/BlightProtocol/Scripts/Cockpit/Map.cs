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
            Vector2 screenSpaceMousePos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
            Vector3 screenPoint = new Vector3(screenSpaceMousePos.x * mapCamera.pixelWidth, screenSpaceMousePos.y * mapCamera.pixelHeight, 100f);
            uiTarget.transform.position = mapCamera.ScreenToWorldPoint(screenPoint);

            if (Input.GetMouseButtonDown(0))
            {
                SetTarget(screenPoint);
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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.lockState = CursorLockMode.Confined;
            isInFocus = true;

            mouseOffset = new Vector2(Input.mousePosition.x - mapCamera.pixelWidth, Input.mousePosition.y-mapCamera.pixelHeight);
        }

    }

    public void SetTarget(Vector3 screenPoint)
    {
        ray = mapCamera.ScreenPointToRay(screenPoint);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, hitMask))
        {
            Vector3 targetPos = new Vector3(hit.point.x, 0, hit.point.z);
            Harvester.Instance.mover.SetDestination(targetPos);
        }
    }

}
