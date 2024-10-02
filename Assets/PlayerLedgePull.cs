using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLedgePull : MonoBehaviour
{
    [Header("Ledge Detection")]
    public float bottomOffset;
    public float topOffset;
    private float playerHeight;

    public float ledgeCheckDistance;
    public LayerMask whatIsWall;
    private bool bottom;
    private bool top;
    private RaycastHit bottomHit;
    private RaycastHit topHit;

    [Header("Ledge Pulling Force")]
    public float upwardForce;
    public float forwardForce;

    [Header("References")]
    public Transform orientation;
    private PlayerMovement pm;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        playerHeight = pm.playerHeight;
    }

    private void Update()
    {
        CheckForLedge();
        if (bottom && !top)
        {
            DoLedgeJump();
        }
    }

    private void CheckForLedge()
    {
        bottom = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - playerHeight * bottomOffset, transform.position.z), orientation.forward, out bottomHit, ledgeCheckDistance, whatIsWall);
        top = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - playerHeight * topOffset, transform.position.z), orientation.forward, out topHit, ledgeCheckDistance, whatIsWall);

        /*
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - playerHeight * bottomOffset, transform.position.z), orientation.forward * ledgeCheckDistance, Color.green);
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - playerHeight * topOffset, transform.position.z), orientation.forward * ledgeCheckDistance, Color.red);
        */
    }

    private void DoLedgeJump()
    {
        Vector3 forceToApply = orientation.forward * forwardForce + orientation.up * upwardForce;
        rb.AddForce(forceToApply, ForceMode.Force);
    }
}
