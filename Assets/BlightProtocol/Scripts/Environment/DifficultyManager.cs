using UnityEngine;
using UnityEngine.Events;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;
    public int difficultyLevel = 0;
    public int maximumDifficultyRegions = 4;
    public int maxDifficultyReached { get; private set; } = 0;
    public UnityEvent OnDifficultyLevelChanged = new UnityEvent();

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
        if (difficultyLevel != dL)
        {
            difficultyLevel = dL;
            maxDifficultyReached = difficultyLevel > maxDifficultyReached ? difficultyLevel : maxDifficultyReached;

            OnDifficultyLevelChanged.Invoke();
        }
    }
}
