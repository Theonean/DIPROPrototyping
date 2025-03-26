using UnityEngine;
using UnityEngine.Events;

public enum RocketState
{
    ATTACHED,
    FLYING,
    IDLE,
    RETURNING,
    EXPLODING,
    REGROWING
}

public class Rocket : MonoBehaviour
{
    public GameObject propulsionComponent { get; private set; }
    public GameObject bodyComponent { get; private set; }
    public GameObject frontComponent { get; private set; }

    public RocketState state { get; private set; } = RocketState.ATTACHED;
    public UnityEvent<RocketState> OnRocketStateChange;
    public RocketData settings;
    public Transform initialTransform { get; private set; }

    void Start()
    {
        // Create a new GameObject to hold the initial transform
        GameObject initialTransformHolder = new GameObject($"{gameObject.name}_InitialTransform");
        initialTransformHolder.transform.SetParent(transform.parent);
        initialTransformHolder.transform.position = transform.position;
        initialTransformHolder.transform.rotation = transform.rotation;

        initialTransform = initialTransformHolder.transform;

        RocketAimController rAI = GetComponentInParent<RocketAimController>();
        SetPropulsion(rAI.rocketPropulsions[0]);
        SetBody(rAI.rocketBodies[0]);
        SetFront(rAI.rocketFronts[0]);
    }

    public void SetState(RocketState state)
    {
        if (this.state == state) return;

        this.state = state;
        OnRocketStateChange?.Invoke(state);
    }

    public bool CanExplode()
    {
        return state == RocketState.FLYING || state == RocketState.RETURNING || state == RocketState.IDLE;
    }

    public bool CanBeReturned()
    {
        return state == RocketState.FLYING || state == RocketState.IDLE;
    }

    public void Shoot(Vector3 target)
    {
        SetState(RocketState.FLYING);
        propulsionComponent.GetComponent<ACRocketPropulsion>().Shoot(target);
    }

    public void Explode()
    {
        bodyComponent.GetComponent<ACRocketBody>().TryExplode();
    }

    public void SetPropulsion(GameObject propulsion)
    {
        if (propulsion.GetComponent<ACRocketPropulsion>() == null)
        {
            Debug.LogError("Propulsion component must inherit from ACRocketPropulsion");
            return;
        }
        else
        {
            if (propulsionComponent != null)
            {
                Destroy(propulsionComponent);
            }
            propulsionComponent = Instantiate(propulsion, transform);
        }
    }

    public void SetBody(GameObject body)
    {
        if (body.GetComponent<ACRocketBody>() == null)
        {
            Debug.LogError("Body component must inherit from ACRocketBody");
            return;
        }
        else
        {
            if (bodyComponent != null)
            {
                Destroy(bodyComponent);
            }
            bodyComponent = Instantiate(body, transform);
        }
    }

    public void SetFront(GameObject front)
    {
        if (front.GetComponent<ACRocketFront>() == null)
        {
            Debug.LogError("Front component must inherit from ACRocketFront");
            return;
        }
        else
        {
            if (frontComponent != null)
            {
                Destroy(frontComponent);
            }
            frontComponent = Instantiate(front, transform);
        }
    }
}
