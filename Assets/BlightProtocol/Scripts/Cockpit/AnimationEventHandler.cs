using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHandler : MonoBehaviour
{
    public UnityEvent animationFinished;

    public void AnimationFinished()
    {
        animationFinished.Invoke();
    }
}
