using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FMODAudioManagement : MonoBehaviour
{
    public static FMODAudioManagement instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Plays a FMOD sound and binds it's position to the specified GameObject.
    /// </summary>
    /// <param name="eventInstance">Instance of the sound to be played</param>
    /// <param name="soundPath">Path of the sound in FMOD, eg. event:/...</param>
    /// <param name="objectToTrack">Gameobject to tie this sound on</param>
    public void PlaySound(out EventInstance eventInstance, string soundPath, GameObject objectToTrack)
    {
        eventInstance = RuntimeManager.CreateInstance(soundPath);
        eventInstance.start();
        eventInstance.release(); // Automatically releases once playback ends

        StartCoroutine(UpdateSoundPositionWhileValid(eventInstance, objectToTrack));
    }

    /// <summary>
    /// Plays a FMOD sound at a specific position
    /// </summary>
    /// <param name="eventInstance">Instance of the sound to be played</param>
    /// <param name="soundPath">Path of the sound in FMOD, eg. event:/...</param>
    /// <param name="position">Position to play this sound at</param>
    public void PlaySound(out EventInstance eventInstance, string soundPath, Vector3 position)
    {
        eventInstance = RuntimeManager.CreateInstance(soundPath);
        eventInstance.start();
        eventInstance.release(); // Automatically releases once playback ends
        eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
    }

    /// <summary>
    /// Plays a FMOD sound at a specific position which is not tied to any object without returned reference
    /// </summary>
    /// <param name="soundPath">Path of the sound in FMOD, eg. event:/...</param>
    /// <param name="position">Position to play this sound at</param>
    public void PlayOneShot(string soundPath, Vector3 position)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(soundPath);
        eventInstance.start();
        eventInstance.release(); // Automatically releases once playback ends
        eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
    }

    /// <summary>
    /// Plays a FMOD sound at a specific position which is not tied to any object without returned reference
    /// </summary>
    /// <param name="soundPath">Path of the sound in FMOD, eg. event:/...</param>
    /// <param name="objectToTrack">Object to tie this sound on</param>
    public void PlayOneShot(string soundPath, GameObject objectToTrack)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(soundPath);
        eventInstance.start();
        eventInstance.release(); // Automatically releases once playback ends
        StartCoroutine(UpdateSoundPositionWhileValid(eventInstance, objectToTrack));
    }

    IEnumerator UpdateSoundPositionWhileValid(EventInstance eventInstance, GameObject targetObject)
    {
        while (eventInstance.isValid())
        {
            eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(targetObject.transform.position));
            yield return null;
        }
    }
}
