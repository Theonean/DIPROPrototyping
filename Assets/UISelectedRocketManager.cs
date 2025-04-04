using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UISelectedRocketManager : MonoBehaviour
{
    public static UISelectedRocketManager Instance { get; private set; }
    public Rocket selectedRocketMirrorDummy;
    public Rocket selectedRocket;
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
        SetSelectedRocket(GetComponentsInChildren<Rocket>().FirstOrDefault());
    }


    public void SetSelectedRocket(Rocket rocket)
    {
        selectedRocket = rocket;
        equivalentPlayerRocket = PlayerCore.Instance.GetComponentsInChildren<Rocket>().Where(x => x.name == selectedRocket.name).FirstOrDefault();
        Debug.Log("Selected Rocket: " + selectedRocket.name);

        //Copy settins to dummy
        selectedRocketMirrorDummy.SetFront(selectedRocket.frontComponent.gameObject);
        selectedRocketMirrorDummy.SetBody(selectedRocket.bodyComponent.gameObject);
        selectedRocketMirrorDummy.SetPropulsion(selectedRocket.propulsionComponent.gameObject);
    }

    public void ChangeComponent(RocketComponentType componentType, GameObject newComponent)
    {
        switch (componentType)
        {
            case RocketComponentType.FRONT:
                selectedRocket.SetFront(newComponent);
                equivalentPlayerRocket.SetFront(newComponent);
                selectedRocketMirrorDummy.SetFront(newComponent);
                break;
            case RocketComponentType.BODY:
                selectedRocket.SetBody(newComponent);
                equivalentPlayerRocket.SetBody(newComponent);
                selectedRocketMirrorDummy.SetBody(newComponent);
                break;
            case RocketComponentType.PROPULSION:
                selectedRocket.SetPropulsion(newComponent);
                equivalentPlayerRocket.SetPropulsion(newComponent);
                selectedRocketMirrorDummy.SetPropulsion(newComponent);
                break;
        }
    }
}
