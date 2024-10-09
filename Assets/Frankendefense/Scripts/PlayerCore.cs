using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerCore : MonoBehaviour
{
    public bool DashDoesDamage; //true = dash damages enemies, false = dash knocks enemies back
    bool m_IsDashing = false;
    float m_DashKnockback = 50f;
    float m_DashTime = 0.5f;
    float m_DashSpeed = 15f;
    public float moveSpeed;
    public UnityEvent returnLegs;
    public Material transparentMaterial;
    Camera m_Camera;
    Vector3 m_OriginalCameraPosition;
    Vector3 m_MoveDirection;

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
        //If rightclick is pressed, return all legs to the player
        if (Input.GetMouseButtonDown(1))
        {
            returnLegs.Invoke();
        }

        //If spacebar is pressed, make the player Dash
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Dash();
        }

        // Update m_MoveDirection based on input
        Vector3 tempDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) m_MoveDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) m_MoveDirection += Vector3.back;
        if (Input.GetKey(KeyCode.A)) m_MoveDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D)) m_MoveDirection += Vector3.right;

        //Lerp movedirection to 0
        m_MoveDirection = Vector3.Lerp(m_MoveDirection, Vector3.zero, 20f * Time.deltaTime); 

        //Clamp the move direction to maxspeed
        if (m_MoveDirection.magnitude > moveSpeed)
            m_MoveDirection = m_MoveDirection.normalized * moveSpeed;


    }

    void FixedUpdate()
    {
        if (!m_IsDashing) transform.position += m_MoveDirection * moveSpeed * Time.fixedDeltaTime;
    }

    //When the player collides with an object, check if the object is an enemy and apply knockback or damage when dashing
    void OnTriggerEnter(Collider other)
    {
        if (m_IsDashing && other.CompareTag("Enemy"))
        {
            if (DashDoesDamage)
            {
                // Damage the enemy
                Debug.Log("Dealt damage to enemy");
                Destroy(other.gameObject);
            }
            else
            {
                // Knockback the enemy
                Debug.Log("Knocked back enemy");
                StartCoroutine(other.GetComponentInParent<FollowPlayer>().ApplyKnockback(m_MoveDirection, m_DashKnockback));
            }

        }
    }

    private void Dash()
    {
        if (m_IsDashing) return; // Prevent multiple dashes at the same time

        m_IsDashing = true;
        m_Renderer.material.color = Color.red; // Change color to indicate dash

        StartCoroutine(DashMovement());
        StartCoroutine(DashEffect());
    }

    private IEnumerator DashMovement()
    {
        float elapsedTime = 0f;

        while (elapsedTime < m_DashTime)
        {
            // Move the player in the current move direction at the specified dash speed
            transform.position += m_MoveDirection * m_DashSpeed * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        m_IsDashing = false;
        m_Renderer.material.color = Color.white; // Reset color after dash
    }

    IEnumerator DashEffect()
    {
        int shadowCount = 8; // Number of shadows to leave behind
        float shadowInterval = m_DashTime / (shadowCount + 1); // Time between each shadow
        float fadeDuration = 0.5f; // Time for the shadow to fade out

        for (int i = 0; i < shadowCount; i++)
        {
            CreateShadowCopy(fadeDuration);
            yield return new WaitForSeconds(shadowInterval);
        }
    }

    void CreateShadowCopy(float fadeDuration)
    {
        GameObject shadow = new GameObject("Shadow");
        shadow.transform.position = transform.position;
        shadow.transform.rotation = transform.rotation;

        MeshFilter meshFilter = shadow.AddComponent<MeshFilter>();
        meshFilter.mesh = GetComponent<MeshFilter>().mesh;

        MeshRenderer shadowRenderer = shadow.AddComponent<MeshRenderer>();
        shadowRenderer.material = transparentMaterial;
        shadowRenderer.material.color = new Color(0, 0, 0, 0.5f); // Set shadow to semi-transparent

        StartCoroutine(FadeOutShadow(shadowRenderer, fadeDuration));
        Destroy(shadow, fadeDuration + 0.1f); // Destroy shadow slightly after fade out completes
    }

    IEnumerator FadeOutShadow(MeshRenderer shadowRenderer, float duration)
    {
        float elapsedTime = 0f;
        Color startColor = shadowRenderer.material.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsedTime < duration)
        {
            shadowRenderer.material.color = Color.Lerp(startColor, endColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        shadowRenderer.material.color = endColor; // Ensure the color is fully transparent at the end
    }
}
