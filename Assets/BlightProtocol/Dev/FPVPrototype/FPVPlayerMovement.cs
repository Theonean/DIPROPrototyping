using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPVPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    public bool isAtDroneControl = false;

    private void Start() {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        MyInput();
    }

    private void MyInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer() {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void FixedUpdate() {
        MovePlayer();
    }
}
