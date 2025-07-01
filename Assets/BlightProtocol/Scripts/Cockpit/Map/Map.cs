using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Linq;
using TMPro;

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
    private Ray screenRay;
    private Ray targetRay;
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

    bool isHovering = false;

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
        base.Start();
        mapCamera = mapCameraCam.GetComponent<MapCamera>();
        UpdateHover = true;
    }
    public override void OnStartHover()
    {
        base.OnStartHover();
        isHovering = true;
    }

    public override void OnUpdateHover(Vector2 mousePos)
    {
        screenRay = FPVInputManager.Instance.fpvPlayerCam.ScreenPointToRay(mousePos);
        targetRay = mapCameraCam.ScreenPointToRay(screenPoint);
        if (Physics.Raycast(screenRay, out RaycastHit hit, Mathf.Infinity))
        {
            screenPoint = new Vector3(hit.textureCoord.x * mapCameraCam.pixelWidth, hit.textureCoord.y * mapCameraCam.pixelWidth, 0);
            uiTarget.GetComponent<RectTransform>().anchoredPosition = screenPoint;
            TouchPoint.transform.position = hit.point;
        }
    }

    public override void OnEndHover()
    {
        base.OnEndHover();
        isHovering = false;
    }

    public override void OnStartInteract()
    {
        if (screenPoint != null)
        {
            SetTarget();
        }
    }

    public void SetTarget()
    {
        if (TutorialManager.Instance.IsTutorialOngoing() && TutorialManager.Instance.progressState is not TutorialProgress.SETFIRSTMAPPOINT and not TutorialProgress.SETDESTINATIONTORESOURCEPOINT and not TutorialProgress.DRIVETOCHECKPOINT)
        {
            TutorialManager.Instance.FlashBackGround();
            return;
        }

        if (Physics.Raycast(targetRay, out hit, Mathf.Infinity, hitMask))
        {
            if (hit.collider.CompareTag("Checkpoint"))
            {
                Transform checkpointRoot = hit.collider.transform.parent; 
                Transform dockingPosition = checkpointRoot.GetComponentsInChildren<Transform>()
                    .FirstOrDefault(x => x.name == "DockingPosition");
                Harvester.Instance.mover.SetDestination(dockingPosition.position);
                Logger.Log("Harvester moving to checkpoint", LogLevel.INFO, LogType.HARVESTER);
            }
            if (hit.collider.CompareTag("ResourcePoint"))
            {
                if (TutorialManager.Instance.IsTutorialOngoing())
                {
                    Vector3 targetPos = hit.collider.transform.position;
                    targetPos.y = 0;

                    Harvester.Instance.mover.SetDestination(targetPos);
                    TutorialManager.Instance.CompleteSETDESTINATIONTORESOURCEPOINT();
                }
                else
                {
                    Vector3 targetPos = new Vector3(hit.point.x, 0, hit.point.z);
                    Harvester.Instance.mover.SetDestination(targetPos);
                }
            }
            if (hit.collider.CompareTag("TutorialTarget"))
            {
                Vector3 targetPos = hit.collider.transform.position;
                targetPos.y = 0;

                Harvester.Instance.mover.SetDestination(targetPos);
                TutorialManager.Instance.CompleteSETFIRSTMAPPOINT();
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