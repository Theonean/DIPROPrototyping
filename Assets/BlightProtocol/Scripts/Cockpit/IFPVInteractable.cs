using UnityEngine;

public interface IFPVInteractable
{
    bool IsCurrentlyInteractable{ get; }

    public string LookAtText { get; set; }
    public string InteractText { get; set; }
    void OnStartInteract();
    void OnUpdateInteract();
    void OnEndInteract();
    void OnHover();
    public bool UpdateHover { get; set; }
    public bool UpdateInteract { get; set; }
    public Transform TouchPoint {get; set; }
}

public static class FPVInteractableExtensions
{
    public static void DefaultOnHover(this IFPVInteractable interactable)
    {
        if (!string.IsNullOrEmpty(interactable.LookAtText))
        {
            FPVUI.Instance.SetLookAtText(interactable.LookAtText);
        }
    }

    public static void DefaultOnInteract(this IFPVInteractable interactable)
    {
        if (!string.IsNullOrEmpty(interactable.InteractText)) {
            FPVUI.Instance.SetInteractText(interactable.InteractText);
        }
    }

    public static Vector2[] GetScreenSpaceBounds(this IFPVInteractable interactable, Vector3 worldPosMin, Vector3 worldPosMax, Camera camera) {
        Vector3 screenMin = camera.WorldToScreenPoint(worldPosMin);
        Vector3 screenMax = camera.WorldToScreenPoint(worldPosMax);
        return new Vector2[2] {screenMin, screenMax};
    }

    public static float GetMouseProgressOnSlider(this IFPVInteractable interactable, Vector2 screenSpaceMin, Vector2 screenSpaceMax, Vector2 MousePos) {
        // Direction vector of the bounds (A)
        Vector2 boundsVector = screenSpaceMax - screenSpaceMin;

        // Vector from min to MousePos (AP)
        Vector2 AP = MousePos - screenSpaceMin;

        // Projection of AP onto A: t = (AP • A) / (A • A)
        float t = Vector2.Dot(AP, boundsVector) / Vector2.Dot(boundsVector, boundsVector);

        // Clamp t to ensure C stays within the bounds segment
        t = Mathf.Clamp01(t);

        // Compute C as min + t * boundsVector
        Vector2 C = screenSpaceMin + t * boundsVector;

        return t;
    }
}