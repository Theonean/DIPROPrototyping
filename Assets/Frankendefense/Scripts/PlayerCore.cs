using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerCore : MonoBehaviour
{
    public float moveSpeed;
    public UnityEvent returnLegs;
    Camera m_Camera;
    Vector3 m_OriginalCameraPosition;

    MeshRenderer m_Renderer;

    private void Awake()
    {
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_Renderer = GetComponent<MeshRenderer>();
        m_Renderer.material.color = Color.white;
    }


    //When spacebar is pressed in update, call the returnLegs event
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            returnLegs.Invoke();
        }
    }

    //WASD Movement
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += Vector3.forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += Vector3.back * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
        }
    }
}
