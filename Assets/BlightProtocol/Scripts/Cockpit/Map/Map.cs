using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class Map : ACInteractable
{
    public static Map Instance { get; private set; }

    public Transform cameraLockPos;

    [Header("Display")]
    public Camera mapCameraCam;
    private MapCamera mapCamera;
    public float dragSpeed = 0.1f;
    public Vector2 cameraBounds = new Vector2(50f, 50f);
    private Vector3 dragOrigin;

    [Header("Target Setting")]
    private Ray ray;
    private RaycastHit hit;
    public LayerMask hitMask;
    public RectTransform uiTarget;
    public Vector2 mouseOffset;
    private Vector3 screenPoint;

    [Header("Markers")]
    public GameObject energySignaturePing;
    public GameObject customMarker;
    public int maxMarkers = 2;
    private List<GameObject> customMarkers = new List<GameObject>();

    // Dictionary to track discovered energy signatures
    private Dictionary<EnergySignature, GameObject> discoveredSignatures = new Dictionary<EnergySignature, GameObject>();

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
    }

    protected override void Start()
    {
        mapCamera = mapCameraCam.GetComponent<MapCamera>();
        UpdateHover = true;
    }

    public override void OnStartHover()
    {
        base.OnStartHover();
    }

    public override void OnUpdateHover()
    {
        Ray ray = FPVPlayerCam.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            screenPoint = new Vector3(hit.textureCoord.x * mapCameraCam.pixelWidth, hit.textureCoord.y * mapCameraCam.pixelHeight, 0);
            uiTarget.GetComponent<RectTransform>().anchoredPosition = screenPoint;
            TouchPoint.transform.position = hit.point;
        }
    }

    public override void OnStartInteract()
    {
        // Left click - set target
        if (Input.GetMouseButton(0))
        {
            if (screenPoint != null)
            {
                SetTarget(screenPoint);
            }
        }
    }

    public void SetTarget(Vector3 screenPoint)
    {
        ray = mapCameraCam.ScreenPointToRay(screenPoint);
        Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.green, 5f);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, hitMask))
        {
            Debug.Log(hit.collider.name);
            if (hit.collider.CompareTag("ResourcePoint"))
            {
                Vector3 targetPos = new Vector3(hit.point.x, 0, hit.point.z);
                Harvester.Instance.mover.SetDestination(targetPos);
            }
            else if (NavMesh.SamplePosition(hit.point, out NavMeshHit navMeshHit, 50f, NavMesh.AllAreas))
            {
                Vector3 targetPos = navMeshHit.position;
                Harvester.Instance.mover.SetDestination(targetPos);
            }
        }
    }

    public void SetCustomMarker(Vector3 position)
    {
        if (customMarkers.Count >= maxMarkers)
        {
            Destroy(customMarkers[0]);
            customMarkers.RemoveAt(0);
        }
        GameObject newMarker = Instantiate(customMarker, new Vector3(position.x, 0, position.z), Quaternion.identity);
        customMarkers.Add(newMarker);
    }

    public void SetEnergySignature(Vector3 position, EnergySignature signature)
    {
        if (!discoveredSignatures.TryGetValue(signature, out GameObject marker))
        {
            GameObject newSignatureMarker = Instantiate(energySignaturePing, new Vector3(position.x, 0, position.z), Quaternion.identity);
            newSignatureMarker.GetComponent<EnergySignatureDisplayer>().DisplaySignature(signature);

            discoveredSignatures[signature] = newSignatureMarker;
        }
    }
}