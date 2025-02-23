using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPVInteractable : MonoBehaviour
{
    public virtual void Interact() {
        Debug.Log("Interact");
    }
}
