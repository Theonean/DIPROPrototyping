using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCore : MonoBehaviour
{
    public float moveSpeed = 10f;
    public UnityEvent returnLegs;
    Camera m_Camera;
    Vector3 m_OriginalCameraPosition;

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
            StartCoroutine(ShakeScreen(0.5f, 0.005f));
            Debug.Log("Player should Take Damage");
        }
    }

    private IEnumerator ShakeScreen(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;

            m_Camera.transform.localPosition = new Vector3(x, m_OriginalCameraPosition.y, z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        m_Camera.transform.localPosition = m_OriginalCameraPosition;
    }
}
