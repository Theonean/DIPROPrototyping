using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class HarvesterSFX : MonoBehaviour
{
    [Header("SFX")]
    public string harvestingSFXPath = "event:/...";
    private EventInstance m_HarvestingSFX;
    public StudioEventEmitter movingSFX;
    public string enemyImpactSFXPath = "event:/...";
    public string takeoffSFXPath = "event:/...";

    private Harvester harvester;

    private void Awake()
    {
        harvester = Harvester.Instance;
    }

    private void OnEnable()
    {
        harvester.health.tookDamage.AddListener(PlayHarvesterTookDamage);
        harvester.changedState.AddListener(PlayAudioOnHarvesterStateChange);
    }

    private void OnDisable()
    {
        harvester.health.tookDamage.RemoveListener(PlayHarvesterTookDamage);
        harvester.changedState.RemoveListener(PlayAudioOnHarvesterStateChange);
    }

    private void PlayAudioOnHarvesterStateChange(HarvesterState state)
    {
        switch (state)
        {
            case HarvesterState.MOVING:
                movingSFX.EventInstance.setPaused(false);
                break;
            case HarvesterState.START_HARVESTING:
                FMODAudioManagement.instance.PlaySound(out m_HarvestingSFX, harvestingSFXPath, gameObject);
                movingSFX.EventInstance.setPaused(true);
                break;
            case HarvesterState.HARVESTING: break;
            case HarvesterState.END_HARVESTING:
                m_HarvestingSFX.keyOff();
                break;
            case HarvesterState.IDLE: break;
            case HarvesterState.DIED:
                FMODAudioManagement.instance.PlayOneShot(takeoffSFXPath, transform.position);
                break;

        }
    }

    private void PlayHarvesterTookDamage(GameObject enemy)
    {
        FMODAudioManagement.instance.PlayOneShot(enemyImpactSFXPath, enemy.transform.position);
    }

}
