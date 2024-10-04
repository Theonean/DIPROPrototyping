using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    MISSNOMER CLASS BUT IM NOT RENAMING IT AS THIS IS A PROTOTYPE

    Enemies now target the control zone and die when touching it.
*/

public class FollowPlayer : MonoBehaviour
{

    GameObject m_ControlZone;
    [SerializeField]
    float m_MoveSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        m_ControlZone = GameObject.Find("ControlZone");
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(m_ControlZone.transform);

        //If enemy is seen by camera.main or close to zone move normal speed otherwise move 5x speed
        if (IsVisibleToCamera() || Vector3.Distance(transform.position, m_ControlZone.transform.position) < 35f)
        {
            transform.position += transform.forward * m_MoveSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += transform.forward * (m_MoveSpeed * 5f) * Time.deltaTime;
        }


        //transform.position = Vector3.MoveTowards(transform.position, m_ControlZone.transform.position, m_MoveSpeed * Time.deltaTime);
    }
    bool IsVisibleToCamera()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return GeometryUtility.TestPlanesAABB(planes, GetComponentInChildren<Collider>().bounds);
    }
}
