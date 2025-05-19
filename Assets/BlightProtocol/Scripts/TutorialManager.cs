using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;

public enum TutorialProgress
{
    INACTIVE = 0,
    WASD = 1,
    DASH = 2,
    SHOOTROCKET = 3,
    EXPLODROCKET = 4,
    RETRACTROCKET = 5,
    PERSPECTIVESWITCHTOFPV = 6,
    SETFIRSTMAPPOINT = 7,
    SETSPEED = 8,
    WAITFORARRIVEATFIRSTPOINT = 9,
    USEFIRSTTIMEPULSE = 10,
    DRIVETORESOURCEPOINT = 11,
    HARVEST = 12,
    PERSPECTIVESWITCHTODRONE = 13,
    FIGHT = 14,
    CONFIGUPGRADE = 15,
    DONE = 16
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    public TutorialProgress progressState = TutorialProgress.INACTIVE;
    public UnityEvent<TutorialProgress> OnProgressChanged = new UnityEvent<TutorialProgress>();
    public TextMeshProUGUI tutorialText;

    public GameObject droneStartPosition;
    public GameObject HarvesterStartPosition;

    [Header("Segment Doors")]
    [SerializeField] private TutorialDoor MOVEMENT_SEGMENT_EXIT;
    [SerializeField] private TutorialDoor ROCKET_SEGMENT_EXIT;
    [SerializeField] private TutorialDoor NAVIGATION_SEGMENT_EXIT;
    [SerializeField] private TutorialDoor PULSE_SEGMENT_EXIT;
    [SerializeField] private TutorialDoor RESOURCEPOINT_SEGMENT_EXIT;

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
        if(FrankenGameManager.Instance.startWithTutorial)
        {
            PlayerCore.Instance.transform.position = droneStartPosition.transform.position;

            Harvester harvester = Harvester.Instance;
            harvester.gameObject.SetActive(false);
            harvester.transform.position = HarvesterStartPosition.transform.position;
            harvester.gameObject.SetActive(true);

            NextTutorialStep();
        }
        else
        {
            RESOURCEPOINT_SEGMENT_EXIT.CloseSegmentDoor();
        }
    }

    public void NextTutorialStep()
    {
        if (progressState == TutorialProgress.DASH)
        {
            MOVEMENT_SEGMENT_EXIT.OpenSegmentDoor();
        }
        else if (progressState == TutorialProgress.RETRACTROCKET)
        {
            ROCKET_SEGMENT_EXIT.OpenSegmentDoor();
        }
        else if (progressState == TutorialProgress.SETSPEED)
        {
            NAVIGATION_SEGMENT_EXIT.OpenSegmentDoor();
        }
        else if(progressState == TutorialProgress.USEFIRSTTIMEPULSE)
        {
            PULSE_SEGMENT_EXIT.OpenSegmentDoor();
        }


        IncrementProgress();
    }

    private void IncrementProgress()
    {
        int progressInt = (int)progressState + 1;
        TutorialProgress newProgress = (TutorialProgress)progressInt;
        progressState = newProgress;
    }

    public bool IsTutorialOngoing()
    {
        return progressState is not TutorialProgress.INACTIVE and not TutorialProgress.DONE;
    }

}
