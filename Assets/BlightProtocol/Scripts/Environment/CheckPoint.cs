using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    //Called by entitiy detector event on dockingPoint
    public void SetCheckpoint()
    {
        Vector3 spawnPosition = new Vector3(transform.position.x, 0f, transform.position.z);
        Harvester harvester = Harvester.Instance;
        harvester.respawnPoint = transform.position;
        harvester.respawnPointDifficultyRegion = DifficultyManager.Instance.difficultyLevel;
        harvester.health.Fullheal();
        
        Logger.Log("Set spawn point for Harvester", LogLevel.INFO, LogType.HARVESTER);
    }  
    
}
