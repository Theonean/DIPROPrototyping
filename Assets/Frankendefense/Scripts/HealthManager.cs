using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthManager : MonoBehaviour
{
    [Tooltip("Hits needed until this object signals death")]
    public int health;
    public UnityEvent died;
}
