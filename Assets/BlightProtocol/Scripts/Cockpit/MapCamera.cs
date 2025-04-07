using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public float yPos;

    void Start()
    {
        yPos = transform.position.y;

    }

    void Update()
    {
        transform.position = new Vector3(Harvester.Instance.transform.position.x, yPos, Harvester.Instance.transform.position.z);
    }

    public void SetHeight(float height) {
        yPos = height;
    }
}
