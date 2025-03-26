using UnityEngine;

public abstract class ACRocketComponent : MonoBehaviour
{
    public Rocket parentRocket;
    public Rocket ParentRocket
    {
        get
        {
            if (parentRocket == null)
            {
                parentRocket = GetComponentInParent<Rocket>();
            }
            return parentRocket;
        }
    }
    public int componentLevel = 0;
    public string DescriptiveName;

    protected Vector3 rocketOriginalScale;

    protected Transform rocketTransform
    {
        get { return ParentRocket.transform; }
    }

    private void Awake()
    {
        rocketOriginalScale = ParentRocket.transform.localScale;
    }
}