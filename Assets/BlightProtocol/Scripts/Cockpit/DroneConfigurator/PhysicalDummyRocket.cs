using UnityEngine;

public class PysicalDummyRocket : MonoBehaviour
{
    [SerializeField] private bool singleComponent = false;
    private GameObject frontComponent, bodyComponent, propulsionComponent;
    [SerializeField] private float yOffsetFront, yOffsetBody, yOffsetProp = 0f;
    [SerializeField] private float scaleModifier = 1f;
    [SerializeField] private GameObject[] fronts, bodies, props;

    void Start()
    {
        SetComponent(RocketComponentType.FRONT, fronts[Mathf.FloorToInt(Random.Range(0, fronts.Length - 1))]);
        SetComponent(RocketComponentType.BODY, bodies[Mathf.FloorToInt(Random.Range(0, bodies.Length - 1))]);
        SetComponent(RocketComponentType.PROPULSION, props[Mathf.FloorToInt(Random.Range(0, props.Length - 1))]); 
    }

    public void SetComponent(RocketComponentType type, GameObject newComponent)
    {
        if (singleComponent)
        {
            if (frontComponent != null) Destroy(frontComponent);
            if (bodyComponent != null) Destroy(bodyComponent);
            if (propulsionComponent != null) Destroy(propulsionComponent);
        }
        switch (type)
        {
            case RocketComponentType.FRONT:
                if (frontComponent != null) Destroy(frontComponent);
                frontComponent = Instantiate(newComponent, transform, false);
                frontComponent.transform.localScale *= scaleModifier;
                frontComponent.transform.localPosition = new Vector3(0, yOffsetFront, 0);
                frontComponent.transform.localRotation = Quaternion.identity;
                frontComponent.layer = gameObject.layer;
                frontComponent.GetComponentInChildren<MeshRenderer>().enabled = true;
                break;
            case RocketComponentType.BODY:
                if (bodyComponent != null) Destroy(bodyComponent);
                bodyComponent = Instantiate(newComponent, transform, false);
                bodyComponent.transform.localScale *= scaleModifier;
                bodyComponent.transform.localPosition = new Vector3(0, -yOffsetBody, 0);
                bodyComponent.transform.localRotation = Quaternion.identity;
                bodyComponent.layer = gameObject.layer;
                bodyComponent.GetComponentInChildren<MeshRenderer>().enabled = true;
                break;
            case RocketComponentType.PROPULSION:
                if (propulsionComponent != null) Destroy(propulsionComponent);
                propulsionComponent = Instantiate(newComponent, transform, false);
                propulsionComponent.transform.localScale *= scaleModifier;
                propulsionComponent.transform.localPosition = new Vector3(0, yOffsetProp, 0);
                propulsionComponent.transform.localRotation = Quaternion.identity;
                propulsionComponent.layer = gameObject.layer;
                propulsionComponent.GetComponentInChildren<MeshRenderer>().enabled = true;
                break;
        }
    }
}
