using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using TMPro;

public class SelectedRocketManager : MonoBehaviour
{
    public static SelectedRocketManager Instance { get; private set; }
    [SerializeField] private List<Button> rocketButtons = new List<Button>(4);
    private Dictionary<Button, Rocket> buttonRocketPairs = new Dictionary<Button, Rocket>();
    private List<Rocket> selectedRockets = new List<Rocket>();
    public UnityEvent onRocketsLoaded;


    [Header("Research")]
    [SerializeField] private TextMeshPro researchCostCrystalText;
    [SerializeField] private TextMeshPro researchCostComponentText;
    [SerializeField] private TextMeshPro researchResultDescriptionText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void OnEnable()
    {
        foreach(Button b in rocketButtons) {
            b.OnPressed.AddListener(AddSelectedRocket);
            b.OnReleased.AddListener(RemoveSelectedRocket);
        }
    }

    void OnDisable()
    {
        foreach(Button b in rocketButtons) {
            b.OnPressed.RemoveListener(AddSelectedRocket);
            b.OnReleased.RemoveListener(RemoveSelectedRocket);
        }
    }

    private void Start()
    {
        int index = 0;
        Rocket[] playerRockets = PlayerCore.Instance.GetComponentsInChildren<Rocket>();
        if (playerRockets != null)
        {
            foreach (var button in rocketButtons)
            {
                if (index < playerRockets.Length)
                {
                    buttonRocketPairs[button] = playerRockets[index];
                    index++;
                }
                else
                {
                    Logger.Log("button index out of range of player Rocket count!", LogLevel.ERROR, LogType.COCKPIT);
                }
            }
        }
        else
        {
            Logger.Log("playerRockets not found!", LogLevel.ERROR, LogType.COCKPIT);
        }

        foreach(Button button in rocketButtons) {
            //Select all Rockets by default
            button.OnStartInteract();
        }

        onRocketsLoaded.Invoke();
    }

    public void AddSelectedRocket(Button button)
    {
        if (buttonRocketPairs[button] != null)
        {
            if (selectedRockets.Contains(buttonRocketPairs[button])) {
                Logger.Log("Rocket already selected!", LogLevel.WARNING, LogType.COCKPIT);
                return;
            }
            selectedRockets.Add(buttonRocketPairs[button]);
        }
        else
        {
            Logger.Log("rocket not found!", LogLevel.ERROR, LogType.COCKPIT);
        }
    }

    public void RemoveSelectedRocket(Button button)
    {
        if (buttonRocketPairs[button] != null)
        {
            selectedRockets.Remove(buttonRocketPairs[button]);
        }
        else
        {
            Logger.Log("rocket not found!", LogLevel.ERROR, LogType.COCKPIT);
        }
    }

    public void ChangeComponent(RocketComponentType componentType, GameObject newComponent)
    {
        ACRocketComponent rocketComponent = newComponent.GetComponent<ACRocketComponent>();
        UpdateResearchFields(rocketComponent);

        foreach (Rocket rocket in selectedRockets)
        {
            switch (componentType)
            {
                case RocketComponentType.FRONT:
                    rocket.SetFront(newComponent);
                    break;
                case RocketComponentType.BODY:
                    rocket.SetBody(newComponent);
                    break;
                case RocketComponentType.PROPULSION:
                    rocket.SetPropulsion(newComponent);
                    break;
            }
        }
    }

    public void LevelUpComponent(RocketComponentType componentType)
    {
        string componentName = null;
        ACRocketComponent[] rocketComponents = new ACRocketComponent[4];
        int i = 0;
        switch (componentType)
        {
            case RocketComponentType.FRONT:
                //selectedRocketMirrorDummy.frontComponent.LevelUpComponent();
                foreach (Rocket rocket in selectedRockets)
                {
                    rocketComponents[i] = rocket.frontComponent; 
                    i++;
                }
                break;
            case RocketComponentType.BODY:
                //selectedRocketMirrorDummy.bodyComponent.LevelUpComponent();
                foreach (Rocket rocket in selectedRockets)
                {
                    rocketComponents[i] = rocket.frontComponent;
                    i++;
                }
                break;
            case RocketComponentType.PROPULSION:
                //selectedRocketMirrorDummy.propulsionComponent.LevelUpComponent();
                foreach (Rocket rocket in selectedRockets)
                {
                    rocketComponents[i] = rocket.frontComponent;
                    i++;
                }
                break;
        }

        //First validate if we have enough resources for an upgrade
        (int, int) researchCosts = rocketComponents[0].GetResearchCost();
        componentName = rocketComponents[0].DescriptiveName;

        if (ItemManager.Instance.GetCrystal() >= researchCosts.Item1 
            && ItemManager.Instance.GetComponentAmount(componentName) >= researchCosts.Item2)
        {
            ItemManager.Instance.RemoveCrystal(researchCosts.Item1);
            ItemManager.Instance.RemoveComponent(componentName, researchCosts.Item2);
            ItemManager.Instance.IncreaseItemLevel(componentName);

            foreach(ACRocketComponent rocketComponent in rocketComponents)
            {
                rocketComponent.LevelUpComponent();
            }

            UpdateResearchFields(rocketComponents[0]);
        }
    }

    private void UpdateResearchFields(ACRocketComponent componentToUpgrade)
    {
        (int, int) researchCosts = componentToUpgrade.GetResearchCost();
        int ownedCrystals = ItemManager.Instance.GetCrystal();
        int ownedComponents = ItemManager.Instance.GetComponentAmount(componentToUpgrade.DescriptiveName);

        string crystalCostsText = ownedCrystals + " / " + researchCosts.Item1;
        string componentCostsText = ownedComponents + " / " + researchCosts.Item2;
        string upgradeText = componentToUpgrade.GetResearchDescription();

        researchCostCrystalText.text = crystalCostsText;
        researchCostComponentText.text = componentCostsText;
        researchResultDescriptionText.text = upgradeText;
    }

    
}