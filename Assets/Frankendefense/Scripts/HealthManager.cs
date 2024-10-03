using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [Tooltip("Hits needed until this object signals death")]
    public int health;
    public Slider healthSlider;
    public UnityEvent died;
    Color m_OriginalColor;

    void Start()
    {
        healthSlider.maxValue = health;
        healthSlider.value = health;
        m_OriginalColor = GetComponent<Renderer>().material.color;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            health--;
            healthSlider.value = health;
            Destroy(other.gameObject);
            
            StartCoroutine(TakeDamageEffect());
            if (health <= 0)
            {
                died.Invoke();
            }
        }
    }

    //Turn material red and quickly fade back to normal
    IEnumerator TakeDamageEffect()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = Color.red;
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            renderer.material.color = Color.Lerp(Color.red, m_OriginalColor, t);
            yield return null;
        }
        renderer.material.color = m_OriginalColor;
    }
}
