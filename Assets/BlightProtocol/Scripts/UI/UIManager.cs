using System.Collections;
using System.Collections.Generic;
using Assets.BlightProtocol.Scripts;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    PerspectiveSwitcher perspectiveSwitcher;
    public GameObject topDownUI;
    public GameObject fpvUI;
    public GameObject switchingUI;
    public GameObject generalUI;
    public CanvasGroup howToWinGroup;
    private bool howToWinActive = false;
    public float howToWinFadeTime = 1f;

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
        if (!TutorialManager.Instance.IsTutorialOngoing())
        {
            ShowHowToWin(true);
        }
    }

    void Update()
    {
        if (howToWinActive && Input.GetMouseButtonDown(0))
        {
            if (EndOfGameManager.Instance.isPaused)
            {
                ShowHowToWinInstant(false);
            }
            else
            {
                ShowHowToWin(false);
            }
        }
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
                switchingUI.SetActive(false);
                generalUI.SetActive(true);
                break;

            case CameraPerspective.FPV:
                topDownUI.SetActive(false);
                fpvUI.SetActive(true);
                switchingUI.SetActive(false);
                generalUI.SetActive(true);
                break;

            case CameraPerspective.SWITCHING:
                topDownUI.SetActive(false);
                fpvUI.SetActive(false);
                switchingUI.SetActive(true);
                generalUI.SetActive(true);
                break;
        }
    }

    public void ShowHowToWin(bool show)
    {
        float alphaTarget = show ? 1f : 0f;
        howToWinActive = show;
        StartCoroutine(FadeHowToWin(alphaTarget));
    }

    public void ShowHowToWinInstant(bool show)
    {
        howToWinActive = show;
        howToWinGroup.alpha = show ? 1f : 0f;
    }

    private IEnumerator FadeHowToWin(float target)
    {
        float startA = howToWinGroup.alpha;
        float elapsedTime = 0f;
        while (elapsedTime < howToWinFadeTime)
        {
            howToWinGroup.alpha = Mathf.Lerp(startA, target, elapsedTime / howToWinFadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        howToWinGroup.alpha = target;
    }
}
