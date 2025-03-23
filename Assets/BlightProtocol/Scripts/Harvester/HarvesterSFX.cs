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

    private ControlZoneManager harvester;

    private void Awake()
    {
        harvester = ControlZoneManager.Instance;
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

    private void PlayAudioOnHarvesterStateChange(ZoneState state)
    {
        switch (state)
        {
            case ZoneState.MOVING:
                movingSFX.EventInstance.setPaused(false);
                break;
            case ZoneState.START_HARVESTING:
                FMODAudioManagement.instance.PlaySound(out m_HarvestingSFX, harvestingSFXPath, gameObject);
                movingSFX.EventInstance.setPaused(true);
                break;
            case ZoneState.HARVESTING: break;
            case ZoneState.END_HARVESTING:
                m_HarvestingSFX.keyOff();
                break;
            case ZoneState.IDLE: break;
            case ZoneState.DIED:
                FMODAudioManagement.instance.PlayOneShot(takeoffSFXPath, transform.position);
                break;

        }
    }

    private void PlayHarvesterTookDamage(GameObject enemy)
    {
        FMODAudioManagement.instance.PlayOneShot(enemyImpactSFXPath, enemy.transform.position);
    }

}
