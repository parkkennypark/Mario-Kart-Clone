using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartController : MonoBehaviour
{
    public float acceleration = 6;
    public float deceleration = 5;
    public float brakingForce = 8;
    public float topSpeed = 10;
    public float reverseSpeed = -4;

    public float rotationalAcceleration = 6;
    public float rotationalDeceleration = 5;
    public float topTurnSpeed = 5;

    private float currentSpeed;
    private float currentTurnSpeed;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        var accelerating = Input.GetButton("Accelerate");
        var targetSpeed = accelerating ? topSpeed : 0;
        // currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * (accelerating ? acceleration : deceleration));
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.deltaTime * (accelerating ? acceleration : deceleration));

        if (Input.GetButton("Reverse"))
        {
            currentSpeed = Mathf.Lerp(currentSpeed, reverseSpeed, brakingForce * Time.deltaTime);
        }

        rb.velocity = transform.forward * currentSpeed;

        float turning = Input.GetAxis("Horizontal");

        currentTurnSpeed = Mathf.Lerp(currentSpeed, turning, rotationalAcceleration * Time.deltaTime);
        rb.rotation *= Quaternion.Euler(Vector3.up * turning * topTurnSpeed);

    }
}
