using UnityEngine;
using UnityEngine.AI;
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
    [SerializeField] private NavMeshObstacle navMeshObstacle;
    public ACRocketPropulsion propulsionComponent;
    public ACRocketBody bodyComponent;
    public ACRocketFront frontComponent;

    public RocketState state { get; private set; } = RocketState.ATTACHED;
    public UnityEvent<RocketState> OnRocketStateChange;
    public RocketData settings;
    public Transform initialTransform { get; private set; }
    public Vector3 shootingDirection { get; private set; }

    void Start()
    {
        navMeshObstacle.enabled = false;

        // Create a new GameObject to hold the initial transform
        GameObject initialTransformHolder = new GameObject($"{gameObject.name}_InitialTransform");
        initialTransformHolder.transform.SetParent(transform.parent);
        initialTransformHolder.transform.position = transform.position;
        initialTransformHolder.transform.rotation = transform.rotation;

        initialTransform = initialTransformHolder.transform;
    }

    void OnTriggerEnter(Collider other)
    {
        if (state == RocketState.ATTACHED || state == RocketState.REGROWING || state == RocketState.IDLE) return;
        if (other.gameObject.tag == "Ground") return;

        //Collision Logic for Enemies
        if (other.gameObject.layer == LayerMask.NameToLayer("PL_IsEnemy"))
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                other.GetComponent<EnemyDamageHandler>().DestroyEnemy();
            }
            else if (other.gameObject.CompareTag("EnemyArmor"))
            {
                if (frontComponent.GetType() == typeof(BouncingFront) || frontComponent.GetType() == typeof(PenetrativeFront))
                {
                    frontComponent.ActivateAbility(other);
                }
                else
                {
                    Explode();
                }
                return;
            }
        }

        //Colision logic for environment and harvester
        if (other.gameObject.layer == LayerMask.NameToLayer("PL_IsEnvironmentPhysicalObject") || other.gameObject.layer == LayerMask.NameToLayer("PL_IsHarvester"))
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
    }
    public void SetState(RocketState state)
    {
        if (this.state == state) return;

        this.state = state;
        navMeshObstacle.enabled = state == RocketState.IDLE;

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
        gameObject.layer = LayerMask.NameToLayer("PL_IsRocket");
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
                Destroy(propulsionComponent.gameObject);
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
                Destroy(bodyComponent.gameObject);
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
                Destroy(frontComponent.gameObject);
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

        gameObject.layer = LayerMask.NameToLayer("PL_IsPlayer");
    }
}
