using UnityEngine;

public class MapCamera : MonoBehaviour
{
    private float yPos;

    void Start()
    {
        yPos = transform.position.y;
    }

    void Update()
    {
        transform.position = new Vector3(Harvester.Instance.transform.position.x, yPos, Harvester.Instance.transform.position.z);
    }
}
