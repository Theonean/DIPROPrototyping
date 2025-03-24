using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public abstract class ACRocketBody : MonoBehaviour
{
    public string DescriptiveName;
    public abstract void Explode();

    private void OnMouseDown()
    {
        Explode();
    }
}