
using UnityEngine;

public class CrystalScreen : ACScreenValueDisplayer
{
    private ItemManager itemManager;
    protected void Awake()
    {
        itemManager = ItemManager.Instance;
    }
    protected void OnEnable()
    {
        itemManager.crystalAmountChanged.AddListener(OnCrystalAmountChanged);
        itemManager.notEnoughCrystals.AddListener(OnNotEnoughCrystals);

        SetValue(itemManager.GetCrystal());
    }

    protected void OnDisable()
    {
        itemManager.crystalAmountChanged.RemoveListener(OnCrystalAmountChanged);
        itemManager.notEnoughCrystals.RemoveListener(OnNotEnoughCrystals);
    }

    public void OnCrystalAmountChanged(int delta)
    {
        SetValue(itemManager.GetCrystal());
        if (delta < 0) Flash();
    }

    public void OnNotEnoughCrystals()
    {
        Flash();
    }

}
