using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;
    public int difficultyLevel { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void SetDifficultyLevel(int dL)
    {
        difficultyLevel = dL;
    }

    public void PlayerLeftRegion()
    {
        difficultyLevel = Mathf.Max(difficultyLevel - 1, 0);
    }
}
