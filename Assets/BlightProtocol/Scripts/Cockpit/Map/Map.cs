using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class Map : MonoBehaviour, IFPVInteractable
{
    public static Map Instance { get; private set; }
    public string lookAtText = "E";
    public string interactText = "[Left Click] Set Target Position\n[Right Click Drag] Move Map";
    public bool UpdateHover { get; set; } = true;
    public bool UpdateInteract { get; set; } = true;

    [SerializeField] private Transform _touchPoint;
    public Transform TouchPoint
    {
        get => _touchPoint;
        set => _touchPoint = value;
    }

    public string LookAtText { get => lookAtText; set => lookAtText = value; }
    public string InteractText { get => interactText; set => interactText = value; }

    public Transform cameraLockPos;
    public bool IsCurrentlyInteractable { get; set; } = true;
    private bool isInFocus = false;

    [Header("Display")]
    public Camera mapCameraCam;
    private MapCamera mapCamera;
    public float dragSpeed = 0.1f;
    public Vector2 cameraBounds = new Vector2(50f, 50f);
    private Vector3 dragOrigin;
    private bool isDragging = false;

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

    void Start()
    {
        mapCamera = mapCameraCam.GetComponent<MapCamera>();
    }

    public void OnHover()
    {
        this.DefaultOnHover();
        Ray ray = FPVPlayerCam.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            screenPoint = new Vector3(hit.textureCoord.x * mapCameraCam.pixelWidth, hit.textureCoord.y * mapCameraCam.pixelHeight, 0);
            uiTarget.GetComponent<RectTransform>().anchoredPosition = screenPoint;
            TouchPoint.transform.position = hit.point;
        }
    }

    public void OnStartInteract()
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

    public void OnUpdateInteract()
    {
    }

    public void OnEndInteract()
    { }

    public void SetTarget(Vector3 screenPoint)
    {
        ray = mapCameraCam.ScreenPointToRay(screenPoint);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, hitMask))
        {
            if (hit.collider.CompareTag("ResourcePoint"))
            {
                Vector3 targetPos = new Vector3(hit.point.x, 0, hit.point.z);
                Harvester.Instance.mover.SetDestination(targetPos);
            }
            else if (NavMesh.SamplePosition(hit.point, out NavMeshHit navMeshHit, 10f, NavMesh.AllAreas))
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
        GameObject newMarker = Instantiate(customMarker, new Vector3(position.x, 0, position.z), Quaternion.Euler(90, 0, 0));
        customMarkers.Add(newMarker);
    }

    public void SetEnergySignature(Vector3 position, EnergySignature signature)
    {
        if (!discoveredSignatures.TryGetValue(signature, out GameObject marker))
        {
            GameObject newSignatureMarker = Instantiate(energySignaturePing, new Vector3(position.x, 0, position.z), Quaternion.Euler(90, 0, 0));
            newSignatureMarker.GetComponent<EnergySignatureDisplayer>().DisplaySignature(signature);

            discoveredSignatures[signature] = newSignatureMarker;
        }
    }
}