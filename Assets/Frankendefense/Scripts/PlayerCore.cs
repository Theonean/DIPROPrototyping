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
    public Slider healthSlider;

    MeshRenderer m_Renderer;

    public UnityEvent PlayerDeath;
    Coroutine m_damageEffectCoroutine;

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

    //When colliding with an enemy, destroy enemy
    private void OnTriggerEnter(Collider other)
    {
        //If the object is an enemy, destroy the enemy and reduce health
        if (other.gameObject.tag == "Enemy")
        {
            //Destroy enemy and start damage effect
            Destroy(other.gameObject);

            if (m_damageEffectCoroutine != null)
                StopCoroutine(m_damageEffectCoroutine);

            m_damageEffectCoroutine = StartCoroutine(DamageEffect());

            healthSlider.value -= 1;
            //If health is less than or equal to 0, player has died
            if (healthSlider.value <= 0)
            {
                //Inform to all listeners that the player has died to allow for a manual resetting of scene instead of reloading the scene
                PlayerDeath.Invoke();

                //Create a huge physics overlap sphere and destroy all Enemy Objects within the sphere
                Collider[] colliders = Physics.OverlapSphere(transform.position, 100f);
                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject.tag == "Enemy")
                    {
                        Destroy(collider.gameObject.transform.parent.gameObject);
                    }
                }

                //Return health back to full
                healthSlider.value = 3;
            }
        }
    }

    //When player takes damage, turn the body red and fade back to white over time
    IEnumerator DamageEffect()
    {
        m_Renderer.material.color = Color.red;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            m_Renderer.material.color = Color.Lerp(Color.red, Color.white, t);
            yield return null;
        }
    }
}
