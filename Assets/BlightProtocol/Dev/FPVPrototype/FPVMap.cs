using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPVMap : MonoBehaviour, IFPVInteractable
{
    public Camera mapCamera;
    private Ray ray;
    private RaycastHit hit;
    public LayerMask hitMask;
    public bool IsCurrentlyInteractable {get; set;} = true;

    public string lookAtText = "E"; // Backing field for Inspector

    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    public void OnHover() {
        this.DefaultOnHover();
    }

    public void OnInteract()
    {
        // idk
    }

    public void SetTarget(RaycastHit _hit)
    {
        ray = mapCamera.ViewportPointToRay(new Vector3(_hit.textureCoord.x, _hit.textureCoord.y));
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, hitMask))
        {
            Vector3 targetPos = new Vector3(hit.point.x, 0, hit.point.z);
            ControlZoneManager.Instance.SetNextPathPosition(targetPos);
        }
    }
}
