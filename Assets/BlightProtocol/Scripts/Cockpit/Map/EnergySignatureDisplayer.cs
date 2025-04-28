using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnergySignatureDisplayer : MonoBehaviour
{
    [SerializeField] private List<GameObject> baseSprites;

    public void DisplaySignature(EnergySignature signature)
    {
        signature.onDestroy.AddListener(DestroyThis);

        GameObject baseSprite = baseSprites.FirstOrDefault(b => b.GetComponent<EnergySignatureBase>().type == signature.baseType);
        
        GameObject  instantiatedBase = Instantiate(baseSprite, transform);
        EnergySignatureBase baseComponent = instantiatedBase.GetComponent<EnergySignatureBase>();

        if (signature.eMagnitude > 0)
        {
            for (int i = 0; i < signature.eMagnitude; i++)
            {
                baseComponent.magnitudeSprites[i].SetActive(true);
            }
        }
    }
    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}
