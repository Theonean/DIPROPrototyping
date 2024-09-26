using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSceneGravity : MonoBehaviour
{
    public Vector3 newGravity;

    private void Start()
    {
        Physics.gravity = newGravity;
    }
}
