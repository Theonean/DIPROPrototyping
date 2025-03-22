using System.Collections.Generic;
using UnityEngine;

public class EnergySignatureDisplayer : MonoBehaviour
{
    [SerializeField] private List<EnergySignatureBase> classSprites;
    [SerializeField] private List<GameObject> frequencySprites;
    [SerializeField] private SpriteRenderer frequencySprite;
    [SerializeField] private List<GameObject> magnitudeSprites;

    public void DisplaySignature(EnergySignature signature)
    {
        frequencySprites[signature.eFrequency].SetActive(true);;
        classSprites[signature.eClass].gameObject.SetActive(true);
        for (int i = 0; i < signature.eMagnitude; i++)
        {
            
            magnitudeSprites[i].SetActive(true);
            magnitudeSprites[i].transform.position = classSprites[signature.eClass].magnitudePositions[i].position;
        }

    }
}
