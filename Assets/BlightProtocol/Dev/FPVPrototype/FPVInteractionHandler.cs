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
            Cursor.visible = false;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Interact();
            }
        }
    }

    void Interact()
    {
        ray = fpvCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.Log(hit.collider.name);
            {
                SetTarget(hit);
            }
        }
    }

    void SetTarget(RaycastHit _hit)
    {
        ray = mapCamera.ViewportPointToRay(new Vector3(_hit.textureCoord.x, _hit.textureCoord.y));
        if (Physics.Raycast(ray, out mapHit))
        {
            Debug.Log(mapHit.point);
        }
    }
}
