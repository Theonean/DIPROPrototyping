using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvesterTrailTracker : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    private Transform harvester;
    [SerializeField] private float recordInterval = 0.1f; // How often to record positions
    [SerializeField] private int maxPositions = 1000; // Max points before trimming
    private bool isRecording = true;

    private List<Vector3> _pathPositions = new List<Vector3>();

    void Start()
    {
        harvester = Harvester.Instance.transform;
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 0;
        StartCoroutine(RecordPath());
        Harvester.Instance.changedState.AddListener(ToggleRecording);
    }

    private void ToggleRecording(ZoneState zoneState)
    {
        switch (zoneState)
        {
            case ZoneState.MOVING:
                isRecording = true;
                break;
            default:
                isRecording = false;
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
