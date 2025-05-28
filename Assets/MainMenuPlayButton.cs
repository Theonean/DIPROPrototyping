using UnityEngine;

public class MainMenuPlayButton : MonoBehaviour
{
    [SerializeField] MainMenuHandler mainMenuHandler;
    public void StartGame(bool tutorial)
    {
        FrankenGameManager.Instance.SetStartWithTutorial(tutorial);
        mainMenuHandler.SwitchScene("1_MainScene");
    }
}
