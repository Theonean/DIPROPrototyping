using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

public enum TutorialProgress
{
    INACTIVE,
    WASD,
    DASH,
    SHOOTROCKET,
    EXPLODROCKET,
    RETRACTROCKET,
    PERSPECTIVESWITCHTOFPV,
    SETFIRSTMAPPOINT,
    SETSPEED,
    WAITFORARRIVEATFIRSTPOINT,
    USEFIRSTTIMEPULSE,
    SETDESTINATIONTORESOURCEPOINT,
    SETSPEEDRESOURCEPOINT,
    DRIVETORESOURCEPOINT,
    HARVEST,
    SWITCHDIRECTION_A_D,
    PERSPECTIVESWITCHTODRONE,
    FIGHT,
    FINALSWITCHTOFPV,
    SELECTNEWCOMPONENT,
    UPGRADENEWCOMPONENT,
    DRIVETOCHECKPOINT,
    DONE
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public TutorialProgress progressState = TutorialProgress.INACTIVE;
    public UnityEvent<TutorialProgress> OnProgressChanged = new UnityEvent<TutorialProgress>();

    public GameObject TutorialMissionGroup;
    public TextMeshProUGUI tutorialText;
    private List<(TextMeshProUGUI, TutorialProgress)> tutorialChecklist = new List<(TextMeshProUGUI, TutorialProgress)>();

    public GameObject droneStartPosition;
    public GameObject HarvesterStartPosition;

    public SOItem componentDroppedByEnemies;
    public EnemySpawner resourcePointSpawner;
    private ACEnemyMovementBehaviour[] enemies = new ACEnemyMovementBehaviour[5];
    private int deadEnemyCounter = 0;

    public ResourcePoint tutorialResourcePoint;

