using System.Collections.Generic;
using UnityEngine;

public class BlobFamilyHandler : MonoBehaviour
{
    [Header("Value")]
    public int value;
    public int targetValue;
    public GameManager gameManager;
    private bool familyComplete = false;

    [Header("Blob Display")]
    public GameObject blobPrefab;
    public float yOffset;
    public List<GameObject> childBlobs = new List<GameObject>();

    BoxCollider boxCollider;
    Rigidbody rb;
    public bool isHeld = false;
    private bool hasSplit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        UpdateFamilyDisplay();
    }

    public void UpdateFamilyDisplay()
    {
        IndividualBlobHandler[] children = GetComponentsInChildren<IndividualBlobHandler>();
        int childDifference = value - children.Length;
        if (childDifference > 0)
        {
            AddChildren(children.Length, childDifference);
        }
        else if (childDifference < 0)
        {
            RemoveChildren(-childDifference);
        }
    }

    private void AddChildren(int childCount, int difference)
    {
        float childYOffset = childCount * yOffset;
        for (int i = 0; i < difference; i++)
        {
            GameObject newChild = Instantiate(blobPrefab, new Vector3(transform.position.x, transform.position.y + childYOffset, transform.position.z), Quaternion.identity, transform);
            childYOffset += yOffset;
            childBlobs.Add(newChild);
        }
    }
    private void RemoveChildren(int difference)
    {
        for (int i = 0; i < difference; i++)
        {
            if (childBlobs.Count > 0)
            {
                GameObject blobToRemove = childBlobs[childBlobs.Count - 1];
                childBlobs.Remove(blobToRemove);
                Destroy(blobToRemove);
            }
        }
    }

    private void CompleteFamily()
    {
        familyComplete = true;
        foreach (GameObject childBlob in childBlobs)
        {
            IndividualBlobHandler indBlobHandler = childBlob.GetComponent<IndividualBlobHandler>();
            indBlobHandler.SetState(IndividualBlobHandler.Emotion.happy);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Blob") && collision.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rbOther) && !hasSplit && !isHeld)
        {
            float velocitySelf = rb.velocity.magnitude;
            float velocityOther = rbOther.velocity.magnitude;
            if (collision.gameObject.TryGetComponent<BlobFamilyHandler>(out BlobFamilyHandler otherBlobFamilyHandler) && !otherBlobFamilyHandler.isHeld && !familyComplete && !otherBlobFamilyHandler.familyComplete)
            {
                if (velocitySelf > velocityOther)
                {
                    Combine(otherBlobFamilyHandler);
                }
                else if (velocitySelf == velocityOther && GetInstanceID() > rbOther.GetInstanceID())
                {
                    Combine(otherBlobFamilyHandler);
                }
            }
        }
    }

    private void Combine(BlobFamilyHandler otherBlobFamilyHandler)
    {
        if (value + otherBlobFamilyHandler.value <= targetValue)
        {
            value += otherBlobFamilyHandler.value;
            otherBlobFamilyHandler.Destroy();
            UpdateFamilyDisplay();

            if (value == targetValue)
            {
                CompleteFamily();
                gameManager.AddProgress();
            }
        }
    }

    public void Split()
    {
        if (value > 1)
        {
            int newValue = value / 2;
            int otherValue = value - newValue;

            GameObject newBlob = Instantiate(gameObject, transform.position + new Vector3(0.5f, 0, 0), Quaternion.identity);
            if (newBlob.TryGetComponent<BlobFamilyHandler>(out BlobFamilyHandler otherBlobFamilyHandler))
            {
                otherBlobFamilyHandler.value = otherValue;
                otherBlobFamilyHandler.hasSplit = true;
                otherBlobFamilyHandler.Invoke(nameof(EndHasSplit), 1f);
                otherBlobFamilyHandler.UpdateFamilyDisplay();

                value = newValue;
                UpdateFamilyDisplay();

                hasSplit = true;
                Invoke(nameof(EndHasSplit), 1f);
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
