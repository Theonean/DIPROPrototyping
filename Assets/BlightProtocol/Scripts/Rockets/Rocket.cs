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
    public ACRocketPropulsion propulsionComponent { get; private set; }
    public ACRocketBody bodyComponent { get; private set; }
    public ACRocketFront frontComponent { get; private set; }

    public RocketState state { get; private set; } = RocketState.ATTACHED;
    public UnityEvent<RocketState> OnRocketStateChange;
    public RocketData settings;
    public Transform initialTransform { get; private set; }
    public Vector3 shootingDirection { get; private set; }

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

    void OnTriggerEnter(Collider other)
    {
        if (state == RocketState.ATTACHED) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyArmor"))
        {
            switch (frontComponent.GetType().Name)
            {
                case "BouncingFront":
                    frontComponent.ActivateAbility(other);
                    break;
                case "PenetrativeFront":
                    frontComponent.ActivateAbility(other);
                    break;
                default:
                    Explode();
                    break;
            }
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") || other.gameObject.layer == LayerMask.NameToLayer("Harvester"))
        {
            switch (frontComponent.GetType().Name)
            {
                case "BouncingFront":
                    frontComponent.ActivateAbility(other);
                    break;
                default:
                    Explode();
                    break;
            }
            return;
        }

        if (state == RocketState.FLYING)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Rocket"))
            {
                Rocket otherRocket = other.gameObject.GetComponent<Rocket>();
                if (otherRocket.state == RocketState.ATTACHED ||
                    otherRocket.state == RocketState.RETURNING)
                    return;
            }

            frontComponent.ActivateAbility(other);
        }

        //If other has component enemydamagehandler, destroy
        if (other.GetComponent<EnemyDamageHandler>() != null)
        {
            other.GetComponent<EnemyDamageHandler>().DestroyEnemy();
        }
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
        shootingDirection = (target - transform.position).normalized;
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
            propulsionComponent = Instantiate(propulsion, transform).GetComponent<ACRocketPropulsion>();
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
            bodyComponent = Instantiate(body, transform).GetComponent<ACRocketBody>();
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
            frontComponent = Instantiate(front, transform).GetComponent<ACRocketFront>();
        }
    }

    public void ReattachRocketToDrone(RocketState stateOverride = RocketState.ATTACHED)
    {
        transform.position = initialTransform.position;
        SetState(stateOverride);
        transform.SetParent(initialTransform.parent);
        transform.rotation = initialTransform.rotation;
        transform.localScale = initialTransform.localScale;

        frontComponent.abilityUsesLeft = frontComponent.maxAbilityUses;
    }
}
