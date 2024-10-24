using UnityEngine;

public class LegDirectionalClickHandler : MonoBehaviour
{
    public GameObject leg1; //northwest
    public GameObject leg2; //Northeast
    public GameObject leg3; //Southeast
    public GameObject leg4; //Southwest
    private GameObject activeLeg; // The leg currently tracked and attached to the core
    GameObject lastLegClicked;

    //Detect if a click happens, then call "LegClicked", when released call "LegReleased" on the same leg
    void Update()
    {
        // 1. Track the leg with the lowest index that is attached to the core
        activeLeg = GetLowestIndexAttachedLeg();

        // Rotate the object towards the mouse
        RotateTowardsMouse();

        if (Input.GetMouseButtonDown(0) && activeLeg != null)
        {
            //Raycast to find the position to fly to, and if nothing is hit return
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;

            //Check if a leg was hit, if yes return
            if (hit.collider.gameObject.CompareTag("Leg"))
            {
                return;
            }

            // Make the tracked leg the one to shoot
            lastLegClicked = activeLeg;
            lastLegClicked.GetComponent<LegHandler>().LegClicked();
        }

        if (Input.GetMouseButtonUp(0) && lastLegClicked != null)
        {
            lastLegClicked.GetComponent<LegHandler>().LegReleased();
            lastLegClicked = null;
        }
    }

    // Function to track the leg with the lowest index that is attached to the core
    private GameObject GetLowestIndexAttachedLeg()
    {
        // Check legs in order of index to find the first attached leg
        if (leg1.GetComponent<LegHandler>().m_LegState == LegState.ATTACHED)
        {
            return leg1;
        }
        if (leg2.GetComponent<LegHandler>().m_LegState == LegState.ATTACHED)
        {
            return leg2;
        }
        if (leg3.GetComponent<LegHandler>().m_LegState == LegState.ATTACHED)
        {
            return leg3;
        }
        if (leg4.GetComponent<LegHandler>().m_LegState == LegState.ATTACHED)
        {
            return leg4;
        }

        // Return null if no legs are attached
        return null;
    }
    
    // Function to rotate the object so the tracked leg points towards the mouse
    private void RotateTowardsMouse()
    {
        if (activeLeg == null) return; // No active leg, no need to rotate

        // Raycast to find the mouse position in world space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 direction = hit.point - transform.position; // Direction from the core to the mouse
            direction.y = 0; // Keep rotation only on the XZ plane

            // Get the current rotation of the player
            Quaternion currentRotation = transform.rotation;

            // Calculate the new desired rotation relative to the current rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Apply the rotation relative to the current one
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * 10f); // Smooth rotation, adjust speed if necessary

            // Ensure the active leg is pointing towards the mouse
            activeLeg.transform.rotation = Quaternion.Slerp(activeLeg.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

}
