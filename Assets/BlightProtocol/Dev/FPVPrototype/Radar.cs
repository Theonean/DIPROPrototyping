using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    public float rotationSpeed = 180f;
    public float radarDistance = 150f;
    public LayerMask layerMask;
    public GameObject enemyPing;
    public GameObject terrainPing;

    private List<Collider> colliderList = new List<Collider>();

    // Update is called once per frame
    void Update()
    {
        float previousRotation = (transform.eulerAngles.y % 360) - 180;
        transform.eulerAngles -= new Vector3(0, rotationSpeed * Time.deltaTime, 0);
        float currentRotation = (transform.eulerAngles.y % 360) - 180;

        if (previousRotation < 0 && currentRotation > 0)
        {
            // half rotation
            colliderList.Clear();
        }

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, radarDistance, layerMask))
        {
            if (hit.collider != null)
            {
                /*if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
                {
                    Instantiate(terrainPing, hit.point, Quaternion.Euler(90, 0, 0));
                }
                else */if (!colliderList.Contains(hit.collider))
                {
                    colliderList.Add(hit.collider);
                    Instantiate(enemyPing, hit.point, Quaternion.Euler(90, 0, 0));
                }
            }
        }
        ;
    }
}
