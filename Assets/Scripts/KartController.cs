using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartController : MonoBehaviour
{
    enum DriftMode
    {
        NONE, LEFT, RIGHT
    }

    const float GRAVITY = -9.8f * 2.5f;

    [Header("References")]
    public Transform model;
    public Transform person;
    public Tire[] frontTires;
    public Tire[] backTires;
    public Sparks[] sparks;

    public UnityEngine.UI.Image groundedIndicator;

    [Space]

    [Header("Kart Properties")]
    public float acceleration = 6;
    public float deceleration = 5;
    public float brakingForce = 8;
    public float topSpeed = 10;
    public float reverseSpeed = -4;
    public float speedMultReturnSpeed = 5;
    [Space]
    public float rotationalAcceleration = 6;
    public float rotationalDeceleration = 5;
    public float topTurnSpeed = 5;
    [Space]
    public float jumpVelocity = 10;

    [Space]
    [Header("Floor Rotation Properties")]
    public LayerMask trackLayerMask;
    public float trackRotationSpeed = 5;

    [Header("Drifting Properties")]
    public float driftSpeedThreshold;
    public float driftTurnSpeedThreshold = 2;
    public int driftLevels = 3;
    public float[] driftLevelTimeThresholds;
    public float driftRotationMult = 2;
    public float[] driftSpeedMultipliers;
    public float driftAmountAdd = 1.1f;
    public float driftAmountMult = 0.7f;
    public float driftInitialBounceAmt = 1;

    [Space]
    [Header("Ramp Boost Properties")]
    public float rampBoostTimeAllowance = 0.1f;
    public float rampBoostSpeedMultiplier = 1.3f;


    [Space]
    [Header("Model Properties")]
    public float xRotationAmount = 2;
    public float yRotationAmount = 8;
    public float zRotationAmount = 2;
    public float modelRotationSpeed = 8;
    public float personRotateAmount = 15;

    private Rigidbody rb;

    private Vector3 velocity;
    private Quaternion targetRot;

    private float currentSpeed;
    private float currentTurnSpeed;
    private float currentYRotation;
    private float currentXRotation;

    private DriftMode driftMode;
    private int currentDriftLevel;
    private float currentDriftTime;

    private Vector3 floorNormal;

    private float currentSpeedMult = 1;

    private bool driftTriggerIsDown;

    private float timeSinceLeftGround;
    private bool groundedLastFrame;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandleInput();

        if (IsDrifting())
            CheckDriftLevel();

        // ApplyGravity();

        if (groundedLastFrame && !IsGrounded())
        {
            timeSinceLeftGround = 0;
        }
        timeSinceLeftGround += Time.deltaTime;
        groundedLastFrame = IsGrounded();

        if (Input.GetButtonDown("Drift") || (Input.GetAxis("Drift") > 0 && !driftTriggerIsDown))
        {
            TryRampBoost();
        }

        ApplyVelocityAndRotation();

        DoModelRotations();

        RotateToSlope();

        UpdateTires();

        groundedIndicator.color = IsGrounded() ? Color.green : Color.red;
    }

    void HandleInput()
    {
        // DRIFT STUFF
        bool meetsSpeedThreshold = currentSpeed > driftSpeedThreshold;
        bool meetsTurnThreshold = Mathf.Abs(currentTurnSpeed) > driftTurnSpeedThreshold;
        if (IsGrounded() && (Input.GetButtonDown("Drift") || (Input.GetAxis("Drift") > 0 && !driftTriggerIsDown)))
        {
            driftTriggerIsDown = true;
            DoDriftBounce();
            if (!IsDrifting() && meetsSpeedThreshold && meetsTurnThreshold)
                StartDrift(currentTurnSpeed < 0 ? DriftMode.LEFT : DriftMode.RIGHT);
        }

        bool driftInputLifted = Input.GetButtonUp("Drift") || (Input.GetAxis("Drift") < 1);
        if (IsDrifting() && (driftInputLifted || !meetsSpeedThreshold))
        {
            driftTriggerIsDown = false;
            EndDrift(!meetsSpeedThreshold);
        }

        if (Input.GetAxis("Drift") < 1 && driftTriggerIsDown)
        {
            driftTriggerIsDown = false;
        }

        // ACCELERATION STUFF
        var accelerating = Input.GetButton("Accelerate");
        var targetSpeed = accelerating ? topSpeed : 0;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.deltaTime * (accelerating ? acceleration : deceleration));

        if (Input.GetButton("Reverse"))
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, reverseSpeed, brakingForce * Time.deltaTime);
        }

        // STEERING STUFF
        float turning = Input.GetAxis("Horizontal");
        if (IsDrifting())
        {
            turning = (turning + (driftMode == DriftMode.LEFT ? -1 : 1)) / 2 * driftAmountMult;
            turning += (driftAmountAdd * (driftMode == DriftMode.LEFT ? -1 : 1));
        }

        currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, turning * topTurnSpeed, rotationalAcceleration * Time.deltaTime);

        // JUMPING STUFF
        if (IsGrounded() && Input.GetButtonDown("Jump"))
        {
            Vector3 vel = rb.velocity;
            vel += floorNormal * jumpVelocity;
            rb.velocity = vel;
        }
    }

    void CheckDriftLevel()
    {
        currentDriftTime += Time.deltaTime;
        if (currentDriftLevel < driftLevels && currentDriftTime >= driftLevelTimeThresholds[currentDriftLevel - 1])
        {
            IncreaseDriftLevel();
        }
    }

    void ApplyGravity()
    {
        if (IsGrounded())
        {
            return;
        }

        velocity += floorNormal * GRAVITY * Time.deltaTime;
    }

    void ApplyVelocityAndRotation()
    {
        currentSpeedMult = Mathf.MoveTowards(currentSpeedMult, 1, Time.deltaTime * speedMultReturnSpeed);

        Vector3 vel = rb.velocity;
        vel = transform.InverseTransformVector(vel);

        float cachedY = vel.y;
        vel = Vector3.forward * currentSpeed * currentSpeedMult;
        vel.y = cachedY;

        vel = transform.TransformVector(vel);

        // Vector3 vel = transform.forward * currentSpeed * currentSpeedMult;

        // if (!IsGrounded())
        if (true)
            vel += floorNormal * GRAVITY * Time.deltaTime;

        // vel.y = rb.velocity.y;

        rb.velocity = vel;

        // rb.rotation *= Quaternion.Euler(Vector3.up * currentTurnSpeed * Time.deltaTime * GetSpeedRatio());
        // targetRot *= Quaternion.Euler(Vector3.up * currentTurnSpeed * Time.deltaTime * GetSpeedRatio());

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * trackRotationSpeed);
        // transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, Time.deltaTime * trackRotationSpeed);
        // transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * trackRotationSpeed);

        transform.localRotation *= Quaternion.AngleAxis(currentTurnSpeed * Time.deltaTime * GetSpeedRatio(), Vector3.up);

    }

    void DoModelRotations()
    {
        float rotationRatio = currentTurnSpeed / topTurnSpeed;
        // float driftMult = IsDrifting() ? driftRotationMult : 1;
        float driftMult = 1;

        float targetYRot = rotationRatio * GetSpeedRatio() * yRotationAmount * driftMult;
        currentYRotation = Mathf.Lerp(currentYRotation, targetYRot, Time.deltaTime * modelRotationSpeed);
        model.localRotation = Quaternion.AngleAxis(currentYRotation, Vector3.up);

        model.localRotation *= Quaternion.AngleAxis(rotationRatio * GetSpeedRatio() * zRotationAmount, Vector3.forward);


        // float dot = Vector3.Dot(rb.velocity)
        Vector3 relativeVelocity = transform.InverseTransformDirection(rb.velocity);
        // print(relativeVelocity);

        float targetXRot = -xRotationAmount * Vector3.Dot(floorNormal, rb.velocity);
        targetXRot = Mathf.Clamp(targetXRot, -30, 30);
        currentXRotation = Mathf.Lerp(currentXRotation, targetXRot, Time.deltaTime * modelRotationSpeed);
        model.localRotation *= Quaternion.AngleAxis(currentXRotation, Vector3.right);

        Quaternion targetPersonRot = Quaternion.AngleAxis(Input.GetAxis("Horizontal") * personRotateAmount, Vector3.forward);
        person.localRotation = Quaternion.Slerp(person.localRotation, targetPersonRot, Time.deltaTime * 5);
    }

    void RotateToSlope()
    {
        // floorNormal = Vector3.up;
        // targetRot = transform.rotation;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 50, trackLayerMask))
        {
            floorNormal = hit.normal;

            // Courtesy of runevision from the Unity forums: https://forum.unity.com/threads/quaternion-rotation-along-normal.22727/
            targetRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

        }
    }

    float GetSpeedRatio()
    {
        return currentSpeed / topSpeed;
    }

    void DoDriftBounce()
    {
        Vector3 vel = rb.velocity;
        vel += floorNormal * driftInitialBounceAmt;
        rb.velocity = vel;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -transform.up, 0.3f);
    }

    void StartDrift(DriftMode driftMode)
    {
        this.driftMode = driftMode;
        currentDriftTime = 0;
        currentDriftLevel = 1;
        UpdateSparks();
    }

    void EndDrift(bool noSpeedBoost)
    {
        if (!noSpeedBoost)
            currentSpeedMult = driftSpeedMultipliers[currentDriftLevel - 1];

        driftMode = DriftMode.NONE;
        currentDriftLevel = 0;
        UpdateSparks();
    }

    void IncreaseDriftLevel()
    {
        currentDriftLevel++;
        UpdateSparks();
    }

    bool IsDrifting()
    {
        return driftMode != DriftMode.NONE;
    }

    void UpdateSparks()
    {
        foreach (Sparks spark in sparks)
        {
            spark.SetSparkLevel(currentDriftLevel);
        }
    }

    void UpdateTires()
    {
        foreach (Tire tire in frontTires)
        {
            tire.SetSpeed(currentSpeed);
            tire.SetTilt(currentTurnSpeed / topTurnSpeed * GetSpeedRatio());
        }

        foreach (Tire tire in backTires)
        {
            tire.SetSpeed(currentSpeed);
        }
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed * currentSpeedMult;
    }

    private void TryRampBoost()
    {
        bool successful = timeSinceLeftGround <= rampBoostTimeAllowance;

        if (successful)
        {
            person.GetComponent<Animator>().SetTrigger("Jump");
            currentSpeedMult = rampBoostSpeedMultiplier;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        SpeedBooster booster = other.GetComponent<SpeedBooster>();
        if (booster)
        {
            currentSpeed = topSpeed;
            currentSpeedMult = booster.speedMult;
        }
    }
}
