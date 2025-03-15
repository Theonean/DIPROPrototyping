using UnityEngine;
using UnityEngine.UI;

public class FlyingDotController : MonoBehaviour
{
    // Singleton-like instance
    public static FlyingDotController Instance { get; private set; }

    [SerializeField]
    private GameObject dotImagePrefab;
    [SerializeField]
    private AnimationCurve curve; // Set this directly in the Inspector on the instance in the scene
    [SerializeField]
    private float size = 10f; // Set this directly in the Inspector on the instance in the scene
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private Color explosionColor = Color.red;
    [SerializeField]
    private Color shotSpeedColor = Color.blue;
    [SerializeField]
    private Color healthColor = Color.green;


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
    public static void CreateFlyingDot(Vector3 worldPosition, Vector3 targetScreenPosition, ECollectibleType type)
    {
        // Ensure Instance is available
        if (Instance == null)
        {
            Debug.LogError("No instance of FlyingDotController found in the scene. Please add one to set up dotTexture.");
            return;
        }

        // Create the dot GameObject
        GameObject dot = Instantiate(Instance.dotImagePrefab);
        Image dotImage = dot.GetComponent<Image>();
        Debug.Log(dotImage);


        // Set color based on CollectibleType
        switch (type)
        {
            case ECollectibleType.ExplosionRange:
                dotImage.color = Instance.explosionColor;
                break;
            case ECollectibleType.ShotSpeed:
                dotImage.color = Instance.shotSpeedColor;
                break;
            case ECollectibleType.FullHealth:
                dotImage.color = Instance.healthColor;
                break;
        }

        // Set dot to start at world position in screen space
        Vector3 startScreenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        dot.transform.SetParent(Instance.canvas.transform);
        dot.transform.position = startScreenPosition;
        dot.transform.localScale = new Vector3(Instance.size, Instance.size, Instance.size);

        dot.AddComponent<FlyingDotMover>().Initialize(targetScreenPosition, dot, Instance.curve);
    }
}
