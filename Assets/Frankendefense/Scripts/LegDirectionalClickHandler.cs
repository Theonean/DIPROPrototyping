using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegDirectionalClickHandler : MonoBehaviour
{
    public GameObject leg1; //northwest
    public GameObject leg2; //Northeast
    public GameObject leg3; //Southeast
    public GameObject leg4; //Southwest
    GameObject lastLegClicked;

    //Detect if a click happens, then call "LegClicked", when released call "LegReleased" on the same leg
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            //Raycast to find the position to fly to
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit);

            //Check if a leg was hit, if yes return
            if (hit.collider.gameObject.CompareTag("Leg"))
            {
                return;
            }

            Vector3 direction = hit.point - transform.position;

            direction.Normalize();

            Debug.Log(direction);

            if (direction.x > 0 && direction.z > 0)
            {
                leg2.GetComponent<LegHandler>().LegClicked();
                lastLegClicked = leg2;
            }
            else if (direction.x > 0 && direction.z < 0)
            {
                leg3.GetComponent<LegHandler>().LegClicked();
                lastLegClicked = leg3;
            }
            else if (direction.x < 0 && direction.z < 0)
            {
                leg4.GetComponent<LegHandler>().LegClicked();
                lastLegClicked = leg4;
            }
            else if (direction.x < 0 && direction.z > 0)
            {
                leg1.GetComponent<LegHandler>().LegClicked();
                lastLegClicked = leg1;
            }
        }

        if (Input.GetMouseButtonUp(0) && lastLegClicked != null)
        {
            lastLegClicked.GetComponent<LegHandler>().LegReleased();
            lastLegClicked = null;
        }
    }
}
