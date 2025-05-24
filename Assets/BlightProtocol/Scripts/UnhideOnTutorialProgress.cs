using UnityEngine;

public class TutorialInformationDisplay : MonoBehaviour
{
    public TutorialProgress unhideOnTutorialReached;

    private void Start()
    {
        TutorialManager.Instance.OnProgressChanged.AddListener(TryUnHide);
        gameObject.SetActive(false);
        
    }

    private void TryUnHide(TutorialProgress tutorialProgress)
    {
        if(tutorialProgress == unhideOnTutorialReached)
        {
            gameObject.SetActive(true);
        }
    }
}
