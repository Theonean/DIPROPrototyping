using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class RotatingSelectedRocketManager : MonoBehaviour
{
    public static RotatingSelectedRocketManager Instance { get; private set; }
    [SerializeField] private Button turnButton;
    [SerializeField] private GameObject barrel;
    [SerializeField] private float rotationTime;
    [SerializeField] private AnimationCurve rotationCurve;
    private bool isRotating;

    private int selectedRocketIndex = 0;
    public Rocket[] rockets = new Rocket[4];
    [SerializeField] private RocketHolder[] rocketHolders = new RocketHolder[4];
    [SerializeField] private ConfiguratorDummyRocket screenRocket;
    [SerializeField] private ComponentDescriptionDisplayer descriptionDisplayer;
    private Rocket selectedRocket;
    public UnityEvent<Rocket> rocketSelected;

    private ResearchManager frontResearchManager, bodyResearchManager, propResearchManager;

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
        turnButton.OnPressed.AddListener(RotateBarrel);
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(OnPerspectiveSwitched);
    }

    void OnDisable()
    {
        turnButton.OnPressed.RemoveListener(RotateBarrel);
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.RemoveListener(OnPerspectiveSwitched);
    }

    private void OnPerspectiveSwitched()
    {
        if (PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.FPV)
        {
            rocketSelected.Invoke(selectedRocket);
            UpdateAllResearchFields();
        }
    }

    private void Start()
    {
        rockets = PlayerCore.Instance.GetComponentsInChildren<Rocket>();

        frontResearchManager = ComponentSelectorManager.Instance.GetResearchManager(RocketComponentType.FRONT);
        bodyResearchManager = ComponentSelectorManager.Instance.GetResearchManager(RocketComponentType.BODY);
        propResearchManager = ComponentSelectorManager.Instance.GetResearchManager(RocketComponentType.PROPULSION);

        LoadDummyRockets();
        ActivateSelectedHolder();

        selectedRocket = rockets[selectedRocketIndex];
        rocketSelected.Invoke(selectedRocket);

        UpdateDescription(RocketComponentType.FRONT, selectedRocket.frontComponent);
        UpdateDescription(RocketComponentType.BODY, selectedRocket.bodyComponent);
        UpdateDescription(RocketComponentType.PROPULSION, selectedRocket.propulsionComponent);

        descriptionDisplayer.ShowLock(RocketComponentType.FRONT, !ItemManager.Instance.GetComponentEntry(selectedRocket.frontComponent.DescriptiveName).isUnlocked);
        descriptionDisplayer.ShowLock(RocketComponentType.BODY, !ItemManager.Instance.GetComponentEntry(selectedRocket.bodyComponent.DescriptiveName).isUnlocked);
        descriptionDisplayer.ShowLock(RocketComponentType.PROPULSION, !ItemManager.Instance.GetComponentEntry(selectedRocket.propulsionComponent.DescriptiveName).isUnlocked);
    }

    public void RotateBarrel(Button button)
    {
        if (isRotating) return;
        selectedRocketIndex++;

        if (selectedRocketIndex >= rockets.Count())
        {
            selectedRocketIndex = 0;
        }

        ActivateSelectedHolder();
        selectedRocket = rockets[selectedRocketIndex];
        StartCoroutine(SmoothRotateBarrel(-90f));
    }

    private void ActivateSelectedHolder()
    {
        for (int i = 0; i < rocketHolders.Length; i++)
        {
            if (i == selectedRocketIndex) rocketHolders[i].SetSelected(true);
            else rocketHolders[i].SetSelected(false);
        }
    }

    private IEnumerator SmoothRotateBarrel(float amount)
    {
        isRotating = true;
        float elapsedTime = 0f;
        Quaternion startRotation = barrel.transform.localRotation;
        while (elapsedTime < rotationTime)
        {
            float newRot = Mathf.Lerp(startRotation.eulerAngles.y, startRotation.eulerAngles.y + amount, rotationCurve.Evaluate(elapsedTime / rotationTime));
            barrel.transform.localRotation = Quaternion.Euler(startRotation.eulerAngles.x, newRot, startRotation.eulerAngles.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        barrel.transform.localRotation = Quaternion.Euler(startRotation.eulerAngles.x, startRotation.eulerAngles.y + amount, startRotation.eulerAngles.z);
        isRotating = false;

        UpdateAllResearchFields(true);

        UpdateDescription(RocketComponentType.FRONT, selectedRocket.frontComponent);
        UpdateDescription(RocketComponentType.BODY, selectedRocket.bodyComponent);
        UpdateDescription(RocketComponentType.PROPULSION, selectedRocket.propulsionComponent);

        rocketSelected.Invoke(selectedRocket);
    }

    public void ChangeActiveRocketSelectedComponent(RocketComponentType componentType, GameObject newComponent, bool unlocked)
    {
        ChangeComponent(selectedRocketIndex, componentType, newComponent, unlocked);
    }

    public void ChangeComponent(int index, RocketComponentType componentType, GameObject newComponent, bool unlocked)
    {
        ACRocketComponent rocketComponent = newComponent.GetComponent<ACRocketComponent>();
        GameObject dummyObject = newComponent.GetComponentInChildren<MeshRenderer>().gameObject;
        if (unlocked)
        {
            switch (componentType)
            {
                case RocketComponentType.FRONT:
                    rockets[index].SetFront(newComponent);
                    break;
                case RocketComponentType.BODY:

                    rockets[index].SetBody(newComponent);
                    break;
                case RocketComponentType.PROPULSION:
                    rockets[index].SetPropulsion(newComponent);
                    break;
            }
            // set dummy rocket
            rocketHolders[index].dummyRocket.SetComponent(componentType, dummyObject);
        }

        if (index == selectedRocketIndex)
        {
            // update panels and dummy rockets
            descriptionDisplayer.ShowLock(componentType, !unlocked);

            screenRocket.SetComponent(componentType, dummyObject);
            UpdateResearchFields(componentType);
            UpdateDescription(componentType, rocketComponent);
        }
    }

    private void UpdateDescription(RocketComponentType type, ACRocketComponent newComponent)
    {
        string description = newComponent.componentDescription;
        descriptionDisplayer.SetText(type, description);

        GameObject dummyObject = newComponent.GetComponentInChildren<MeshRenderer>().gameObject;
        screenRocket.SetComponent(type, dummyObject);
    }

    public void LevelUpComponent(RocketComponentType componentType)
    {
        GameObject componentPrefab = null;
        ACRocketComponent rocketComponent = null;
        switch (componentType)
        {
            case RocketComponentType.FRONT:
                componentPrefab = ComponentSelectorManager.Instance.frontSelector.GetCurrentSelectionPrefab(out bool frontUnlocked);
                rocketComponent = componentPrefab.GetComponent<ACRocketComponent>();
                break;
            case RocketComponentType.BODY:
                componentPrefab = ComponentSelectorManager.Instance.bodySelector.GetCurrentSelectionPrefab(out bool bodyUnlocked);
                rocketComponent = componentPrefab.GetComponent<ACRocketComponent>();
                break;
            case RocketComponentType.PROPULSION:
                componentPrefab = ComponentSelectorManager.Instance.propSelector.GetCurrentSelectionPrefab(out bool propUnlocked);
                rocketComponent = componentPrefab.GetComponent<ACRocketComponent>();
                break;
        }

        if (rocketComponent == null || componentPrefab == null)
        {
            Logger.Log("component to upgrade not found!", LogLevel.ERROR, LogType.ROCKETS);
            return;
        }

        //First validate if we have enough resources for an upgrade
        int researchCosts = rocketComponent.GetResearchCost();
        string componentName = rocketComponent.DescriptiveName;

        ComponentEntry entry = ItemManager.Instance.GetComponentEntry(componentName);
        if (ItemManager.Instance.GetCrystal() >= researchCosts && entry.highestLevelUpgraded <= rocketComponent.maxComponentLevel - 1)
        {
            if (entry.highestLevelUpgraded == 0)
            {
                entry.isUnlocked = true;
                descriptionDisplayer.ShowLock(componentType, false);

                // set the new component on the selected rocket
                switch (componentType)
                {
                    case RocketComponentType.FRONT:
                        ChangeComponent(selectedRocketIndex, componentType, componentPrefab, true);
                        break;
                    case RocketComponentType.BODY:
                        ChangeComponent(selectedRocketIndex, componentType, componentPrefab, true);
                        break;
                    case RocketComponentType.PROPULSION:
                        ChangeComponent(selectedRocketIndex, componentType, componentPrefab, true);
                        break;
                }
            }

            ItemManager.Instance.RemoveCrystal(researchCosts);
            ItemManager.Instance.IncreaseItemLevel(componentName);

            rocketComponent.LevelUpComponent();

            UpdateAllResearchFields();
        }
    }

    public void UpdateAllResearchFields(bool useSelectedRocket = false)
    {
        UpdateResearchFields(RocketComponentType.FRONT, useSelectedRocket);
        UpdateResearchFields(RocketComponentType.BODY, useSelectedRocket);
        UpdateResearchFields(RocketComponentType.PROPULSION, useSelectedRocket);
    }

    public void UpdateResearchFields(RocketComponentType componentType, bool useSelectedRocket = false)
    {
        ACRocketComponent component = null;
        ResearchManager researchManager = null;
        switch (componentType)
        {
            case RocketComponentType.FRONT:
                if (useSelectedRocket)
                {
                    component = selectedRocket.frontComponent;
                }
                else
                {
                    component = ComponentSelectorManager.Instance.frontSelector.GetCurrentSelectionPrefab(out bool frontUnlocked).GetComponent<ACRocketComponent>();
                }
                researchManager = frontResearchManager;
                break;
            case RocketComponentType.BODY:
                if (useSelectedRocket)
                {
                    component = selectedRocket.bodyComponent;
                }
                else
                {
                    component = ComponentSelectorManager.Instance.bodySelector.GetCurrentSelectionPrefab(out bool bodyUnlocked).GetComponent<ACRocketComponent>();
                }
                researchManager = bodyResearchManager;
                break;
            case RocketComponentType.PROPULSION:
                if (useSelectedRocket)
                {
                    component = selectedRocket.propulsionComponent;
                }
                else
                {
                    component = ComponentSelectorManager.Instance.propSelector.GetCurrentSelectionPrefab(out bool propUnlocked).GetComponent<ACRocketComponent>();
                }
                researchManager = propResearchManager;
                break;

        }

        int componentLevel = ItemManager.Instance.GetItemLevel(component.DescriptiveName);

        int researchCosts = component.GetResearchCost(componentLevel);

        int ownedCrystals = ItemManager.Instance.GetCrystal();

        string crystalCostsText = ownedCrystals + " / " + researchCosts;
        string upgradeText = component.GetResearchDescription(componentLevel);

        researchManager.SetText(crystalCostsText, upgradeText);
    }

    private void LoadDummyRockets()
    {
        int index = 0;
        foreach (RocketHolder holder in rocketHolders)
        {
            GameObject frontComponent = rockets[index].frontComponent.GetComponentInChildren<MeshRenderer>().gameObject;
            GameObject bodyComponent = rockets[index].bodyComponent.GetComponentInChildren<MeshRenderer>().gameObject;
            GameObject propulsionComponent = rockets[index].propulsionComponent.GetComponentInChildren<MeshRenderer>().gameObject;

            holder.dummyRocket.SetComponent(RocketComponentType.FRONT, frontComponent);
            holder.dummyRocket.SetComponent(RocketComponentType.BODY, bodyComponent);
            holder.dummyRocket.SetComponent(RocketComponentType.PROPULSION, propulsionComponent);

            if (index == selectedRocketIndex)
            {
                screenRocket.SetComponent(RocketComponentType.FRONT, frontComponent);
                screenRocket.SetComponent(RocketComponentType.BODY, bodyComponent);
                screenRocket.SetComponent(RocketComponentType.PROPULSION, propulsionComponent);
            }
            index++;
        }

    }
}