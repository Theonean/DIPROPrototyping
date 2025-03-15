using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "ResourceData", order = 0)]
public class ResourceData : ScriptableObject
{
    public string displayName = "Resource";
    public float energyValue = 1f;

}