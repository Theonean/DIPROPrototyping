using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Azimuth : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable { get; set; } = true;

    public string lookAtText = "Use Azimuth";
    private bool isInFocus = false;

    [Header("Camera Controls")]
    public Transform cameraLockPos;
    public float sensX = 300;
    public float sensY = 300;

    [Header("Azimuth Parts")]
    public GameObject ring;
    public float maxRotation;
    public GameObject targetCircle;
    public Vector2 circleHeightRange;

    [Header("Raycasting")]
    public Transform rayOrigin;
    private RaycastHit hit;
    public LayerMask hitMask;
    public GameObject radarMarker;

    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    void Update()
    {
        if (isInFocus)
        {
            float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY;

            ring.transform.Rotate(Vector3.up, mouseX);
            ring.transform.rotation = Quaternion.Euler(0, Mathf.Clamp(ring.transform.rotation.eulerAngles.y, -maxRotation, maxRotation), 0);

            targetCircle.transform.localPosition += new Vector3(0, mouseY, 0);
            targetCircle.transform.localPosition = new Vector3(targetCircle.transform.localPosition.x, Mathf.Clamp(targetCircle.transform.localPosition.y, circleHeightRange.x, circleHeightRange.y), targetCircle.transform.localPosition.z);

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 rayDirection = (rayOrigin.position - cameraLockPos.position).normalized;
                if (Physics.Raycast(new Ray(rayOrigin.position, rayDirection), out hit, Mathf.Infinity, hitMask)) {
                    Logger.Log(hit.collider.name, LogLevel.INFO, LogType.HARVESTER);
                    //Debug.DrawRay(rayOrigin.position, rayDirection* 1000, Color.red, 10f);
                    Instantiate(radarMarker, new Vector3(hit.point.x, 1, hit.point.z), Quaternion.Euler(90, 0, 0));
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                FPVPlayerCam.Instance.UnlockPosition();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                isInFocus = false;
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
            FPVPlayerCam.Instance.LockToPosition(cameraLockPos, false, true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isInFocus = true;
        }

    }
}
