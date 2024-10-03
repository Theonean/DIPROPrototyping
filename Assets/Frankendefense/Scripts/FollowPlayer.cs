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
        transform.position = Vector3.MoveTowards(transform.position, m_ControlZone.transform.position, m_MoveSpeed * Time.deltaTime);
    }
}
