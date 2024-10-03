using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DiegeticTarget : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject targetSection;
    public float targetYOffset = 1;

    private CapsuleCollider capsCollider;

    private int progressGoal;

    public ParticleSystem particles;

    [Header("Text")]
    public TextMeshPro progress;

    [Header("Sections")]
    private List<GameObject> sections = new List<GameObject>();

    [Header("Color")]
    public GameObject blobFamilyPrefab;
    List<Color> stackColors;
    int colorIncrement;
    public float progressAnimDelay = 0.2f;

    public void Initiate(int targetValue, int targetPairs)
    {
        capsCollider = GetComponent<CapsuleCollider>();
        particles = GetComponentInChildren<ParticleSystem>();
        
        // get colors
        stackColors = blobFamilyPrefab.GetComponent<BlobFamilyHandler>().stackColors;
        colorIncrement = blobFamilyPrefab.GetComponent<BlobFamilyHandler>().colorIncrement;

        SetTarget(targetValue);
        progressGoal = targetPairs;
        SetProgress(0);
    }
    public void SetTarget(int target)
    {
        for (int i = 0; i < target; i++)
        {
            GameObject targetSectionObject = Instantiate(targetSection, transform.position + transform.up * targetYOffset*i, Quaternion.identity, transform);
            sections.Add(targetSectionObject);

            SetStackColors();

            if (i == target-1)
            {
                progress.transform.localPosition = Vector3.up * targetYOffset * i;
                capsCollider.height = targetYOffset * i;
            }
        }
    }

    private void SetStackColors()
    {
        for (int i = 0; i < sections.Count; i++)
        {
            int colorIndex = (i / colorIncrement) % stackColors.Count;
            Color newColor = new Color(stackColors[colorIndex].r, stackColors[colorIndex].g, stackColors[colorIndex].b, 0.75f);
            sections[i].GetComponent<Renderer>().material.SetColor("_BaseColor", newColor);
        }
    }

    public void SetProgress(int progressValue)
    {
        progress.text = progressValue.ToString() + "/" + progressGoal.ToString();
        DoProgressAnimation();
    }
    private void DoProgressAnimation()
    {
        for (int i = 0; i < sections.Count; i++)
        {
            StartCoroutine(MakeSectionGreen(i, progressAnimDelay * i));
        }
    }

    private IEnumerator MakeSectionGreen(int i, float delay)
    {
        yield return new WaitForSeconds(delay);
        Color newColor = new(0, 1, 0, 1f);
        sections[i].GetComponent<Renderer>().material.SetColor("_BaseColor", newColor);
        if (i == sections.Count - 1)
            Invoke(nameof(SetStackColors), 0.5f);
    }

    private float collisionTimeout = 0.5f;
    private float collisionTimer = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Blob") && collisionTimer <= 0)
        {
            if (!other.gameObject.TryGetComponent<BlobFamilyHandler>(out BlobFamilyHandler blob))
                blob = other.gameObject.GetComponent<IndividualBlobHandler>().parentInteractable.GetComponent<BlobFamilyHandler>();

            if (blob.familyComplete)
            {
                gameManager.AddProgress();
                blob.DestroyFamily();
                particles.Play();
                collisionTimer = collisionTimeout;
                GetComponentInChildren<BlobAudioHandler>().PlayAudioAction("Family");
            }
        }
    }

    private void Update()
    {
        if (collisionTimer >= 0)
        {
            collisionTimer -= Time.deltaTime;
        }
    }
}
