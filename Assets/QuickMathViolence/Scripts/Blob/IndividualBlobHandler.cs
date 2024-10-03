using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IndividualBlobHandler : MonoBehaviour
{
    public Renderer face;
    public Material happy;
    public Material sad;
    

    public BlobInteractable parentInteractable;

    private void Awake()
    {
        SetState(Emotion.sad);
    }

    public Emotion state = Emotion.sad;
    public enum Emotion
    {
        happy,
        sad
    }

    public void SetState(Emotion emotion)
    {
        state = emotion;
        switch(state)
        {
            case Emotion.happy:
                face.material = happy;
                break;

            case Emotion.sad:
                face.material = sad;
                break;
        }
    }
}
