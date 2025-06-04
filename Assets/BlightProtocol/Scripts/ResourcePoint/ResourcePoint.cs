using UnityEngine;
using UnityEngine.VFX;

public class ResourcePoint : MonoBehaviour
{
    public int resourceAmount = 100;
    private float gasHarvested = 0f;

    [SerializeField] private VisualEffect[] smokeVFX;
    [SerializeField] private GameObject glowingPlane;
    [SerializeField] private EnergySignature energySignature;
    [SerializeField] private Collider _collider;

    public bool HarvestResource(float amount)
    {
        if (smokeVFX[0] != null && smokeVFX[0].HasAnySystemAwake())
        {
            foreach (var effect in smokeVFX)
            {
                effect.Stop();
            }
        }
        if (resourceAmount <= 0f)
            return false;

        gasHarvested += amount;

        if (gasHarvested > 1f)
        {
            int gasInt = (int)gasHarvested;
            gasHarvested -= gasInt;

            ItemManager.Instance.AddGas(gasInt);
            resourceAmount -= gasInt;
        }

        return true;
    }

    public void FinishHarvest()
    {
        glowingPlane.SetActive(false);
        if (energySignature != null)
        {
            Destroy(energySignature.gameObject);
        }
        _collider.enabled = false;
        enabled = false;
    }
}