using UnityEngine;




[CreateAssetMenu(fileName = "SOItem", menuName = "ScriptableObjects/SOItem", order = 0)]
[System.Serializable]
public class SOItem : ScriptableObject
{
    public GameObject prefab;
    public EItemTypes itemType;
}
