using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartController : MonoBehaviour
{
    enum DriftMode
    {
        NONE, LEFT, RIGHT
    }

    [Header("References")]
    public Transform model;
    public Transform slopeRotator;
    public Tire[] frontTires;
    public Tire[] backTires;
    public Sparks[] sparks;
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

    [Header("Drifting Properties")]
    public float driftSpeedThreshold;
    public float driftTurnSpeedThreshold = 2;
    public int driftLevels = 3;
    public float[] driftLevelTimeThresholds;
    public float driftRotationMult = 2;
    public float[] driftSpeedMultipliers;
    public float driftAmountAdd = 1.1f;
    public float driftAmountMult = 0.7f;

    [Space]
    [Header("Model Properties")]
    public float xRotationAmount = 2;
    public float yRotationAmount = 8;
    public float zRotationAmount = 2;
    public float modelRotationSpeed = 8;

    private Rigidbody rb;

    private float currentSpeed;
    private float currentTurnSpeed;
    private float currentYRotation;
    private float currentXRotation;

    private DriftMode driftMode;
    private int currentDriftLevel;
    private float currentDriftTime;

    private Vector3 floorNormal;

    private float currentSpeedMult = 1;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        bool meetsSpeedThreshold = currentSpeed > driftSpeedThreshold;
        bool meetsTurnThreshold = Mathf.Abs(currentTurnSpeed) > driftTurnSpeedThreshold;
        if (!IsDrifting() && (Input.GetButtonDown("Drift") || Input.GetAxis("Drift") > 0))
        {
            if (meetsSpeedThreshold && meetsTurnThreshold)
                StartDrift(currentTurnSpeed < 0 ? DriftMode.LEFT : DriftMode.RIGHT);
        }

        bool driftInputLifted = Input.GetButtonUp("Drift") || Input.GetAxis("Drift") < 1;
        if (IsDrifting() && (driftInputLifted || !meetsSpeedThreshold))
        {
            EndDrift(!meetsSpeedThreshold);
        }

        currentDriftTime += Time.deltaTime;
        if (IsDrifting() && currentDriftLevel < driftLevels && currentDriftTime >= driftLevelTimeThresholds[currentDriftLevel - 1])
        {
            IncreaseDriftLevel();
        }

        currentSpeedMult = Mathf.MoveTowards(currentSpeedMult, 1, Time.deltaTime * speedMultReturnSpeed);

        HandleInput();

        ApplyVelocityAndRotation();

        DoModelRotations();

        RotateToSlope();

        UpdateTires();
    }

    void HandleInput()
    {
        var accelerating = Input.GetButton("Accelerate");
        var targetSpeed = accelerating ? topSpeed : 0;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.deltaTime * (accelerating ? acceleration : deceleration));

        if (Input.GetButton("Reverse"))
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, reverseSpeed, brakingForce * Time.deltaTime);
        }


        float turning = Input.GetAxis("Horizontal");
        if (IsDrifting())
        {
            turning = (turning + (driftMode == DriftMode.LEFT ? -1 : 1)) / 2 * driftAmountMult;
            turning += (driftAmountAdd * (driftMode == DriftMode.LEFT ? -1 : 1));
        }

        currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, turning * topTurnSpeed, rotationalAcceleration * Time.deltaTime);
    }

    void ApplyVelocityAndRotation()
    {
        Vector3 vel = transform.forward * currentSpeed * currentSpeedMult;
        vel.y = rb.velocity.y;
        rb.velocity = vel;

        rb.rotation *= Quaternion.Euler(Vector3.up * currentTurnSpeed * Time.deltaTime * GetSpeedRatio());

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
        Vector3 relativeVelocity = slopeRotator.TransformDirection(rb.velocity);
        // print(relativeVelocity);

        float targetXRot = -xRotationAmount * rb.velocity.y;
        targetXRot = Mathf.Clamp(targetXRot, -80, 80);
        currentXRotation = Mathf.Lerp(currentXRotation, targetXRot, Time.deltaTime * modelRotationSpeed);
        model.localRotation *= Quaternion.AngleAxis(currentXRotation, Vector3.right);
    }

    void RotateToSlope()
    {
        floorNormal = Vector3.up;

        float xRot = 0;
        float zRot = 0;
        float dot = 0;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1))
        {
            floorNormal = hit.normal;

            xRot = Vector3.SignedAngle(Vector3.up, floorNormal, transform.right);
            // zRot = Vector3.SignedAngle(Vector3.up, floorNormal, Vector3.up);
            dot = Vector3.Dot(transform.forward, floorNormal);
            print(dot);

            Debug.DrawRay(transform.position, floorNormal, Color.red, Time.deltaTime);
        }
        Quaternion target = Quaternion.Euler(xRot, 0, -zRot * Mathf.Sign(dot));
        // Quaternion target = Quaternion.LookRotation(Vector3.Cross(slopeRotator.right, floorNormal));
        // slopeRotator.rotation = Quaternion.Slerp(slopeRotator.rotation, target, Time.deltaTime * modelRotationSpeed);
        slopeRotator.localRotation = Quaternion.Slerp(slopeRotator.localRotation, target, Time.deltaTime * modelRotationSpeed);
        // slopeRotator.transform.up = Quaternion.Euler(0, transform.eulerAngles.y, 0) * normal;
    }

    float GetSpeedRatio()
    {
        return currentSpeed / topSpeed;
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
}
