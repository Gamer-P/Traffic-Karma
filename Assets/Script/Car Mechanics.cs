using UnityEngine;

public class CarMechanics : MonoBehaviour
{
    [Header("Car Settings")]
    public float motorForce = 2200f;   // 🔥 increased
    public float steeringAngle = 30f;
    public float brakeForce = 3000f;
    public float downforce = 100f;     // slightly higher
    public float maxSpeed = 30f;       // 🔥 increased
    public float maxSteerAngle = 30f;
    public float minSteerAngle = 10f;
    public float steerSmoothness = 5f;

    public bool reverseControls = false;


    private float horizontalInput;
    private float verticalInput;
    private bool isBraking;

    private Rigidbody rb;

    // Wheel Colliders
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    // Wheel Meshes
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = new Vector3(0, -0.8f, 0);

        // 🔥 Correct physics settings
        rb.linearDamping = 0.2f;
        rb.angularDamping = 1.5f;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        ApplyDownforce();
        UpdateWheels();
        LimitSpeed();
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBraking = Input.GetKey(KeyCode.Space);

        // 🔥 ADD THIS PART
        if (reverseControls)
        {
            horizontalInput *= -1f;
        }
    }

    void HandleMotor()
    {
        float speedDirection = Vector3.Dot(rb.linearVelocity, transform.forward);

        float torque = motorForce * verticalInput;

        // 🚀 DIRECT REVERSE (no heavy brake delay)
        if (verticalInput < 0)
        {
            frontLeftWheel.motorTorque = torque;
            frontRightWheel.motorTorque = torque;

            ApplyBraking(0f); // ❌ no braking
            return;
        }

        // 🚗 FORWARD
        if (verticalInput > 0)
        {
            frontLeftWheel.motorTorque = torque;
            frontRightWheel.motorTorque = torque;

            float brake = isBraking ? brakeForce : 0f;
            ApplyBraking(brake);
        }
        else
        {
            // 🚫 No input → light brake
            ApplyBraking(brakeForce * 0.5f);

            frontLeftWheel.motorTorque = 0;
            frontRightWheel.motorTorque = 0;
        }
    }

    void ApplyBraking(float brake)
    {
        frontLeftWheel.brakeTorque = brake;
        frontRightWheel.brakeTorque = brake;
        rearLeftWheel.brakeTorque = brake;
        rearRightWheel.brakeTorque = brake;
    }

    void HandleSteering()
    {
        float speed = rb.linearVelocity.magnitude;

        float speedFactor = Mathf.Clamp01(speed / maxSpeed);

        float allowedSteer = Mathf.Lerp(maxSteerAngle, minSteerAngle, speedFactor);

        float targetSteer = Mathf.Clamp(horizontalInput, -1f, 1f) * allowedSteer;

        frontLeftWheel.steerAngle = Mathf.Lerp(
            frontLeftWheel.steerAngle,
            targetSteer,
            Time.fixedDeltaTime * steerSmoothness
        );

        frontRightWheel.steerAngle = Mathf.Lerp(
            frontRightWheel.steerAngle,
            targetSteer,
            Time.fixedDeltaTime * steerSmoothness
        );
    }

    void ApplyDownforce()
    {
        rb.AddForce(-transform.up * downforce);
    }

    void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheel, frontLeftTransform);
        UpdateSingleWheel(frontRightWheel, frontRightTransform);
        UpdateSingleWheel(rearLeftWheel, rearLeftTransform);
        UpdateSingleWheel(rearRightWheel, rearRightTransform);
    }

    void UpdateSingleWheel(WheelCollider wheel, Transform trans)
    {
        Vector3 pos;
        Quaternion rot;
        wheel.GetWorldPose(out pos, out rot);

        trans.position = pos;
        trans.rotation = rot;
    }

    void LimitSpeed()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}