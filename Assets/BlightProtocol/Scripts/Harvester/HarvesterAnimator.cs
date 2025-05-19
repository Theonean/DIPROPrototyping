using UnityEngine;

public enum HARVESTER_ANIMATION
{
    Idle,
    Start_Harvesting,
    Harvesting,
    Stop_Harvesting
}

[RequireComponent(typeof(Animator))]
public class HarvesterAnimator : MonoBehaviour
{
    private Animator animator;

    private static readonly int m_StartHarvesting = Animator.StringToHash("Start_Harvesting");
    private static readonly int m_Harvesting = Animator.StringToHash("Harvesting");
    private static readonly int m_StopHarvesting = Animator.StringToHash("Stop_Harvesting");
    private static readonly int m_Idle = Animator.StringToHash("Idle");

    private void Start()
    {
        animator = GetComponent<Animator>();

        Harvester.Instance.changedState.AddListener(SetHarvesterAnimation);
    }

    /// <summary>
    /// Get progress of currently playing animation
    /// </summary>
    /// <returns>Normalized time of current animation progress, 1 = done</returns>
    public float GetCurrentAnimationProgress()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    /// <summary>
    /// Get progress of a specific animation, returns 0 if not playing
    /// </summary>
    /// <param name="animation"></param>
    /// <returns></returns>
    public float GetCurrentAnimationProgress(HARVESTER_ANIMATION animation)
    {
        int targetHash = 0;
        switch (animation)
        {
            case HARVESTER_ANIMATION.Idle:
                targetHash = m_Idle;
                break;
            case HARVESTER_ANIMATION.Start_Harvesting:
                targetHash = m_StartHarvesting;
                break;
            case HARVESTER_ANIMATION.Harvesting:
                targetHash = m_Harvesting;
                break;
            case HARVESTER_ANIMATION.Stop_Harvesting:
                targetHash = m_StopHarvesting;
                break;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.shortNameHash == targetHash)
        {
            return stateInfo.normalizedTime;
        }
        return 0f;
    }


    private void SetHarvesterAnimation(HarvesterState state)
    {
        switch (state)
        {
            case HarvesterState.MOVING:
                animator.Play(m_Idle, 0, 0f);
                break;
            case HarvesterState.START_HARVESTING:
                animator.Play(m_StartHarvesting, 0, 0f);
                break;
            case HarvesterState.HARVESTING:
                animator.Play(m_Harvesting, 0, 0f);
                break;
            case HarvesterState.END_HARVESTING:
                animator.Play(m_StopHarvesting, 0, 0f);
                break;
            case HarvesterState.IDLE:
                animator.Play(m_Idle, 0, 0f);
                break;
            case HarvesterState.DIED:
                animator.Play(m_Idle, 0, 0f);
                break;
        }
    }
}
