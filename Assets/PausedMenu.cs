using UnityEngine;
using UnityEngine.SceneManagement;

public class PausedMenu : MonoBehaviour
{
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("0_MainMenu");
    }
}
