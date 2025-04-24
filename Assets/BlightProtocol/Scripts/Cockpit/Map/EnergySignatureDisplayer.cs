using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnergySignatureDisplayer : MonoBehaviour
{
    [SerializeField] private List<EnergySignatureBase> baseSprites;
    [SerializeField] private List<GameObject> magnitudeSprites;

    [SerializeField] private float fadeDelay = 5f;
    [SerializeField] private float fadeDuration = 5f;
    [SerializeField] float lifeTime = 30f;
    public LineRenderer lineRenderer;

    public void DisplaySignature(EnergySignature signature)
    {
        Debug.Log(signature.eMagnitude);
        signature.onDestroy.AddListener(DestroyThis);
        EnergySignatureBase baseSprite = baseSprites.FirstOrDefault(b => b.type == signature.baseType);
        baseSprite.gameObject.SetActive(true);
        if (signature.eMagnitude > 0)
        {
            for (int i = 0; i < signature.eMagnitude; i++)
            {
                magnitudeSprites[i].SetActive(true);
                magnitudeSprites[i].transform.position = baseSprite.magnitudePositions[i].position;
                magnitudeSprites[i].transform.rotation = baseSprite.magnitudePositions[i].rotation;
            }
        }

    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}
