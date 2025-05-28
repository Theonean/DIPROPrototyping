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
    private Rocket[] rockets = new Rocket[4];
    [SerializeField] private RocketHolder[] rocketHolders = new RocketHolder[4];
    [SerializeField] private ConfiguratorDummyRocket screenRocket;
    [SerializeField] private ComponentDescriptionDisplayer descriptionDisplayer;
    private Rocket selectedRocket;
    public UnityEvent<Rocket> rocketSelected;

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
        if (PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.FPV) rocketSelected.Invoke(selectedRocket);
    }

    private void Start()
    {
        rockets = PlayerCore.Instance.GetComponentsInChildren<Rocket>();

        LoadDummyRockets();
        ActivateSelectedHolder();

        selectedRocket = rockets[selectedRocketIndex];
        rocketSelected.Invoke(selectedRocket);

        UpdateDescription(RocketComponentType.FRONT, selectedRocket.frontComponent);
        UpdateDescription(RocketComponentType.BODY, selectedRocket.bodyComponent);
        UpdateDescription(RocketComponentType.PROPULSION, selectedRocket.propulsionComponent);
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

        UpdateResearchFields(selectedRocket.frontComponent, RocketComponentType.FRONT);
        UpdateResearchFields(selectedRocket.bodyComponent, RocketComponentType.BODY);
        UpdateResearchFields(selectedRocket.propulsionComponent, RocketComponentType.PROPULSION);

        UpdateDescription(RocketComponentType.FRONT, selectedRocket.frontComponent);
        UpdateDescription(RocketComponentType.BODY, selectedRocket.bodyComponent);
        UpdateDescription(RocketComponentType.PROPULSION, selectedRocket.propulsionComponent);

        rocketSelected.Invoke(selectedRocket);
    }

    public void ChangeActiveRocketComponent(RocketComponentType componentType, GameObject newComponent)
    {
        ChangeComponent(selectedRocketIndex, componentType, newComponent);
    }

    public void ChangeComponent(int index, RocketComponentType componentType, GameObject newComponent)
    {
        ACRocketComponent rocketComponent = newComponent.GetComponent<ACRocketComponent>();
        GameObject dummyObject = newComponent.GetComponentInChildren<MeshRenderer>().gameObject;
        UpdateResearchFields(rocketComponent, componentType);
        UpdateDescription(componentType, rocketComponent);

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
        rocketHolders[index].dummyRocket.SetComponent(componentType, dummyObject);
        screenRocket.SetComponent(componentType, dummyObject);


    }

    private void UpdateDescription(RocketComponentType type, ACRocketComponent newComponent)
    {
        string description = newComponent.componentDescription;
        descriptionDisplayer.SetText(type, description);
    }

    public void ApplySelectionToAll(Button button)
    {
        GameObject selectedFront = rockets[selectedRocketIndex].frontComponent.gameObject;
        GameObject selectedBody = rockets[selectedRocketIndex].bodyComponent.gameObject;
        GameObject selectedPropulsion = rockets[selectedRocketIndex].propulsionComponent.gameObject;
        for (int i = 0; i < rockets.Length; i++)
        {
            ChangeComponent(i, RocketComponentType.FRONT, selectedFront);
            ChangeComponent(i, RocketComponentType.BODY, selectedBody);
            ChangeComponent(i, RocketComponentType.PROPULSION, selectedPropulsion);
        }
    }

    public void LevelUpComponent(RocketComponentType componentType)
    {
        ACRocketComponent rocketComponent = null;
        switch (componentType)
        {
            case RocketComponentType.FRONT:
                rocketComponent = rockets[selectedRocketIndex].frontComponent;
                break;
            case RocketComponentType.BODY:
                rocketComponent = rockets[selectedRocketIndex].bodyComponent;
                break;
            case RocketComponentType.PROPULSION:
                rocketComponent = rockets[selectedRocketIndex].propulsionComponent;
                break;
        }

        //First validate if we have enough resources for an upgrade
        (int, int) researchCosts = rocketComponent.GetResearchCost();
        string componentName = rocketComponent.DescriptiveName;

        if (ItemManager.Instance.GetCrystal() >= researchCosts.Item1
            && ItemManager.Instance.GetComponentAmount(componentName) >= researchCosts.Item2)
        {
            ItemManager.Instance.RemoveCrystal(researchCosts.Item1);
            ItemManager.Instance.RemoveComponent(componentName, researchCosts.Item2);
            ItemManager.Instance.IncreaseItemLevel(componentName);

            rocketComponent.LevelUpComponent();

            UpdateResearchFields(rocketComponent, componentType);
        }
    }

    public void LoadResearchFieldsOfActiveRocket()
    {
        UpdateResearchFields(selectedRocket.frontComponent, RocketComponentType.FRONT);
        UpdateResearchFields(selectedRocket.bodyComponent, RocketComponentType.BODY);
        UpdateResearchFields(selectedRocket.propulsionComponent, RocketComponentType.PROPULSION);
    }

    public void UpdateResearchFields(ACRocketComponent componentToUpgrade, RocketComponentType componentType)
    {
        ResearchManager researchManager = ComponentSelectorManager.Instance.GetResearchManager(componentType);

        (int, int) researchCosts = componentToUpgrade.GetResearchCost(ItemManager.Instance.GetItemLevel(componentToUpgrade.DescriptiveName));

        int ownedCrystals = ItemManager.Instance.GetCrystal();
        int ownedComponents = ItemManager.Instance.GetComponentAmount(componentToUpgrade.DescriptiveName);

        string crystalCostsText = ownedCrystals + " / " + researchCosts.Item1;
        string componentCostsText = ownedComponents + " / " + researchCosts.Item2;
        string upgradeText = componentToUpgrade.GetResearchDescription(ItemManager.Instance.GetItemLevel(componentToUpgrade.DescriptiveName));

        researchManager.SetText(crystalCostsText, componentCostsText, upgradeText);
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