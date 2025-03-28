public interface IFPVInteractable
{
    bool IsCurrentlyInteractable{ get; }

    public string LookAtText { get; set; }
    public string InteractText { get; set; }
    void OnInteract();

    void OnHover();
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
}