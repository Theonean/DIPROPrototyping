using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerCore : MonoBehaviour
{
    public float moveSpeed = 10f;
    public UnityEvent returnLegs;
    Camera m_Camera;
    Vector3 m_OriginalCameraPosition;
    public Slider healthSlider;

    public UnityEvent PlayerDeath;

    private void Awake()
    {
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
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

    //When colliding with an enemy, destroy enemy
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            Destroy(other.gameObject);
            healthSlider.value -= 1;
            if (healthSlider.value <= 0)
            {
                //Inform to all listeners that the player has died to allow for a manual resetting of scene instead of reloading the scene
                PlayerDeath.Invoke();
            }
        }
    }
}
