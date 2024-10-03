using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AudioAction
{
    public string name;
    public List<AudioClip> clips;
    public float volume;
}

public class BlobAudioHandler : MonoBehaviour
{
    private AudioSource audioSource;
    public List<AudioAction> actions;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudioAction(string actionName)
    {
        AudioAction actionToPlay = actions.Find(action => action.name == actionName);
        int index = Random.Range(0, actionToPlay.clips.Count - 1);
        AudioClip clipToPlay = actionToPlay.clips[index];

        if (actionToPlay != null && clipToPlay != null)
        {
            //audioSource.pitch = 1 + Random.Range(-0.2f, 0.2f);
            audioSource.PlayOneShot(clipToPlay, actionToPlay.volume);
        }
    }
}
