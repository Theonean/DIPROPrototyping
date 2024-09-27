using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    PlayerCore m_Player;
    [SerializeField]
    float m_MoveSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        m_Player = GameObject.Find("Core").GetComponent<PlayerCore>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(m_Player.transform);
        transform.position = Vector3.MoveTowards(transform.position, m_Player.transform.position, m_MoveSpeed * Time.deltaTime);
    }
}
