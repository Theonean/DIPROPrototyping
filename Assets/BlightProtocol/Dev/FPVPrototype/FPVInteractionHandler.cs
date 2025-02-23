using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPVInteractionHandler : MonoBehaviour
{
    private Camera fpvCamera;
    public Camera mapCamera;
    private Ray ray;
    private RaycastHit hit;
    private RaycastHit mapHit;
    public LayerMask hitMask;

    void Start()
    {
        fpvCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fpvCamera.enabled)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Interact();
            }
        }
    }

    void Interact()
    {
        ray = fpvCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, hitMask))
        {
            switch(hit.collider.tag) {
                case "Map":
                    SetTarget(hit);
                break;

                default:
                    hit.collider.GetComponent<FPVInteractable>()?.Interact();
                break;
            }
        }
    }

    void SetTarget(RaycastHit _hit)
    {
        Debug.Log(hit.collider.name + " " + _hit.textureCoord.x);
        ray = mapCamera.ViewportPointToRay(new Vector3(_hit.textureCoord.x, _hit.textureCoord.y));
        if (Physics.Raycast(ray, out mapHit))
        {
            ControlZoneManager.Instance.SetNextPathPosition(mapHit.point);
        }
    }
}
