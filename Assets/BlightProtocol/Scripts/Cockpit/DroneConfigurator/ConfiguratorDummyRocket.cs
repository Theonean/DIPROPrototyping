using UnityEngine;

public class ConfiguratorDummyRocket : MonoBehaviour
{
    private GameObject frontComponent, bodyComponent, propulsionComponent;

    public void SetComponent(RocketComponentType type, GameObject newComponent)
    {
        switch (type)
        {
            case RocketComponentType.FRONT:
                if (frontComponent != null) Destroy(frontComponent);
                frontComponent = Instantiate(newComponent, transform.position, transform.rotation, transform);
                break;
            case RocketComponentType.BODY:
                if (bodyComponent != null) Destroy(bodyComponent);
                bodyComponent = Instantiate(newComponent, transform.position, transform.rotation, transform);
                break;
            case RocketComponentType.PROPULSION:
                if (propulsionComponent != null) Destroy(propulsionComponent);
                propulsionComponent = Instantiate(newComponent, transform.position, transform.rotation, transform);
                break;
        }
    }
}
