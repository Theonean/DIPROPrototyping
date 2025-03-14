using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPVInteractionHandler : MonoBehaviour
{
    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;
    private bool interactKeyPressed = false;

    [Header("Cameras")]
    private Camera fpvCamera;

    [Header("Raycasting")]
    private Ray ray;
    private RaycastHit hit;
    public float raycastRange;
    public LayerMask hitMask;

    private IFPVInteractable lastHoveredObject = null;

    void Start()
    {
        fpvCamera = GetComponent<Camera>();
    }

    void Update()
    {
        interactKeyPressed = false;
        if (Input.GetKeyDown(interactKey))
        {
            interactKeyPressed = true;
        }
        
    }

    void FixedUpdate()
    {
        ray = fpvCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, raycastRange, hitMask))
        {
            if (hit.collider.TryGetComponent<IFPVInteractable>(out IFPVInteractable interactable))
            {
                if (interactable != lastHoveredObject)
                {
                    lastHoveredObject = interactable;
                    interactable.OnHover();
                }
            }

            if (interactKeyPressed)
            {
                Interact();
                interactKeyPressed = false;
            }
        }
        else
        {
            if (lastHoveredObject != null)
            {
                FPVUI.Instance.ClearLookAtText();
                lastHoveredObject = null;
            }
        }
    }

    void Interact()
    {
        switch (hit.collider.tag)
        {
            /*case "Map":
                hit.collider.GetComponent<FPVMap>().SetTarget(hit);
                break;*/

            default:
                if (lastHoveredObject != null)
                {
                    lastHoveredObject.OnInteract();
                }
                break;
        }
    }
}
