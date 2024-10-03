using System.Collections.Generic;
using UnityEngine;

public class BlobFamilyHandler : MonoBehaviour
{
    [Header("Value")]
    public int value;
    public int targetValue;
    public GameManager gameManager;
    private bool familyComplete = false;
    public bool initiateOnAwake = false;

    [Header("Blob Display")]
    public GameObject blobPrefab;
    public float yOffset;
    public List<GameObject> childBlobs = new List<GameObject>();
    public List<Color> stackColors;
    public Color winColor;
    public int colorIncrement;
    private ParticleSystem particles;

    [Header("Blob Physics")]
    public float regularBlobMass;
    public float topBlobMass;

    private BlobAudioHandler audioHandler;

    BoxCollider boxCollider;
    Rigidbody rb;
    public bool isHeld = false;
    private bool hasSplit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioHandler = GetComponentInChildren<BlobAudioHandler>();
        particles = GetComponentInChildren<ParticleSystem>();
        if (initiateOnAwake)
        {
            Initiate();
        }
    }

    public void Initiate()
    {
        CountChildren();
        UpdateFamilyDisplay();
    }

    public void CountChildren()
    {
        childBlobs.Clear();
        IndividualBlobHandler[] children = GetComponentsInChildren<IndividualBlobHandler>();
        foreach (var child in children)
        {
            childBlobs.Add(child.gameObject);
        }
    }

    public void UpdateFamilyDisplay()
    {
        int childDifference = value - childBlobs.Count;
        int initialCount = childBlobs.Count;
        if (childDifference > 0)
        {
            if (childBlobs.Count == 0)
                AddChildren(initialCount, childDifference, true);
            else 
                AddChildren(initialCount, childDifference, false);
        }
        else if (childDifference < 0)
        {
            RemoveChildren(-childDifference);
        }

        UpdateBlobWeights();

        UpdateBlobColors();
    }

    private void AddChildren(int childCount, int difference, bool firstChild)
    {
        for (int i = 0; i < difference; i++)
        {
            Vector3 childPos = childBlobs[childBlobs.Count - 1].transform.position + yOffset * childBlobs[childBlobs.Count - 1].transform.up;
            if (firstChild && i == 0)
                childPos = Vector3.zero;
                

            GameObject newChild = Instantiate(blobPrefab, childPos, Quaternion.identity);
            newChild.GetComponent<IndividualBlobHandler>().parentInteractable = GetComponent<BlobInteractable>();
            Rigidbody otherRb = childBlobs.Count > 0 ? childBlobs[childBlobs.Count - 1].GetComponent<Rigidbody>() : rb;
            if (otherRb == null)
                otherRb = rb;
            newChild.GetComponent<Joint>().connectedBody = otherRb;
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
            indBlobHandler.GetComponent<Renderer>().material.SetColor("_BaseColor", winColor);
        }
    }

    private void UpdateBlobWeights()
    {
        for (int i = 0; i < childBlobs.Count; i++)
        {
            if (childBlobs[i].GetComponent<Rigidbody>() != null)
            {
                if (i == childBlobs.Count - 1)
                    childBlobs[i].GetComponent<Rigidbody>().mass = topBlobMass;
                else
                    childBlobs[i].GetComponent<Rigidbody>().mass = regularBlobMass;
            }
        }
    }
    private void UpdateBlobColors()
    {
        for (int i = 0; i < childBlobs.Count; i++)
        {
            int colorIndex = (i / colorIncrement) % stackColors.Count;
            childBlobs[i].GetComponent<Renderer>().material.SetColor("_BaseColor", stackColors[colorIndex]);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Blob") && !hasSplit && !isHeld)
        {
            if (!collision.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rbOther))
                rbOther = collision.gameObject.GetComponent<IndividualBlobHandler>().parentInteractable.GetComponent<Rigidbody>();
            float velocitySelf = rb.velocity.magnitude;
            float velocityOther = rbOther.velocity.magnitude;
            BlobFamilyHandler otherBlobFamilyHandler = collision.gameObject.GetComponent<BlobFamilyHandler>();
            if (otherBlobFamilyHandler == null)
                otherBlobFamilyHandler = collision.gameObject.GetComponent<IndividualBlobHandler>().parentInteractable.GetComponent<BlobFamilyHandler>();
            if (!otherBlobFamilyHandler.isHeld && !familyComplete && !otherBlobFamilyHandler.familyComplete)
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
        else if (collision.gameObject.layer == 10)
        {
            audioHandler.PlayAudioAction("Impact");
        }
    }

    private void Combine(BlobFamilyHandler otherBlobFamilyHandler)
    {
        if (value + otherBlobFamilyHandler.value <= targetValue)
        {
            value += otherBlobFamilyHandler.value;
            otherBlobFamilyHandler.Destroy();
            UpdateFamilyDisplay();

            audioHandler.PlayAudioAction("Merge");
            particles.Play();
        }
        else
        {
            audioHandler.PlayAudioAction("Impact");
        }
        if (value == targetValue)
        {
            CompleteFamily();
            gameManager.AddProgress();
            audioHandler.PlayAudioAction("Family");
        }
    }

    public void Split()
    {
        if (value > 1)
        {
            int newValue = value / 2;
            int otherValue = value - newValue;

            GameObject newBlob = Instantiate(gameObject, transform.position + new Vector3(0.5f, 0, 0), Quaternion.identity);
            if (newBlob.TryGetComponent<BlobFamilyHandler>(out BlobFamilyHandler otherBlobFamilyHandler) && !otherBlobFamilyHandler.familyComplete)
            {
                otherBlobFamilyHandler.value = otherValue;
                otherBlobFamilyHandler.hasSplit = true;
                otherBlobFamilyHandler.Invoke(nameof(EndHasSplit), 1f);
                otherBlobFamilyHandler.Initiate();

                value = newValue;
                UpdateFamilyDisplay();

                hasSplit = true;
                Invoke(nameof(EndHasSplit), 1f);

                audioHandler.PlayAudioAction("Split");
            }
        }
    }


    public void EndHasSplit()
    {
        hasSplit = false;
    }

    public void Destroy()
    {
        for (int i = 0; i < childBlobs.Count; i++)
        {
            Destroy(childBlobs[i].gameObject);
        }
        Destroy(gameObject);
    }
}
