using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UISelectedRocketManager : MonoBehaviour
{
    public static UISelectedRocketManager Instance { get; private set; }
    //public Rocket selectedRocketMirrorDummy;
    public Rocket selectedRocket;
    public bool setAllRocketsAtOnce = true;
    [SerializeField] private Rocket[] rockets = new Rocket[4];
    private Rocket equivalentPlayerRocket;

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

    private void Start()
    {
        //if (setAllRocketsAtOnce) selectedRocketMirrorDummy.gameObject.SetActive(false);

        SetSelectedRocket(GetComponentsInChildren<Rocket>().FirstOrDefault());
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.J))
        {
            LevelUpComponent(RocketComponentType.FRONT);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            LevelUpComponent(RocketComponentType.BODY);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            LevelUpComponent(RocketComponentType.PROPULSION);
        }*/
    }

    private void LevelUpComponent(RocketComponentType componentType)
    {
        PlayerCore playerCore = PlayerCore.Instance;
        Rocket[] rockets = playerCore.GetComponentsInChildren<Rocket>();
        switch (componentType)
        {
            case RocketComponentType.FRONT:
                //selectedRocketMirrorDummy.frontComponent.LevelUpComponent();
                foreach (Rocket rocket in rockets)
                {
                    rocket.frontComponent.LevelUpComponent();
                }
                break;
            case RocketComponentType.BODY:
                //selectedRocketMirrorDummy.bodyComponent.LevelUpComponent();
                foreach (Rocket rocket in rockets)
                {
                    rocket.bodyComponent.LevelUpComponent();
                }
                break;
            case RocketComponentType.PROPULSION:
                //selectedRocketMirrorDummy.propulsionComponent.LevelUpComponent();
                foreach (Rocket rocket in rockets)
                {
                    rocket.propulsionComponent.LevelUpComponent();
                }
                break;
        }
    }


    public void SetSelectedRocket(Rocket rocket)
    {
        if (setAllRocketsAtOnce) return;

        selectedRocket = rocket;
        equivalentPlayerRocket = PlayerCore.Instance.GetComponentsInChildren<Rocket>().Where(x => x.name == selectedRocket.name).FirstOrDefault();
        Debug.Log("Selected Rocket: " + selectedRocket.name);

        //Copy settins to dummy
        /*selectedRocketMirrorDummy.SetFront(selectedRocket.frontComponent.gameObject);
        selectedRocketMirrorDummy.SetBody(selectedRocket.bodyComponent.gameObject);
        selectedRocketMirrorDummy.SetPropulsion(selectedRocket.propulsionComponent.gameObject);*/
    }

    public void ChangeComponent(RocketComponentType componentType, GameObject newComponent)
    {
        if (setAllRocketsAtOnce)
        {
            foreach (var rocket in rockets)
            {
                selectedRocket = rocket;
                equivalentPlayerRocket = PlayerCore.Instance.GetComponentsInChildren<Rocket>().Where(x => x.name == selectedRocket.name).FirstOrDefault();
                switch (componentType)
                {
                    case RocketComponentType.FRONT:
                        selectedRocket.SetFront(newComponent);
                        equivalentPlayerRocket.SetFront(newComponent);
                        break;
                    case RocketComponentType.BODY:
                        selectedRocket.SetBody(newComponent);
                        equivalentPlayerRocket.SetBody(newComponent);
                        break;
                    case RocketComponentType.PROPULSION:
                        selectedRocket.SetPropulsion(newComponent);
                        equivalentPlayerRocket.SetPropulsion(newComponent);
                        break;
                }
            }
        }
        else
        {
            switch (componentType)
            {
                case RocketComponentType.FRONT:
                    selectedRocket.SetFront(newComponent);
                    equivalentPlayerRocket.SetFront(newComponent);
                    //selectedRocketMirrorDummy.SetFront(newComponent);
                    break;
                case RocketComponentType.BODY:
                    selectedRocket.SetBody(newComponent);
                    equivalentPlayerRocket.SetBody(newComponent);
                    //selectedRocketMirrorDummy.SetBody(newComponent);
                    break;
                case RocketComponentType.PROPULSION:
                    selectedRocket.SetPropulsion(newComponent);
                    equivalentPlayerRocket.SetPropulsion(newComponent);
                    //selectedRocketMirrorDummy.SetPropulsion(newComponent);
                    break;
            }
        }
    }
}
