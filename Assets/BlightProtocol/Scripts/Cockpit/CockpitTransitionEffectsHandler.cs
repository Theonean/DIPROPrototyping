using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CockpitTransitionEffectsHandler : MonoBehaviour
{
    [SerializeField] private CameraShake cameraShake;

    void Start()
    {
        EnterTransition(5f);
    }

    public void EnterTransition(float magnitude) {
        // add other effects here
        cameraShake.StartShake(magnitude);
    }

    public void ExitTransition() {
        cameraShake.StopShake();
    }
}
