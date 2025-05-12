
using UnityEngine;

public class ResourceScreen : ACScreenValueDisplayer
{
    private ItemManager itemManager;
    protected void Start()
    {
        itemManager = ItemManager.Instance;
        if (itemManager != null)
        {
            itemManager.crystalAmountChanged.RemoveListener(OnCrystalAmountChanged);
            itemManager.notEnoughCrystals.RemoveListener(OnNotEnoughCrystals);
            itemManager.crystalAmountChanged.AddListener(OnCrystalAmountChanged);
            itemManager.notEnoughCrystals.AddListener(OnNotEnoughCrystals);
        }

        SetValue(itemManager.GetCrystal());
    }

    protected void OnEnable()
    {
        if (itemManager != null)
        {
            itemManager.crystalAmountChanged.RemoveListener(OnCrystalAmountChanged);
            itemManager.notEnoughCrystals.RemoveListener(OnNotEnoughCrystals);
            itemManager.crystalAmountChanged.AddListener(OnCrystalAmountChanged);
            itemManager.notEnoughCrystals.AddListener(OnNotEnoughCrystals);
        }

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
        if (delta < 0) OnFeedback();
    }

    public void OnNotEnoughCrystals()
    {
        OnFeedback();
    }

}
