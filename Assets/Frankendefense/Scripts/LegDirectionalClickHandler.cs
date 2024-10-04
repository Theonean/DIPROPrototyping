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
            //Select random number between one and including four to determine leg to shoot
            //if the leg is in attached state shoot it, otherwise select another leg
            int selectedLeg;
            GameObject legToShoot;
            bool validLegFound = false;
              int attempts = 0;
              int maxAttempts = 4;
              while (!validLegFound && attempts < maxAttempts)
              {
                  selectedLeg = attempts + 1;
                  switch (selectedLeg)
                  {
                      case 1:
                          legToShoot = leg1;
                          break;
                      case 2:
                          legToShoot = leg2;
                          break;
                      case 3:
                          legToShoot = leg3;
                          break;
                      case 4:
                          legToShoot = leg4;
                          break;
                      default:
                          legToShoot = null;
                          break;
                  }

                  if (legToShoot != null && legToShoot.GetComponent<LegHandler>().m_LegState == LegState.ATTACHED)
                  {
                      validLegFound = true;
                      lastLegClicked = legToShoot;
                      lastLegClicked.GetComponent<LegHandler>().LegClicked();
                  }
                  attempts++;
              }

              if (!validLegFound)
              {
                  Debug.Log("No attached legs found after " + maxAttempts + " attempts.");
              }

        }

        if (Input.GetMouseButtonUp(0) && lastLegClicked != null)
        {
            lastLegClicked.GetComponent<LegHandler>().LegReleased();
            lastLegClicked = null;
        }
    }
}
