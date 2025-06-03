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
    RETRACTROCKET,
    SHOOTROCKET2,
    SHOOTROCKET3,
    EXPLODROCKET,
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
    COLLECTCRYSTALS,
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
    public GameObject tutorialTextTemplate;
    private TextMeshProUGUI currentTutorialText;

    public GameObject droneStartPosition;
    public GameObject HarvesterStartPosition;

    public SOItem componentDroppedByEnemies;
    public EnemySpawner resourcePointSpawner;
    private int deadEnemyCounter = 0;

    public ResourcePoint tutorialResourcePoint;
    public int crystalsToCollectForUpgrade = 20;
    private int crystalsCollected = 0;

    public GameObject minimap;

    [Header("Segment Doors")]
    [SerializeField] private TutorialDoor MOVEMENT_SEGMENT_EXIT;
    [SerializeField] private TutorialDoor ROCKET_SEGMENT_EXIT;
    [SerializeField] private TutorialDoor NAVIGATION_SEGMENT_EXIT;
    [SerializeField] private TutorialDoor PULSE_SEGMENT_EXIT;
    [SerializeField] private TutorialDoor TUTORIAL_EXIT;

    [Header("Target Positions")]
    [SerializeField] private GameObject WASDTargetPosition;
    [SerializeField] private GameObject SetMapTargetPosition;
    [SerializeField] private GameObject ResourcePointTargetPosition;
    [SerializeField] private GameObject CheckPointTargetPosition;

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
        if (FrankenGameManager.Instance.startWithTutorial)
        {
            StartTutorial();
        }
        else
        {
            TUTORIAL_EXIT.CloseSegmentDoor();
            TutorialMissionGroup.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsTutorialOngoing()) 
            return;

        if (!_isTransitioning &&
            progressState == TutorialProgress.DASH &&
            DroneMovement.Instance.IsDashing)
        {
            NextTutorialStep();
        }

    }

    public void StartTutorial()
    {
        WASDTargetPosition.SetActive(true);
        SetMapTargetPosition.SetActive(false);
        ResourcePointTargetPosition.SetActive(false);
        CheckPointTargetPosition.SetActive(false);

        tutorialText.text = "MOVEMENT";

        PlayerCore.Instance.transform.position = new Vector3(droneStartPosition.transform.position.x, DroneMovement.Instance.distanceFromGround, droneStartPosition.transform.position.z);

        Harvester harvester = Harvester.Instance;
        harvester.gameObject.SetActive(false);
        harvester.transform.position = HarvesterStartPosition.transform.position;
        harvester.gameObject.SetActive(true);

        progressState = TutorialProgress.INACTIVE;

        minimap.SetActive(false);

        NextTutorialStep();
    }


    public bool IsTutorialOngoing()
    {
        return progressState is not TutorialProgress.INACTIVE and not TutorialProgress.DONE;
    }
    
    private void CreateNewcurrentTutorialText(TutorialProgress forTutorialPart)
    {
        if (currentTutorialText == null)
            currentTutorialText = Instantiate(tutorialTextTemplate, Vector3.zero, Quaternion.identity, TutorialMissionGroup.transform).GetComponent<TextMeshProUGUI>();

        switch(forTutorialPart)
        {
            case TutorialProgress.WASD:
                currentTutorialText.text = "[ ] Move to target position using WASD";
                break;
            case TutorialProgress.DASH:
                currentTutorialText.text = "[ ] Dash by moving and pressing spacebar";
                break;
            case TutorialProgress.SHOOTROCKET:
                currentTutorialText.text = "[ ] Shoot a rocket to target 1 using left-click";
                break;
            case TutorialProgress.RETRACTROCKET:
                currentTutorialText.text = "[ ] Pull back a rocket using right-click";
                break;
            case TutorialProgress.SHOOTROCKET2:
                currentTutorialText.text = "[ ] Shoot a rocket to target 2 using left-click";
                break;
            case TutorialProgress.SHOOTROCKET3:
                currentTutorialText.text = "[ ] Shoot a rocket to target 3 using left-click";
                break;
            case TutorialProgress.EXPLODROCKET:
                currentTutorialText.text = "[ ] Explode a rocket by clicking it";
                break;
            case TutorialProgress.PERSPECTIVESWITCHTOFPV:
                currentTutorialText.text = "[ ] Move into Exit zone";
                break;
            case TutorialProgress.SETFIRSTMAPPOINT:
                currentTutorialText.text = "[ ] Set a waypoint by clicking on the map";
                break;
            case TutorialProgress.SETSPEED:
                currentTutorialText.text = "[ ] Start driving by increasing the throttle";
                break;
            case TutorialProgress.WAITFORARRIVEATFIRSTPOINT:
                currentTutorialText.text = "[ ] Enjoy the scenery";
                break;
            case TutorialProgress.USEFIRSTTIMEPULSE:
                currentTutorialText.text = "[ ] Use radar button to reveal closeby gas deposits";
                break;
            case TutorialProgress.SETDESTINATIONTORESOURCEPOINT:
                currentTutorialText.text = "[ ] Set map waypoint to gas deposit by clicking on the symbol on the map";
                break;
            case TutorialProgress.SETSPEEDRESOURCEPOINT:
                currentTutorialText.text = "[ ] Start driving";
                break;
            case TutorialProgress.DRIVETORESOURCEPOINT:
                currentTutorialText.text = "[ ] Enjoy the scenery electric boogaloo V2";
                break;
            case TutorialProgress.HARVEST:
                currentTutorialText.text = "[ ] Pull down harvest lever";
                break;
            case TutorialProgress.SWITCHDIRECTION_A_D:
                currentTutorialText.text = "[ ] switch to drone configurator using 'a' or 'd'";
                break;
            case TutorialProgress.PERSPECTIVESWITCHTODRONE:
                currentTutorialText.text = "[ ] Switch back to drone by clicking visors at bottom of screen";
                break;
            case TutorialProgress.FIGHT:
                currentTutorialText.text = "[ ] Protect harvester from enemies!";
                break;
            case TutorialProgress.COLLECTCRYSTALS:
                currentTutorialText.text = "[ ] Shoot crystal structures to collect some: " + crystalsCollected + "/" + crystalsToCollectForUpgrade;
                break;
            case TutorialProgress.FINALSWITCHTOFPV:
                currentTutorialText.text = "[ ] Enter Harvester from the back";
                break;
            case TutorialProgress.SELECTNEWCOMPONENT:
                currentTutorialText.text = "[ ] Interact with the drone configurator";
                break;
            case TutorialProgress.UPGRADENEWCOMPONENT:
                currentTutorialText.text = "[ ] upgrade your direct line propulsion";
                break;
            case TutorialProgress.DRIVETOCHECKPOINT:
                currentTutorialText.text = "[ ] Drive to the white balloon (checkpoint) to complete the tutorial";
                break;
            default:
                currentTutorialText.text = "Upsie daisy, this state is not implemented yet" + forTutorialPart.ToString();
                break;
        }
    }

    #region Tutorial Progress Functions
    private bool _isTransitioning = false;

    public void NextTutorialStep()
    {
        // nothing else will fire until the current animation is done
        if (_isTransitioning)
            return;

        StartCoroutine(TransitionTutorialStep());
    }

    private IEnumerator TransitionTutorialStep()
    {
        _isTransitioning = true;
        float duration = 0.3f;
        float t = 0f;

        // prepare an ease‐in‐out Bezier curve (0,0 → 1,1)
        var ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // 1. mark & strike the old text, then scale it down
        if (currentTutorialText != null)
        {
            var oldRT = currentTutorialText.rectTransform;
            currentTutorialText.text = currentTutorialText.text.Replace("[ ]", "[X]");
            currentTutorialText.fontStyle |= FontStyles.Strikethrough;

            t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float pct = Mathf.Clamp01(t / duration);
                float eval = ease.Evaluate(pct);
                oldRT.localScale = Vector3.one * (1f - eval);
                yield return null;
            }

            // remove the old text object
            Destroy(oldRT.gameObject);
            currentTutorialText = null;
        }

        // 2. segment‐door & target‐marker logic based on the *old* state
        switch (progressState)
        {
            case TutorialProgress.DASH:
                MOVEMENT_SEGMENT_EXIT.OpenSegmentDoor();
                WASDTargetPosition.SetActive(false);
                tutorialText.text = "COMBAT";
                break;
            case TutorialProgress.RETRACTROCKET:
                MOVEMENT_SEGMENT_EXIT.CloseSegmentDoor();
                ROCKET_SEGMENT_EXIT.OpenSegmentDoor();
                SetMapTargetPosition.SetActive(true);
                tutorialText.text = "NAVIGATION";
                break;
            case TutorialProgress.SETSPEED:
                ROCKET_SEGMENT_EXIT.CloseSegmentDoor();
                NAVIGATION_SEGMENT_EXIT.OpenSegmentDoor();
                tutorialText.text = "RESOURCES";
                break;
            case TutorialProgress.USEFIRSTTIMEPULSE:
                NAVIGATION_SEGMENT_EXIT.CloseSegmentDoor();
                PULSE_SEGMENT_EXIT.OpenSegmentDoor();
                SetMapTargetPosition.SetActive(false);
                ResourcePointTargetPosition.SetActive(true);
                break;
            case TutorialProgress.FIGHT:
                tutorialText.text = "CONFIGURATION";
                break;
            case TutorialProgress.UPGRADENEWCOMPONENT:
                PULSE_SEGMENT_EXIT.CloseSegmentDoor();
                TUTORIAL_EXIT.OpenSegmentDoor();
                ResourcePointTargetPosition.SetActive(false);
                CheckPointTargetPosition.SetActive(true);
                tutorialText.text = "SPAWNPOINT";
                break;
            case TutorialProgress.DRIVETOCHECKPOINT:
                TUTORIAL_EXIT.CloseSegmentDoor();
                break;
        }

        // 3. advance state & instantiate the new line
        currentTutorialText = null;
        IncrementProgress(); // calls CreateNewcurrentTutorialText and sets fontStyle = Normal

        // 4. scale the new text up from zero
        var newRT = currentTutorialText.rectTransform;
        newRT.localScale = Vector3.zero;

        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float pct = Mathf.Clamp01(t / duration);
            float eval = ease.Evaluate(pct);
            newRT.localScale = Vector3.one * eval;
            yield return null;
        }

        _isTransitioning = false;
    }


    private void IncrementProgress()
    {
        int progressInt = (int)progressState + 1;
        TutorialProgress newProgress = (TutorialProgress)progressInt;
        progressState = newProgress;
        OnProgressChanged?.Invoke(progressState);

        if (progressState == TutorialProgress.DONE) 
        {
            tutorialText.enabled = false;
        }
        else
        {
            CreateNewcurrentTutorialText(progressState);
        }
    }

    public void CompletedWASD()
    {
        if (progressState != TutorialProgress.WASD)
            return;

        NextTutorialStep();
    }

    public void CompleteSHOOTROCKET_ONE()
    {
        if (progressState != TutorialProgress.SHOOTROCKET)
            return;

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

    public void CompleteSHOOTROCKET_TWO()
    {
        if (progressState != TutorialProgress.SHOOTROCKET2)
            return;

        NextTutorialStep();
    }
    public void CompleteSHOOTROCKET_THREE()
    {
        if (progressState != TutorialProgress.SHOOTROCKET3)
            return;

        NextTutorialStep();
        RocketAimController.Instance.OnRocketExplode.AddListener(CompleteEXPLODEROCKET);
    }

    public void CompleteEXPLODEROCKET()
    {
        if (progressState != TutorialProgress.EXPLODROCKET)
            return;

        RocketAimController.Instance.OnRocketShot.RemoveListener(CompleteEXPLODEROCKET);
        NextTutorialStep();
    }
    public void CompleteFIRSTSWITCHTOFPV()
    {
        if (progressState != TutorialProgress.PERSPECTIVESWITCHTOFPV)
            return;

        NextTutorialStep();
        minimap.SetActive(true);
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
    }
    public void CompleteHARVEST(HarvesterState harvesterState)
    {
        if (progressState != TutorialProgress.HARVEST || harvesterState != HarvesterState.HARVESTING)
            return;

        Harvester.Instance.changedState.RemoveListener(CompleteHARVEST);
        NextTutorialStep();

        resourcePointSpawner.SpawnEnemy();
        foreach (ACEnemyMovementBehaviour enemy in resourcePointSpawner.spawnedEnemies)
        {
            enemy.StopMovement();
            ItemDropper itemDropper = enemy.GetComponent<ItemDropper>();
            itemDropper.itemToSpawn = componentDroppedByEnemies;
            itemDropper.numberOfItems = 1;
            itemDropper.spawnSingle = true;

            enemy.GetComponentInChildren<EnemyDamageHandler>().enemyDestroyed.AddListener(() => 
            { 
                deadEnemyCounter++; 
                if(deadEnemyCounter == resourcePointSpawner.spawnedEnemies.Count()) CompleteFIGHT();
            });
        }
        
        tutorialResourcePoint.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
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

        foreach (ACEnemyMovementBehaviour enemy in resourcePointSpawner.spawnedEnemies) enemy.ResumeMovement();

        if (deadEnemyCounter == resourcePointSpawner.spawnedEnemies.Count()) CompleteFIGHT();
    }
    public void CompleteFIGHT()
    {
        crystalsCollected = ItemManager.Instance.GetCrystal();
        NextTutorialStep();
        ItemManager.Instance.crystalAmountChanged.AddListener(CompleteCOLLECTCRYSTALS);
    }
    public void CompleteCOLLECTCRYSTALS(int amount)
    {
        if (progressState != TutorialProgress.COLLECTCRYSTALS)
            return;

        crystalsCollected += amount;

        currentTutorialText.text = "[ ] Shoot crystal structures to collect some: " + crystalsCollected + "/" + crystalsToCollectForUpgrade;


        if (crystalsCollected >= crystalsToCollectForUpgrade)
        {
            ItemManager.Instance.crystalAmountChanged.RemoveListener(CompleteCOLLECTCRYSTALS);
            NextTutorialStep();
            PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(CompleteFINALSWITCHTOFPV);
        }
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
        ItemManager.Instance.AddCrystal(5);
        NextTutorialStep();
    }

    public void CompleteDRIVETOCHECKPOINT()
    {
        if (progressState != TutorialProgress.DRIVETOCHECKPOINT)
            return;

        UIHandler.Instance.ShowHowToWin(true);
        NextTutorialStep();
    }

    #endregion

}
