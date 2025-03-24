using UnityEngine;

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

    private void SetHarvesterAnimation(ZoneState state)
    {
        switch (state)
        {
            case ZoneState.MOVING:
                animator.Play(m_Idle, 0, 0f);
                break;
            case ZoneState.START_HARVESTING:
                animator.Play(m_StartHarvesting, 0, 0f);
                break;
            case ZoneState.HARVESTING:
                animator.Play(m_Harvesting, 0, 0f);
                break;
            case ZoneState.END_HARVESTING:
                animator.Play(m_StopHarvesting, 0, 0f);
                break;
            case ZoneState.IDLE:
                animator.Play(m_Idle, 0, 0f);
                break;
            case ZoneState.DIED:
                animator.Play(m_Idle, 0, 0f);
                break;
        }
    }
}
