using TMPro;
using UnityEngine;

public class HealLever : ACLever
{
    private Harvester harvester;
    public TextMeshPro healFeedback;
    private ItemManager ItemManager;
    public int healAmount = 10;
    public int cost = 100;

    protected override void Start()
    {
        base.Start();
        harvester = Harvester.Instance;
        ItemManager = ItemManager.Instance;
    }

    protected override void OnPulled(float normalizedValue)
    {
        if (normalizedValue >= 0.9f && !isPulled)
        {
            if (harvester.health.IsAtFullHealth()) {
                healFeedback.text = "Already at full health!";
                ResetLever();
            }
            else if (ItemManager.RemoveCrystal(cost))
            {
                harvester.health.Heal(healAmount);
                isPulled = true;
                ResetLever();
            }
            else {
                healFeedback.text = "Not enough crystals!";
                ResetLever();
            }
        }
    }
}
