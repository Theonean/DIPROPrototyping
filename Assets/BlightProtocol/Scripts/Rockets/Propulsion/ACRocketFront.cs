using System.Collections;
using UnityEngine;

public abstract class ACRocketPropulsion : MonoBehaviour
{
    public string DescriptiveName;
    public abstract IEnumerator FlyToTargetPosition(Vector3 targetPos);
}