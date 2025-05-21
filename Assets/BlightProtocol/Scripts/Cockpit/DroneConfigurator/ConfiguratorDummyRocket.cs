using UnityEngine;

public class ConfiguratorDummyRocket : MonoBehaviour
{
    private GameObject frontComponent, bodyComponent, propulsionComponent;
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private float scaleModifier = 1f;
    [SerializeField] bool setLayer = false;

    public void SetComponent(RocketComponentType type, GameObject newComponent)
    {
        switch (type)
        {
            case RocketComponentType.FRONT:
                if (frontComponent != null) Destroy(frontComponent);
                frontComponent = Instantiate(newComponent, transform, false);
                frontComponent.transform.localScale *= scaleModifier;
                frontComponent.transform.localPosition = new Vector3(0, -yOffset, 0);
                frontComponent.transform.localRotation = Quaternion.identity;
                frontComponent.layer = gameObject.layer;
                break;
            case RocketComponentType.BODY:
                if (bodyComponent != null) Destroy(bodyComponent);
                bodyComponent = Instantiate(newComponent, transform, false);
                bodyComponent.transform.localScale *= scaleModifier;
                bodyComponent.transform.localPosition = Vector3.zero;
                bodyComponent.transform.localRotation = Quaternion.identity;
                bodyComponent.layer = gameObject.layer;
                break;
            case RocketComponentType.PROPULSION:
                if (propulsionComponent != null) Destroy(propulsionComponent);
                propulsionComponent = Instantiate(newComponent, transform, false);
                propulsionComponent.transform.localScale *= scaleModifier;
                propulsionComponent.transform.localPosition = new Vector3(0, yOffset, 0);
                propulsionComponent.transform.localRotation = Quaternion.identity;
                propulsionComponent.layer = gameObject.layer;
                break;
        }
    }
}
