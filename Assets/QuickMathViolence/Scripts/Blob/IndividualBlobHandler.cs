using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IndividualBlobHandler : MonoBehaviour
{
    public TextMeshPro faceText;
    public string faceHappy;
    public string faceSad;
    public Color colorHappy = Color.green;
    public Color colorSad = Color.gray;
    private Renderer rend;

    public BlobInteractable parentInteractable;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
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
                faceText.text = faceHappy;
                rend.material.SetColor("_BaseColor", colorHappy);
                break;

            case Emotion.sad:
                faceText.text = faceSad;
                rend.material.SetColor("_BaseColor", colorSad);
                break;
        }
    }
}
