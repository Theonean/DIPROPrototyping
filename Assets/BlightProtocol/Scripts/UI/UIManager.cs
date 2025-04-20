using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    PerspectiveSwitcher perspectiveSwitcher;
    public GameObject topDownUI;
    public GameObject fpvUI;
    public GameObject generalUI;

    public static UIHandler Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        perspectiveSwitcher = PerspectiveSwitcher.Instance;
        SwitchUI();
        perspectiveSwitcher.onPerspectiveSwitched.AddListener(SwitchUI);
    }

    void OnEnable()
    {
        if (perspectiveSwitcher != null)
        {
            perspectiveSwitcher.onPerspectiveSwitched.RemoveListener(SwitchUI);
            perspectiveSwitcher.onPerspectiveSwitched.AddListener(SwitchUI);
        }
    }

    void OnDisable()
    {
        if (perspectiveSwitcher != null)
        {
            perspectiveSwitcher.onPerspectiveSwitched.RemoveListener(SwitchUI);
        }
    }

    void SwitchUI()
    {
        Logger.Log("Switching UI to" + perspectiveSwitcher.currentPerspective, LogLevel.INFO, LogType.PERSPECTIVESWITCH);
        switch (perspectiveSwitcher.currentPerspective)
        {
            case CameraPerspective.DRONE:
                topDownUI.SetActive(true);
                fpvUI.SetActive(false);
                generalUI.SetActive(true);
                break;

            case CameraPerspective.FPV:
                topDownUI.SetActive(false);
                fpvUI.SetActive(true);
                generalUI.SetActive(true);
                break;

            case CameraPerspective.SWITCHING:
                topDownUI.SetActive(false);
                fpvUI.SetActive(false);
                generalUI.SetActive(false);
                break;
        }
    }
}
