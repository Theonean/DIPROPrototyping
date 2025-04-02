using UnityEngine;

public class SpeedSlider : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable { get; set; } = true;
    public string lookAtText = "E";
    public string interactText = "";
    public bool UpdateHover { get; set; } = false;
    public bool UpdateInteract { get; set; } = true;
    [SerializeField] private Transform _touchPoint;

    public Transform TouchPoint
    {
        get => _touchPoint;
        set => _touchPoint = value;
    }

    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    public string InteractText
    {
        get => interactText;
        set => interactText = value;
    }

    public Transform sliderHead;
    public float sliderRange = 200f;

    private int speedStepCount;
    private float minY, maxY;

    void Start()
    {
        speedStepCount = HarvesterSpeedControl.Instance.GetSpeedStepCount();
        minY = -sliderRange * 0.5f;
        maxY = sliderRange * 0.5f;
    }

    public void OnStartInteract() {
        Cursor.visible = false;
    }
    public void OnUpdateInteract()
    {
        DragSlider();
    }
    public void OnEndInteract() {
        Cursor.visible = true;
    }

    public void OnHover()
    {
        this.DefaultOnHover();
    }

    private void DragSlider()
    {
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * 5f;
        float newY = Mathf.Clamp(sliderHead.localPosition.y + mouseY, minY, maxY);
        sliderHead.localPosition = new Vector3(sliderHead.localPosition.x, newY, sliderHead.localPosition.z);

        float percentage = Mathf.InverseLerp(minY, maxY, newY);
        int newIndex = Mathf.RoundToInt(percentage * (speedStepCount - 1));

        HarvesterSpeedControl.Instance.SetSpeedStepIndex(newIndex);
    }
}