    [Header("Segment Doors")]
    [SerializeField] private TutorialDoor MOVEMENT_SEGMENT_EXIT;
    [SerializeField] private TutorialDoor ROCKET_SEGMENT_EXIT;
    [SerializeField] private TutorialDoor NAVIGATION_SEGMENT_EXIT;
    [SerializeField] private TutorialDoor PULSE_SEGMENT_EXIT;
    [SerializeField] private TutorialDoor TUTORIAL_EXIT;

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
            StartTutorial();
        }
        else
        {
            TUTORIAL_EXIT.CloseSegmentDoor();
        }
    }

    private void Update()
    {
        if (!IsTutorialOngoing()) 
            return;

        switch(progressState)
        {
            case TutorialProgress.DASH:
                if (DroneMovement.Instance.IsDashing)
                {
                    NextTutorialStep();
                    RocketAimController.Instance.OnRocketShot.AddListener(CompleteSHOOTROCKET);
                }
                break;
        }
    }

    public void StartTutorial()
    {
        PlayerCore.Instance.transform.position = new Vector3(droneStartPosition.transform.position.x, DroneMovement.Instance.distanceFromGround, droneStartPosition.transform.position.z);

        Harvester harvester = Harvester.Instance;
        harvester.gameObject.SetActive(false);
        harvester.transform.position = HarvesterStartPosition.transform.position;
        harvester.gameObject.SetActive(true);

        progressState = TutorialProgress.INACTIVE;
        NextTutorialStep();
    }


    public bool IsTutorialOngoing()
    {
        return progressState is not TutorialProgress.INACTIVE and not TutorialProgress.DONE;
    }
    
    private void CreateNewMissionText(TutorialProgress forTutorialPart)
    {
        TextMeshProUGUI missionText = Instantiate(TutorialMissionGroup.transform.GetChild(0), Vector3.zero, Quaternion.identity, TutorialMissionGroup.transform).GetComponent<TextMeshProUGUI>();

        switch(forTutorialPart)
        {
            case TutorialProgress.WASD:
                missionText.text = "[ ] Move to target position using WASD";
                break;
            case TutorialProgress.DASH:
                missionText.text = "[ ] Dash using spacebar";
                break;
            case TutorialProgress.SHOOTROCKET:
                missionText.text = "[ ] Shoot a rocket using left-click";
                break;
            case TutorialProgress.EXPLODROCKET:
                missionText.text = "[ ] Explode a rocket by clicking it";
                break;
            case TutorialProgress.RETRACTROCKET:
                missionText.text = "[ ] Pull back a rocket using right-clickt";
                break;
            case TutorialProgress.PERSPECTIVESWITCHTOFPV:
                missionText.text = "[ ] Enter Harvester";
                break;
            case TutorialProgress.SETFIRSTMAPPOINT:
                missionText.text = "[ ] Set map waypoint";
                break;
            case TutorialProgress.SETSPEED:
                missionText.text = "[ ] Start driving";
                break;
            case TutorialProgress.WAITFORARRIVEATFIRSTPOINT:
                missionText.text = "[ ] Enjoy the scenery";
                break;
            case TutorialProgress.USEFIRSTTIMEPULSE:
                missionText.text = "[ ] Use pulse button";
                break;
            case TutorialProgress.SETDESTINATIONTORESOURCEPOINT:
                missionText.text = "[ ] Set map waypoint to found resource";
                break;
            case TutorialProgress.SETSPEEDRESOURCEPOINT:
                missionText.text = "[ ] Start driving";
                break;
            case TutorialProgress.DRIVETORESOURCEPOINT:
                missionText.text = "[ ] Enjoy the scenery electric boogaloo V2";
                break;
            case TutorialProgress.HARVEST:
                missionText.text = "[ ] Pull harvest lever";
                break;
            case TutorialProgress.SWITCHDIRECTION_A_D:
                missionText.text = "[ ] switch to drone configurator using 'a' or 'd'";
                break;
            case TutorialProgress.PERSPECTIVESWITCHTODRONE:
                missionText.text = "[ ] Switch back to drone by clicking visors";
                break;
            case TutorialProgress.FIGHT:
                missionText.text = "[ ] Protect harvester from enemies!";
                break;
            case TutorialProgress.FINALSWITCHTOFPV:
                missionText.text = "[ ] Enter Harvester from the back (crazy style)";
                break;
            case TutorialProgress.SELECTNEWCOMPONENT:
                missionText.text = "[ ] Select your newly unlocked component";
                break;
            case TutorialProgress.UPGRADENEWCOMPONENT:
                missionText.text = "[ ] upgrade your newly unlocked component";
                break;
            case TutorialProgress.DRIVETOCHECKPOINT:
                missionText.text = "[ ] Drive to checkpoint to finish tutorial";
                break;
            default:
                missionText.text = "Upsie daisy, this state is not implemented yet" + forTutorialPart.ToString();
                break;
        }

        tutorialChecklist.Add((missionText, forTutorialPart));
    }

    private void CompleteMissionText(TutorialProgress forTutorialPart)
    {
        var missionText = tutorialChecklist.Where(item => item.Item2 == forTutorialPart).FirstOrDefault();
        missionText.Item1.text = missionText.Item1.text.Replace("[ ]", "[X]");
        missionText.Item1.fontStyle = FontStyles.Strikethrough;
    }

    private void EmptyMissionChecklist()
    {
        foreach ((TextMeshProUGUI, TutorialProgress) missionItem in tutorialChecklist)
        {
            Destroy(missionItem.Item1.gameObject);
        }
        tutorialChecklist.Clear();
    }

    #region Tutorial Progress Functions

    public void NextTutorialStep()
    {
        if (IsTutorialOngoing())
            CompleteMissionText(progressState);

        if (progressState == TutorialProgress.DASH)
        {
            MOVEMENT_SEGMENT_EXIT.OpenSegmentDoor();
        }
        else if (progressState == TutorialProgress.RETRACTROCKET)
        {
            MOVEMENT_SEGMENT_EXIT.CloseSegmentDoor();
            ROCKET_SEGMENT_EXIT.OpenSegmentDoor();
        }
        else if (progressState == TutorialProgress.SETSPEED)
        {
            ROCKET_SEGMENT_EXIT.CloseSegmentDoor();
            NAVIGATION_SEGMENT_EXIT.OpenSegmentDoor();
        }
        else if (progressState == TutorialProgress.USEFIRSTTIMEPULSE)
        {
            NAVIGATION_SEGMENT_EXIT.CloseSegmentDoor();
            PULSE_SEGMENT_EXIT.OpenSegmentDoor();
        }
        else if (progressState == TutorialProgress.UPGRADENEWCOMPONENT)
        {
            PULSE_SEGMENT_EXIT.CloseSegmentDoor();
            TUTORIAL_EXIT.OpenSegmentDoor();
        }
        else if(progressState == TutorialProgress.DRIVETOCHECKPOINT)
        {
            TUTORIAL_EXIT.CloseSegmentDoor();
        }

        IncrementProgress();
    }

    private void IncrementProgress()
    {
        EmptyMissionChecklist();

        int progressInt = (int)progressState + 1;
        TutorialProgress newProgress = (TutorialProgress)progressInt;
        progressState = newProgress;

        if (progressState == TutorialProgress.DONE) 
        {
            tutorialText.enabled = false;
        }
        else
        {
            CreateNewMissionText(progressState);
        }
    }
    public void CompletedWASD()
    {
        if (progressState != TutorialProgress.WASD)
            return;

        NextTutorialStep();
    }

    public void CompleteSHOOTROCKET()
    {
        if (progressState != TutorialProgress.SHOOTROCKET) 
            return;

        RocketAimController.Instance.OnRocketShot.RemoveListener(CompleteSHOOTROCKET);
        NextTutorialStep();
        RocketAimController.Instance.OnRocketExplode.AddListener(CompleteEXPLODEROCKET);
    }

    public void CompleteEXPLODEROCKET()
    {
        if (progressState != TutorialProgress.EXPLODROCKET)
            return;

        RocketAimController.Instance.OnRocketShot.RemoveListener(CompleteEXPLODEROCKET);
        NextTutorialStep();
        RocketAimController.Instance.OnRocketRetract.AddListener(CompleteRETRACTROCKET);
    }

    public void CompleteRETRACTROCKET()
    {
        if (progressState != TutorialProgress.RETRACTROCKET)
            return;

        RocketAimController.Instance.OnRocketRetract.RemoveListener(CompleteRETRACTROCKET);
        NextTutorialStep();
    }
    public void CompleteFIRSTSWITCHTOFPV()
    {
        if (progressState != TutorialProgress.PERSPECTIVESWITCHTOFPV)
            return;

        NextTutorialStep();
    }
    public void CompleteSETFIRSTMAPPOINT()
    {
        if (progressState != TutorialProgress.SETFIRSTMAPPOINT)
            return;

        NextTutorialStep();
        Harvester.Instance.changedState.AddListener(CompleteSETSPEED);
    }
    public void CompleteSETSPEED(HarvesterState harvesterState)
    {
        if (progressState != TutorialProgress.SETSPEED && harvesterState != HarvesterState.MOVING)
            return;

        Harvester.Instance.changedState.RemoveListener(CompleteSETSPEED);
        NextTutorialStep();
        Harvester.Instance.changedState.AddListener(CompleteWAITFORARRIVEATFIRSTPOINT);
    }
    public void CompleteWAITFORARRIVEATFIRSTPOINT(HarvesterState harvesterState)
    {
        if (progressState != TutorialProgress.DRIVETORESOURCEPOINT && harvesterState != HarvesterState.IDLE)
            return;

        Harvester.Instance.changedState.RemoveListener(CompleteWAITFORARRIVEATFIRSTPOINT);
        NextTutorialStep();
        ItemManager.Instance.AddCrystal(5);
    }

    public void CompleteUSEFIRSTTIMEPULSE()
    {
        if (progressState != TutorialProgress.USEFIRSTTIMEPULSE)
            return;

        NextTutorialStep();
    }
    public void CompleteSETDESTINATIONTORESOURCEPOINT()
    {
        if (progressState != TutorialProgress.SETDESTINATIONTORESOURCEPOINT)
            return;

        NextTutorialStep();
        Harvester.Instance.changedState.AddListener(CompleteSETSPEEDRESOURCEPOINT);
    }

    public void CompleteSETSPEEDRESOURCEPOINT(HarvesterState harvesterState)
    {
        if (progressState != TutorialProgress.SETSPEEDRESOURCEPOINT && harvesterState != HarvesterState.MOVING)
            return;

        Harvester.Instance.changedState.RemoveListener(CompleteSETSPEEDRESOURCEPOINT);
        NextTutorialStep();
        Harvester.Instance.changedState.AddListener(CompleteDRIVETORESOURCEPOINT);
    }
    public void CompleteDRIVETORESOURCEPOINT(HarvesterState harvesterState)
    {
        if (progressState != TutorialProgress.DRIVETORESOURCEPOINT && harvesterState != HarvesterState.IDLE)
            return;

        Harvester.Instance.changedState.RemoveListener(CompleteDRIVETORESOURCEPOINT);
        NextTutorialStep();
        Harvester.Instance.changedState.AddListener(CompleteHARVEST);

        tutorialResourcePoint.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }
    public void CompleteHARVEST(HarvesterState harvesterState)
    {
        if (progressState != TutorialProgress.HARVEST || harvesterState != HarvesterState.HARVESTING)
            return;

        Harvester.Instance.changedState.RemoveListener(CompleteHARVEST);
        NextTutorialStep();

        resourcePointSpawner.SpawnEnemy();
        enemies = resourcePointSpawner.GetComponentsInChildren<ACEnemyMovementBehaviour>();
        foreach (ACEnemyMovementBehaviour enemy in enemies)
        {
            enemy.StopMovement();
            ItemDropper itemDropper = enemy.GetComponent<ItemDropper>();
            itemDropper.itemToSpawn = componentDroppedByEnemies;
            itemDropper.numberOfItems = 1;
            itemDropper.spawnSingle = true;

            GetComponentInChildren<EnemyDamageHandler>().enemyDestroyed.AddListener(() => 
            { 
                deadEnemyCounter++; 
                if(deadEnemyCounter == enemies.Count()) CompleteFIGHT();
            });
        }
    }

    public void CompleteSWITCHDIRECTION_A_D()
    {
        if (progressState != TutorialProgress.SWITCHDIRECTION_A_D)
            return;

        NextTutorialStep();
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(CompletePERSPECTIVESWITCHTODRONE);
    }
    public void CompletePERSPECTIVESWITCHTODRONE()
    {
        if (progressState != TutorialProgress.PERSPECTIVESWITCHTODRONE)
            return;

        PerspectiveSwitcher.Instance.onPerspectiveSwitched.RemoveListener(CompletePERSPECTIVESWITCHTODRONE);
        NextTutorialStep();

        foreach (ACEnemyMovementBehaviour enemy in enemies) enemy.ResumeMovement();
    }
    public void CompleteFIGHT()
    {
        if (progressState != TutorialProgress.FIGHT)
            return;

        NextTutorialStep();
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(CompleteFINALSWITCHTOFPV);
    }

    public void CompleteFINALSWITCHTOFPV()
    {
        if (progressState != TutorialProgress.FINALSWITCHTOFPV)
            return;

        PerspectiveSwitcher.Instance.onPerspectiveSwitched.RemoveListener(CompleteFINALSWITCHTOFPV);
        NextTutorialStep();
        ComponentSelectorManager.Instance.OnComponentSelectionChanged.AddListener(CompleteSELECTNEWCOMPONENT);
    }

    public void CompleteSELECTNEWCOMPONENT()
    {
        if (progressState != TutorialProgress.SELECTNEWCOMPONENT)
            return;

        ComponentSelectorManager.Instance.OnComponentSelectionChanged.RemoveListener(CompleteSELECTNEWCOMPONENT);
        NextTutorialStep();
        ResearchManager.OnResearched.AddListener(CompleteUPGRADENEWCOMPONENT);
    }

    public void CompleteUPGRADENEWCOMPONENT(RocketComponentType componentType)
    {
        if (progressState != TutorialProgress.UPGRADENEWCOMPONENT)
            return;

        ResearchManager.OnResearched.RemoveListener(CompleteUPGRADENEWCOMPONENT);
        NextTutorialStep();
    }

    public void CompleteDRIVETOCHECKPOINT()
    {
        if (progressState != TutorialProgress.DRIVETOCHECKPOINT)
            return;

        NextTutorialStep();
    }

    #endregion

}
