using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ValueMaterial
{
    public Color color;
}

public class BlobMathHandler : MonoBehaviour
{
    [Header("Value")]
    public int value;
    public int targetValue;
    public GameManager gameManager;

    [Header("Value Display")]
    public TextMeshPro displayText;
    public float scaleFactor = 1.0f;

    public List<ValueMaterial> valueMaterials = new();
    private Renderer rend;
    
    BoxCollider boxCollider;
    Rigidbody rb;
    public bool isHeld = false;
    private bool hasSplit = false;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        Initiate();
    }
    public void Initiate()
    {
        rb = GetComponent<Rigidbody>();
        UpdateValueDisplay();
    }

    private void UpdateValueDisplay()
    {
        displayText.text = value.ToString();
        float scale = 1 + (value - 1) * scaleFactor;
        transform.localScale = new Vector3(scale, scale, scale);
        Color color = valueMaterials[value].color;
        if (color != null)
        {
            rend.material.SetColor("_BaseColor", color);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Blob") && 
            collision.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rbOther)
            && !hasSplit && !isHeld) {
            float velocitySelf = rb.velocity.magnitude;
            float velocityOther = rbOther.velocity.magnitude;
            if (collision.gameObject.TryGetComponent<BlobMathHandler>(out BlobMathHandler otherBlobMathHandler) && !otherBlobMathHandler.isHeld)
            {
                if (velocitySelf > velocityOther)
                {
                    Combine(otherBlobMathHandler);
                }
                else if (velocitySelf == velocityOther)
                {
                    if (GetInstanceID() > rbOther.GetInstanceID())
                    {
                        Combine(otherBlobMathHandler);
                    }
                }
            } 
        }
    }
    private void Combine(BlobMathHandler otherBlobMathHandler)
    {
        if (value + otherBlobMathHandler.value <= targetValue)
        {
            value += otherBlobMathHandler.value;
            otherBlobMathHandler.Destroy();
            UpdateValueDisplay();


            if (value == targetValue)
            {
                gameManager.AddProgress();
                Destroy();
            }
        }
    }

    public void Split()
    {
        if (value > 1)
        {
            int newValue = value / 2;
            int otherValue = value - newValue;

            value = newValue;
            UpdateValueDisplay();
            hasSplit = true;
            Invoke(nameof(EndHasSplit), 1f);

            GameObject newBlob = Instantiate(gameObject, transform.position, Quaternion.identity);
            if (newBlob.TryGetComponent<BlobMathHandler>(out BlobMathHandler otherBlobMathHandler))
            {
                otherBlobMathHandler.value = otherValue;
                otherBlobMathHandler.targetValue = targetValue;
                otherBlobMathHandler.UpdateValueDisplay();
                otherBlobMathHandler.hasSplit = true;
                otherBlobMathHandler.Invoke(nameof(EndHasSplit), 1f);
            }
        }
    }

    public void EndHasSplit()
    {
        hasSplit = false;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
