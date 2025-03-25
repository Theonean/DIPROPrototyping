using UnityEngine;

public class FPVMoveCam : MonoBehaviour
{
    public Transform cameraPosition;

    // Update is called once per frame
    void Update()
    {
        if (!FPVPlayerCam.Instance.isLocked)
        {
            transform.position = cameraPosition.position;
        }

    }
}
