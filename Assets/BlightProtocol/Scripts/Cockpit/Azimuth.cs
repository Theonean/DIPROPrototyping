using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Azimuth : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable { get; set; } = true;

    public string lookAtText = "Use Azimuth";
    public string interactText = "[Left Click] Set Marker";
    public bool UpdateHover { get; set; } = false;
    public bool UpdateInteract { get; set; } = false;
    [SerializeField] private Transform _touchPoint;
    public Transform TouchPoint
    {
        get => _touchPoint;
        set => _touchPoint = value;
    }
    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    public string InteractText
    {
        get => interactText;
        set => interactText = value;
    }
    private bool isInFocus = false;

    [Header("Camera Controls")]
    public Transform cameraLockPos;
    public float sensX = 300;
    public float sensY = 300;

    [Header("Azimuth Parts")]
    public GameObject ring;
    public Vector2 rotationRange;
    public GameObject targetCircle;
    public Vector2 circleHeightRange;
    float yRotation;

    [Header("Raycasting")]
    public Transform rayOrigin;
    public float maxDistance = 600;
    private RaycastHit hit;
    public LayerMask hitMask;
    public LayerMask innerRinghitMask;
    public Color innerRingEnabledColor;
    public Color innerRingDisabledColor;
    public Renderer innerRing;

    void Update()
    {
        if (isInFocus)
        {
            float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY;

            yRotation += mouseX;
            yRotation = Mathf.Clamp(yRotation, rotationRange.x, rotationRange.y);
            ring.transform.localRotation = Quaternion.Euler(0, yRotation, 0);

            targetCircle.transform.localPosition += new Vector3(0, mouseY, 0);
            targetCircle.transform.localPosition = new Vector3(targetCircle.transform.localPosition.x, Mathf.Clamp(targetCircle.transform.localPosition.y, circleHeightRange.x, circleHeightRange.y), targetCircle.transform.localPosition.z);

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 rayDirection = (rayOrigin.position - cameraLockPos.position).normalized;
                if (Physics.Raycast(new Ray(rayOrigin.position, rayDirection), out hit, maxDistance, hitMask))
                {
                    Map.Instance.SetCustomMarker(hit.point);
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                FPVPlayerCam.Instance.UnlockPosition();
                Cursor.lockState = CursorLockMode.Locked;
                isInFocus = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (isInFocus)
        {
            UpdateInnerRing();
        }
    }

    private void UpdateInnerRing()
    {
        Vector3 rayDirection = (rayOrigin.position - cameraLockPos.position).normalized;
        if (Physics.Raycast(new Ray(rayOrigin.position, rayDirection), out hit, maxDistance, innerRinghitMask))
        {
            // Check if hit object should block the azimuth
            //Logger.Log(hit.collider.name, LogLevel.INFO, LogType.HARVESTER);
            if ((hitMask & (1 << hit.collider.gameObject.layer)) != 0)
            {
                innerRing.material.SetColor("_Color", innerRingEnabledColor);
                innerRing.material.SetColor("_EmissionColor", innerRingEnabledColor);
            }
            else
            {
                innerRing.material.SetColor("_Color", innerRingDisabledColor);
                innerRing.material.SetColor("_EmissionColor", innerRingDisabledColor);
            }

        }
        else
        {
            innerRing.material.SetColor("_Color", innerRingDisabledColor);
            innerRing.material.SetColor("_EmissionColor", innerRingDisabledColor);
        }
    }



    public void OnHover()
    {
        this.DefaultOnHover();
    }

    public void OnStartInteract()
    {
        if (FPVPlayerCam.Instance.isLocked)
        {
            FPVPlayerCam.Instance.UnlockPosition();
            Cursor.lockState = CursorLockMode.Locked;
            isInFocus = false;
        }
        else
        {
            FPVPlayerCam.Instance.LockToPosition(cameraLockPos, false, true);
            Cursor.lockState = CursorLockMode.Locked;
            isInFocus = true;
            this.DefaultOnInteract();
        }
    }
    public void OnUpdateInteract() {}
    public void OnEndInteract() {}
}
