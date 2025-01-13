using Microsoft.Unity.VisualStudio.Editor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class irisBehaviour : MonoBehaviour
{
    private Rigidbody2D irisRB;
    public enum EnemyState
    {
        Wandering,
        PathCorrection,
    }

    [Header("Movement")]
    public float moveSpeed = 1.0f;

    [Range(0.0f, 1.0f)]
    public float breakStart = 0.75f;

    [Range(0.0f, 1.0f)]
    public float breakEnd = 0.90f;

    public float breakSpeed = 20f;
    private float targetSpeed = 0.0f;

    [Header("Targeting")]
    public List<Transform> checkpoints = new List<Transform>();
    public float maxSpeedForNextTarget = 0.5f;
    public float maxDistanceToTarget = 0.5f;
    private EnemyState currentState = EnemyState.Wandering;

    private Vector2 lastPointOnLine = Vector2.zero;

    [Range(0.0f, 180.0f)]
    public float angleThreshold = 5.0f;

    private int currentCheckpoint = 0;

    [Header("PID Controller (Turning)")]
    public float maximumTurnSpeed = 10.0f;
    public float pGain = 1.0f;  // Proportional gain
    public float iGain = 0.1f;  // Integral gain
    public float dGain = 0.5f;  // Derivative gain

    private float previousError = 0f;
    private float integral = 0f;

    [Header("Path correction")]

    [Range(0.0f, 180.0f)]
    public float pathCorrectionThresholdAngle = 20.0f;
    public float lookAheadFactor = 2f;
    public float lookAheadOffset = 0.2f;


    private Vector2 startPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        irisRB = GetComponent<Rigidbody2D>();

        startPos = transform.position;
    }

    public float calculateVelocity(float progress, float vMax, float vMin, float breakStart = 0.65f, float breakEnd = 0.90f, float breakSpeed = 10f)
    {
        if(progress >= breakEnd)
        {
            return moveSpeed / 2;
        }
        if (progress < breakStart)
        {
            return vMax;
        }
        else
        {
            float expTerm = Mathf.Exp(-breakSpeed * (progress - breakStart));
            return vMax * expTerm + vMin * (1 - expTerm);
        }
    }

    float PIDController(float targetAngle)
    {
        // Calculate the angular error
        float currentAngle = irisRB.rotation; // Rigidbody2D rotation is in degrees
        float error = Mathf.DeltaAngle(currentAngle, targetAngle); // Difference in angles [-180, 180]

        // Calculate PID terms
        integral += error * Time.fixedDeltaTime;  // Integral term
        float derivative = (error - previousError) / Time.fixedDeltaTime;  // Derivative term
        previousError = error;

        // Compute the control signal (torque)
        float torque = pGain * error + iGain * integral + dGain * derivative;

        return torque;
    }

    bool goToPosition(Vector2 target, bool pathCorrection)
    {
        Vector2 direction = ((Vector3)target - transform.position).normalized;
        float angle = Vector2.SignedAngle(transform.up, direction);

        //Debug.Log(irisRB.linearVelocity.magnitude);

        Debug.DrawLine(transform.position, target, Color.red);
        Debug.DrawLine(startPos, target, Color.green);
        
        // Check if target is reached
        if (Vector2.Distance(transform.position, target) < maxDistanceToTarget && irisRB.linearVelocity.magnitude < maxSpeedForNextTarget)
        {
            if (pathCorrection)
            {
                startPos = transform.position;
                Debug.Log("Start position updated: " + startPos);
            }
                
            Debug.Log("Target reached");
            return true;
        }

        // Check if angle is small enough to move forward
        if (Mathf.Abs(angle) < angleThreshold)
        {
            targetSpeed = moveSpeed;
            float currentDistance = Vector2.Distance(transform.position, target);
            float progress = 1 - currentDistance / Vector2.Distance(startPos, target);

            targetSpeed = calculateVelocity(progress, moveSpeed, -moveSpeed, breakStart, breakEnd, breakSpeed);

            if (irisRB.linearVelocity.magnitude < moveSpeed) // If we are not at max speed
            {
                if (targetSpeed < 0) // If we are breaking
                {
                    if(irisRB.linearVelocity.magnitude > moveSpeed / 2) // If we are not at half speed or less
                    {
                        irisRB.AddForce(transform.up * targetSpeed);
                    }
                    else
                    {
                        irisRB.AddForce(transform.up * moveSpeed / 2);
                    }
                }
                else // If we are accelerating
                {
                    irisRB.AddForce(transform.up * targetSpeed);
                }
                
            }
        }

        // PID controller for turning
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
        float torque = PIDController(targetAngle);

        // Clamp the torque to avoid excessive force
        torque = Mathf.Clamp(torque, -maximumTurnSpeed, maximumTurnSpeed);
        irisRB.AddTorque(torque);

        // Check if path correction is needed
        if (pathCorrection && Vector2.Distance(transform.position, target) >= 1.0)
        {
            // Calculate vectors pointing from C to A and C to B
            Vector2 vectorCA = (Vector2)transform.position - target;
            Vector2 vectorCB = startPos - target;

            // Normalize the vectors
            vectorCA.Normalize();
            vectorCB.Normalize();

            // Compute the dot product
            float dotProduct = Vector3.Dot(vectorCA, vectorCB);

            // Clamp the dot product to the valid range
            dotProduct = Mathf.Clamp(dotProduct, -1.0f, 1.0f);

            // Calculate the angle in radians and convert to degrees
            float angleInDegrees = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;



            if (angleInDegrees > pathCorrectionThresholdAngle)
            {
                Vector2 u = target - startPos;
                float scalar = ((transform.position.x - startPos.x) * u.x + (transform.position.y - startPos.y) * u.y) / (Mathf.Pow(u.x, 2) + Mathf.Pow(u.y, 2)) * 2 + 0.2f;

                if (scalar > 1)
                    scalar = 1;
                else if (scalar < 0)
                    scalar = 0;

                Vector2 pointOnLine = new Vector2(startPos.x + scalar * u.x, startPos.y + scalar * u.y);

                Debug.DrawLine(transform.position, pointOnLine, Color.blue);

                currentState = EnemyState.PathCorrection;
                lastPointOnLine = pointOnLine;
                Debug.Log("Angle too large: " + angleInDegrees);
            }
        }

        return false;
    }

    void followPath()
    {
        if (checkpoints.Count > 0)
        {
            if (goToPosition(checkpoints[currentCheckpoint].position, true))
            {
                ++currentCheckpoint;
                if (currentCheckpoint >= checkpoints.Count)
                {
                    currentCheckpoint = 0;
                }
            }
        }
    }

    void pathCorrection()
    {
        if (goToPosition(lastPointOnLine, false) == true)
        {
            currentState = EnemyState.Wandering;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.R)){
            gameObject.transform.position = checkpoints[3].transform.position;
            currentCheckpoint = 0;
            irisRB.linearVelocity = Vector2.zero;
            irisRB.angularVelocity = 0;
        }
        switch (currentState)
        {
            case EnemyState.Wandering:
                followPath();
                break;

            case EnemyState.PathCorrection:
                pathCorrection();
                break;
        }
    }
}
