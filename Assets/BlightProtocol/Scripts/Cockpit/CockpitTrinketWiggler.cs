using UnityEngine;

public class CockpitTrinketWiggler : MonoBehaviour
{
    [SerializeField] Rigidbody forceTarget;
    [SerializeField] private Vector2 forceMagnitudeRange = new(0.5f, 1f);
    private float currentWiggleForce = 1;
    [SerializeField] private Vector3 forceAxis;
    [SerializeField] private float forceInterval = 1f;
    private bool isWiggling = false;
    private Seismograph seismograph;
    private int lastForceDirSign = 1;

    void Start()
    {
        seismograph = Seismograph.Instance;
        seismograph.OnDangerLevelChanged.AddListener(OnSeismoDangerLevelChanged);
    }

    void OnSeismoDangerLevelChanged(int level) {
        if (level == 0)
        {
            isWiggling = false;
        }
        else
        {
            isWiggling = true;
            currentWiggleForce = level;
        }
    }

    private float timeSinceLastForce = 0f;

    void Update()
    {
        if (isWiggling)
        {
            if (timeSinceLastForce > forceInterval)
            {
                lastForceDirSign = -lastForceDirSign;
                Vector3 forceDir = new Vector3(Random.Range(0, forceAxis.x)*lastForceDirSign,
                Random.Range(0, forceAxis.y)*lastForceDirSign,
                Random.Range(0, forceAxis.z)*lastForceDirSign);
                forceTarget.AddTorque(forceDir * Random.Range(forceMagnitudeRange.x, forceMagnitudeRange.y) * currentWiggleForce, ForceMode.Impulse);
                timeSinceLastForce = 0;
            }
            timeSinceLastForce += Time.deltaTime;
        }

    }
}
