using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class FlyingDotController : MonoBehaviour
{
    // Singleton-like instance
    public static FlyingDotController Instance { get; private set; }

    [SerializeField]
    private Sprite dotSprite; // Set this directly in the Inspector on the instance in the scene
    [SerializeField]
    private AnimationCurve curve; // Set this directly in the Inspector on the instance in the scene
    [SerializeField]
    private float size = 10f; // Set this directly in the Inspector on the instance in the scene
    [SerializeField]
    private Canvas canvas;


    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of FlyingDotController found! Destroying extra instances.");
            Destroy(gameObject);
        }
    }

    // Static method to create the flying dot
    public static void CreateFlyingDot(Vector3 worldPosition, Vector3 targetScreenPosition, CollectibleType type)
    {
        // Ensure Instance is available
        if (Instance == null)
        {
            Debug.LogError("No instance of FlyingDotController found in the scene. Please add one to set up dotTexture.");
            return;
        }

        // Create the dot GameObject
        GameObject dot = new GameObject("FlyingDot");
        Image dotImage = dot.AddComponent<Image>();

        // Use the texture set on the instance
        if (Instance.dotSprite != null)
        {
            dotImage.sprite = Instance.dotSprite;
        }
        else
        {
            Debug.LogWarning("Dot texture is not assigned in the Inspector on FlyingDotController.");
        }

        // Set color based on CollectibleType
        switch (type)
        {
            case CollectibleType.ExplosionRange:
                dotImage.color = Color.red;
                break;
            case CollectibleType.ShotSpeed:
                dotImage.color = Color.blue;
                break;
            case CollectibleType.FullHealth:
                dotImage.color = Color.green;
                break;
        }

        // Set dot to start at world position in screen space
        Vector3 startScreenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        dot.transform.SetParent(Instance.canvas.transform);
        dot.transform.position = startScreenPosition;
        dot.GetComponent<RectTransform>().sizeDelta = new Vector2(Instance.size, Instance.size); // Adjust size as desired

        dot.AddComponent<FlyingDotMover>().Initialize(targetScreenPosition, dot, Instance.curve);
    }
}
