using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSFXHandler : MonoBehaviour
{
    public PlayerCore player;

    //Track the player's position and adjust the volume of the ambient sound
    void FixedUpdate()
    {
        transform.position = player.transform.position;
    }
}
