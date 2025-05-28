using UnityEngine;
using UnityEngine.Events;

public class CheckPoint : MonoBehaviour
{
    public int checkPointCost = 1000;
    public static UnityEvent<CheckPoint> onSetSpawnSpawnpoint = new();
    [SerializeField] private EnergySignature energySignature;
    private bool isSpawnPoint = false;

    private void OnEnable()
    {
        onSetSpawnSpawnpoint.AddListener(OnSpawnPointSet);   
    }

    private void OnDisable()
    {
        onSetSpawnSpawnpoint.RemoveListener(OnSpawnPointSet);
    }

    //Called by entitiy detector event on dockingPoint
    public void SetCheckpoint()
    {
        if (isSpawnPoint) return;
        isSpawnPoint = true;

        Vector3 spawnPosition = new Vector3(transform.position.x, 0f, transform.position.z);
        Harvester harvester = Harvester.Instance;
        harvester.respawnPoint = spawnPosition;
        harvester.respawnPointDifficultyRegion = DifficultyManager.Instance.difficultyLevel;
        harvester.health.Fullheal();

        onSetSpawnSpawnpoint.Invoke(this);

        Logger.Log("Set spawn point for Harvester", LogLevel.INFO, LogType.HARVESTER);

        if (TutorialManager.Instance.IsTutorialOngoing())
        {
            TutorialManager.Instance.CompleteDRIVETOCHECKPOINT();
        }
    }

    private void OnSpawnPointSet(CheckPoint checkPoint)
    {
        if(checkPoint == this && isSpawnPoint)
        {
            if(energySignature.displayer) energySignature.displayer.FlashAndSetSignature(Color.green, 0.5f);
        }
        else if(isSpawnPoint)
        {
            energySignature.displayer.FlashSignature(Color.clear, 0.5f);
            isSpawnPoint = false;
        }
    }
}
