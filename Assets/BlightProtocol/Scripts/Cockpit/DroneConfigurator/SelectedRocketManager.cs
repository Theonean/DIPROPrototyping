using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class SelectedRocketManager : MonoBehaviour
{
    public static SelectedRocketManager Instance { get; private set; }
    [SerializeField] private List<Button> rocketButtons = new List<Button>(4);
    private Dictionary<Button, Rocket> buttonRocketPairs = new Dictionary<Button, Rocket>();
    private List<Rocket> selectedRockets = new List<Rocket>();
    public UnityEvent onRocketsLoaded;

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
        Debug.Log(buttonRocketPairs[button]);
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
        switch (componentType)
        {
            case RocketComponentType.FRONT:
                //selectedRocketMirrorDummy.frontComponent.LevelUpComponent();
                foreach (Rocket rocket in selectedRockets)
                {
                    rocket.frontComponent.LevelUpComponent();
                }
                break;
            case RocketComponentType.BODY:
                //selectedRocketMirrorDummy.bodyComponent.LevelUpComponent();
                foreach (Rocket rocket in selectedRockets)
                {
                    rocket.bodyComponent.LevelUpComponent();
                }
                break;
            case RocketComponentType.PROPULSION:
                //selectedRocketMirrorDummy.propulsionComponent.LevelUpComponent();
                foreach (Rocket rocket in selectedRockets)
                {
                    rocket.propulsionComponent.LevelUpComponent();
                }
                break;
        }
    }
}

