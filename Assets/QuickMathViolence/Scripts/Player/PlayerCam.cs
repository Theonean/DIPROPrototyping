using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform camHolder;

    float xRotation;
    float yRotation;

    float defaultFov;

    public float fovMin;
    public float fovMax;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        defaultFov = GetComponent<Camera>().fieldOfView;

        DOTween.SetTweensCapacity(500, 50);
    }

    private void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate cam and orientation
        camHolder.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void DoFov(float endValue)
    {
        //GetComponent<Camera>().DOFieldOfView(endValue, 0.2f);
    }

    private float prevValue;
    public void DoDynamicFov(float value, float maxValue)
    {
        float relFov = (value / maxValue) * (fovMax - fovMin);
        float fov = fovMin + relFov;
        float tweenTime = 0.2f;
        if (value == 0)
            tweenTime = 2f;
        GetComponent<Camera>().DOFieldOfView(fov, tweenTime);
        prevValue = value;
    }

    public void ResetFov()
    {
        GetComponent<Camera>().DOFieldOfView(defaultFov, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }

    public void DoHunch(float yValue)
    {
        transform.DOLocalMoveY(yValue, 0.4f);
    }

}
