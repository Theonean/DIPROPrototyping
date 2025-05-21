using UnityEngine;

public class ConfiguratorDummyRocket : MonoBehaviour
{
    private GameObject frontComponent, bodyComponent, propulsionComponent;
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private float scaleModifier = 1f;

    public void SetComponent(RocketComponentType type, GameObject newComponent)
    {
        switch (type)
        {
            case RocketComponentType.FRONT:
                if (frontComponent != null) Destroy(frontComponent);
                frontComponent = Instantiate(newComponent, transform.position + new Vector3(0, yOffset, 0), transform.rotation, transform);
                frontComponent.transform.localScale *= scaleModifier;
                break;
            case RocketComponentType.BODY:
                if (bodyComponent != null) Destroy(bodyComponent);
                bodyComponent = Instantiate(newComponent, transform.position, transform.rotation, transform);
                bodyComponent.transform.localScale *= scaleModifier;
                break;
            case RocketComponentType.PROPULSION:
                if (propulsionComponent != null) Destroy(propulsionComponent);
                propulsionComponent = Instantiate(newComponent, transform.position + new Vector3(0, -yOffset, 0), transform.rotation, transform);
                bodyComponent.transform.localScale *= scaleModifier;
                break;
        }
    }
}
