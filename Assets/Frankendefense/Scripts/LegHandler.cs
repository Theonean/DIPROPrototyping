using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegHandler : MonoBehaviour
{
    enum LegState
    {
        ATTACHED, // The leg is attached to the player.
        CLICKED, // The leg is clicked on and is waiting for next click to fly away.
        FLYING, // The leg is flying away.
        DETACHED, // The leg is detached from the player and lying on the floor (could be clicked again for attack).
        RETURNING // The leg is returning to the player.
    }
     Camera m_Camera;
   void Awake()
   {
       m_Camera = Camera.main;
   }
   void Update()
   {
       if (Input.GetMouseButtonDown(0))
       {
           Vector3 mousePosition = Input.mousePosition;
           Ray ray = m_Camera.ScreenPointToRay(mousePosition);
           if (Physics.Raycast(ray, out RaycastHit hit))
           {
            Debug.Log("Hit object: " + hit.collider.gameObject.name);
               // Use the hit variable to determine if this object was clicked
                if (hit.collider.gameObject == gameObject)
                {
                     // The leg was clicked
                     Debug.Log("Leg was clicked" + gameObject.name);
                }
           }
       }
   }
}
