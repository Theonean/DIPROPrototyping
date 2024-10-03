using System.Collections.Generic;
using UnityEngine;

public class BlobFamilyHandler : MonoBehaviour
{
    [Header("Value")]
    public int value;
    public int targetValue;
    public bool familyComplete = false;
    public bool initiateOnAwake = false;
    public GameManager gameManager;

    [Header("Blob Physics")]
    public float regularBlobMass;
    public float topBlobMass;
    public float mergeCooldown = 0.2f;
    public float mergeTimer = 0f;

    BoxCollider boxCollider;
    Rigidbody rb;

    [Header("Blob Display")]
    public GameObject blobPrefab;
    public List<GameObject> childBlobs = new List<GameObject>();
    public float yOffset;

    [Header("Blob Colors & Other Effects")]
    public List<Color> stackColors;
    public Color winColor;
    public int colorIncrement;
    private ParticleSystem particles;

    private BlobAudioHandler audioHandler;

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

    private void Update()
    {
        if (mergeTimer > 0f)
        {
            mergeTimer -= Time.deltaTime;
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
            // Calculate new child position
            Vector3 childPos = childBlobs[childBlobs.Count - 1].transform.position + yOffset * childBlobs[childBlobs.Count - 1].transform.up;
            if (firstChild && i == 0)
                childPos = Vector3.zero;
                
            GameObject newChild = Instantiate(blobPrefab, childPos, Quaternion.identity);
            newChild.GetComponent<IndividualBlobHandler>().parentInteractable = GetComponent<BlobInteractable>();

            // Find Rigidbody to attach joint to, if its the first child, connect to Family Rigidbody
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
        // Make top blob a bit heavier for bendy action
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
        if (collision.gameObject.CompareTag("Blob") && mergeTimer <= 0)
        {
            // get parent Interactable RB if collider hit was on individual blob
            if (!collision.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rbOther))
                rbOther = collision.gameObject.GetComponent<IndividualBlobHandler>().parentInteractable.GetComponent<Rigidbody>();

            BlobFamilyHandler otherBlobFamilyHandler = collision.gameObject.GetComponent<BlobFamilyHandler>();
            if (otherBlobFamilyHandler == null)
                otherBlobFamilyHandler = collision.gameObject.GetComponent<IndividualBlobHandler>().parentInteractable.GetComponent<BlobFamilyHandler>();

            // compare velocity to determine where the merge happens
            float velocitySelf = rb.velocity.magnitude;
            float velocityOther = rbOther.velocity.magnitude;

            if (!familyComplete && otherBlobFamilyHandler.mergeTimer <= 0 && !otherBlobFamilyHandler.familyComplete)
            {
                if (velocitySelf > velocityOther)
                {
                    Merge(otherBlobFamilyHandler);
                }
                else if (velocitySelf == velocityOther && GetInstanceID() > rbOther.GetInstanceID())
                {
                    Merge(otherBlobFamilyHandler);
                }
            }
        }
        // play impact sound when hitting ground
        else if (collision.gameObject.layer == 10)
        {
            audioHandler.PlayAudioAction("Impact");
        }
    }

    private void Merge(BlobFamilyHandler otherBlobFamilyHandler)
    {
        // Cancel merge when colliding with itself
        if (GetInstanceID() == otherBlobFamilyHandler.GetInstanceID())
        {
            Debug.Log("self merge attempt");
            return;
        }

        // Merge
        if (value + otherBlobFamilyHandler.value <= targetValue)
        {
            value += otherBlobFamilyHandler.value;
            otherBlobFamilyHandler.DestroyFamily();
            UpdateFamilyDisplay();

            audioHandler.PlayAudioAction("Merge");
            particles.Play();
        }
        // Play Impact sound when merge fails
        else
        {
            audioHandler.PlayAudioAction("Impact");
        }

        // Complete Family when target is reached
        if (value == targetValue)
        {
            CompleteFamily();

            // If diegetic target not active, add progress directly
            if (!gameManager.enableDiegeticTarget)
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

            // Create new Family with otherValue
            GameObject newFamily = Instantiate(gameObject, transform.position + new Vector3(0.5f, 0, 0), Quaternion.identity);

            if (newFamily.TryGetComponent<BlobFamilyHandler>(out BlobFamilyHandler otherBlobFamilyHandler))
            {
                otherBlobFamilyHandler.value = otherValue;
                otherBlobFamilyHandler.Initiate();

                // Refresh merge timer to avoid instant re-merge
                mergeTimer = mergeCooldown;
                otherBlobFamilyHandler.mergeTimer = mergeCooldown;

                value = newValue;
                UpdateFamilyDisplay();

                audioHandler.PlayAudioAction("Split");
            }
        }
    }

    public void DestroyFamily()
    {
        for (int i = 0; i < childBlobs.Count; i++)
        {
            Destroy(childBlobs[i].gameObject);
        }
        Destroy(gameObject);
    }
}
