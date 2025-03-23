using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] private MonoBehaviour propulsionComponent;
    [SerializeField] private MonoBehaviour bodyComponent;
    [SerializeField] private MonoBehaviour frontComponent;

    public IRocketPropulsion Propulsion => propulsionComponent as IRocketPropulsion;
    public IRocketBody Body => bodyComponent as IRocketBody;
    public IRocketFront Front => frontComponent as IRocketFront;
}
