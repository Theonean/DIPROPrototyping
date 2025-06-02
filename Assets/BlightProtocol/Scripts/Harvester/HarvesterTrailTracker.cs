using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvesterTrailTracker : MonoBehaviour
{
    [SerializeField] private GameObject lineRendererPrefab;
    private LineRenderer lineRenderer;
    private Transform harvester;
    [SerializeField] private float recordInterval = 0.1f; // How often to record positions
    [SerializeField] private int maxPositions = 1000; // Max points before trimming
    private bool isRecording = true;

    private List<Vector3> _pathPositions = new List<Vector3>();

    void Start()
    {
        harvester = Harvester.Instance.transform;
        CreateNewPath();
        Harvester.Instance.changedState.AddListener(ToggleRecording);
    }

    public void CreateNewPath()
    {
        lineRenderer = Instantiate(lineRendererPrefab, gameObject.transform, false).GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        _pathPositions = new List<Vector3>();
        StartCoroutine(RecordPath());
    }

    private void ToggleRecording(HarvesterState zoneState)
    {
        switch (zoneState)
        {
            case HarvesterState.MOVING:
                if (!isRecording)
                {
                    StopAllCoroutines();
                    isRecording = true;
                    StartCoroutine(RecordPath());
                }
                break;
            default:
                if (isRecording)
                {
                    StopAllCoroutines();
                    isRecording = false;
                }
                break;
        }
    }

    IEnumerator RecordPath()
    {
        while (true)
        {
            // Record player position
            _pathPositions.Add(harvester.position);

            // Trim old positions if exceeding max
            if (_pathPositions.Count > maxPositions)
                _pathPositions.RemoveAt(0);

            // Update LineRenderer
            lineRenderer.positionCount = _pathPositions.Count;
            lineRenderer.SetPositions(_pathPositions.ToArray());

            yield return new WaitForSeconds(recordInterval);
        }
    }
}
