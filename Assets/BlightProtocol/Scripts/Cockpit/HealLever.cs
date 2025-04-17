using TMPro;
using UnityEngine;

public class HealLever : ACLever
{
    private Harvester harvester;
    public TextMeshPro healFeedback;
    private ResourceHandler resourceHandler;
    public int healAmount = 10;
    public int cost = 100;

    void Start()
    {
        harvester = Harvester.Instance;
        resourceHandler = ResourceHandler.Instance;
    }

    protected override void OnPulled(float normalizedValue)
    {
        if (normalizedValue >= 0.9f && !isPulled)
        {
            if (harvester.health.IsAtFullHealth()) {
                healFeedback.text = "Already at full health!";
                ResetLever();
            }
            else if (resourceHandler.GetAmount(resourceHandler.fuelResource) > cost)
            {
                harvester.health.Heal(healAmount);
                resourceHandler.Consume(resourceHandler.fuelResource, cost, 1f);
                isPulled = true;
                ResetLever();
            }
            else {
                healFeedback.text = "Not enough fuel!";
                ResetLever();
            }
        }
    }
}
