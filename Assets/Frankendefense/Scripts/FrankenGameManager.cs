using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrankenGameManager : MonoBehaviour
{
    public float zoneRadius = 10f;
    public float zoneWaitTime = 5f;
    float zoneTimer = 0f;
    public GameObject MapBoundaries;
    Vector3[] m_BoundaryPositions;

    private void Start()
    {
        //Load the two corners of the map boundaries
        m_BoundaryPositions = new Vector3[2];
        m_BoundaryPositions[0] = MapBoundaries.transform.GetChild(0).position;
        m_BoundaryPositions[1] = MapBoundaries.transform.GetChild(1).position;
    }

    private void Update()
    {
        zoneTimer += Time.deltaTime;
        if (zoneTimer >= zoneWaitTime)
        {
            zoneTimer = 0f;
            CreateZone();
        }
    }

    void CreateZone()
    {
        Vector3 zonePosition = new Vector3(Random.Range(m_BoundaryPositions[0].x, m_BoundaryPositions[1].x), 0f, Random.Range(m_BoundaryPositions[0].z, m_BoundaryPositions[1].z));
        GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.transform.position = zonePosition;
        zone.transform.localScale = new Vector3(zoneRadius * 2, 0.1f, zoneRadius * 2);
        //Set colour to blue and half transparent
        zone.GetComponent<MeshRenderer>().material.color = Color.blue;
        Color tempColor = zone.GetComponent<MeshRenderer>().material.color;
        tempColor.a = 0.5f;
        zone.GetComponent<MeshRenderer>().material.color = tempColor;

        Destroy(zone, zoneWaitTime);
    }
}
