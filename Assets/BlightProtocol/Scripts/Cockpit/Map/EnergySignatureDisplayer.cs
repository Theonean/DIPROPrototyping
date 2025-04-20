using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class EnergySignatureDisplayer : MonoBehaviour
{
    [SerializeField] private List<EnergySignatureBase> classSprites;
    [SerializeField] private List<GameObject> frequencySprites;
    [SerializeField] private SpriteRenderer frequencySprite;
    [SerializeField] private List<GameObject> magnitudeSprites;

    [SerializeField] private float fadeDelay = 5f;
    [SerializeField] private float fadeDuration = 5f;

    private SpriteRenderer activeClass;
    private SpriteRenderer activeFrequency;
    private SpriteRenderer[] activeMagnitudes;
    [SerializeField] float lifeTime = 30f; 
    public LineRenderer lineRenderer;

    public void DisplaySignature(EnergySignature signature)
    {
        classSprites[signature.eClass].gameObject.SetActive(true);
        activeClass = classSprites[signature.eClass].GetComponent<SpriteRenderer>();

        frequencySprites[signature.eFrequency].SetActive(true); ;
        activeFrequency = frequencySprites[signature.eFrequency].GetComponent<SpriteRenderer>();

        activeMagnitudes = new SpriteRenderer[signature.eMagnitude];
        for (int i = 0; i < signature.eMagnitude; i++)
        {
            magnitudeSprites[i].SetActive(true);
            magnitudeSprites[i].transform.position = classSprites[signature.eClass].magnitudePositions[i].position;
            activeMagnitudes[i] = magnitudeSprites[i].GetComponent<SpriteRenderer>();
        }
    }
}
