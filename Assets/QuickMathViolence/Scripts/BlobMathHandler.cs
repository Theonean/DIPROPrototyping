using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BlobMathHandler : MonoBehaviour
{
    [Header("Value")]
    public int value;

    [Header("Value Display")]
    public TextMeshPro displayText;
    public float scaleFactor = 1.0f;
    
    BoxCollider boxCollider;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        UpdateText();
    }

    private void UpdateText()
    {
        displayText.text = value.ToString();
        float scale = 1 + (value - 1) * scaleFactor;
        transform.localScale = new Vector3(scale, scale, scale);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Blob") && 
            collision.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rbOther)) {
            float velocitySelf = rb.velocity.magnitude;
            float velocityOther = rbOther.velocity.magnitude;
            if (collision.gameObject.TryGetComponent<BlobMathHandler>(out BlobMathHandler otherBlobMathHandler))
            {
                if (velocitySelf > velocityOther)
                {
                    Combine(otherBlobMathHandler.value);
                    otherBlobMathHandler.Destroy();
                }
                else if (velocitySelf == velocityOther)
                {
                    if (GetInstanceID() > rbOther.GetInstanceID())
                    {
                        Combine(otherBlobMathHandler.value);
                        otherBlobMathHandler.Destroy();
                    }
                }
            } 
        }
    }
    private void Combine(int otherValue)
    {
        value += otherValue;
        UpdateText();
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
