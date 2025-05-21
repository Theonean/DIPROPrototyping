using UnityEngine;

[System.Serializable]
public enum LookState
{
    IDLE,
    INTERACTING,
    LOOKING
}

public class FPVInputManager : MonoBehaviour
{
    public static FPVInputManager Instance { get; private set; }
    private FPVPlayerCam fpvPlayerCam;
    public FPVCamRotator fpvCamRotator { get; private set; }
    [SerializeField] private FPVInteractionHandler interactionHandler;

    public LookState lookState = LookState.IDLE;
    public bool isActive = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        fpvPlayerCam = FPVPlayerCam.Instance;
        fpvCamRotator = GetComponentInChildren<FPVCamRotator>();
        //interactionHandler = GetComponentInChildren<FPVInteractionHandler>();
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(HandlePerspectiveSwitch);
    }

    private void OnDestroy()
    {
        if (PerspectiveSwitcher.Instance != null)
        {
            PerspectiveSwitcher.Instance.onPerspectiveSwitched.RemoveListener(HandlePerspectiveSwitch);
        }
    }

    private void HandlePerspectiveSwitch()
    {
        bool isFPV = PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.FPV;
        isActive = isFPV;
        SetLookState(LookState.IDLE);
    }

    public void SetLookState(LookState state)
    {
        lookState = state;
    }

    private void Update()
    {
        if (!isActive) return;

        switch (lookState)
        {
            case LookState.IDLE:
                if (Input.GetMouseButtonDown(0))
                {
                    if (interactionHandler.HoveredInteractable != null)
                    {
                        SetLookState(LookState.INTERACTING);
                        interactionHandler.StartInteraction();
                    }
                    else
                    {
                        SetLookState(LookState.LOOKING);
                        fpvPlayerCam.StartLook(GetNormalisedMousePos());
                    }
                }
                break;

            case LookState.LOOKING:
                if (Input.GetMouseButton(0))
                {
                    fpvPlayerCam.UpdateCameraRotation(GetNormalisedMousePos());
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    SetLookState(LookState.IDLE);
                }
                break;

            case LookState.INTERACTING:
                if (Input.GetMouseButtonUp(0))
                {
                    SetLookState(LookState.IDLE);
                    interactionHandler.EndInteraction();
                }
                return;
        }

        if (Input.GetKeyDown(KeyCode.A)) fpvCamRotator.ChangePosition(-1);
        if (Input.GetKeyDown(KeyCode.D)) fpvCamRotator.ChangePosition(1);
    }

    public Vector2 GetNormalisedMousePos()
    {
        return new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
        );
    }
}