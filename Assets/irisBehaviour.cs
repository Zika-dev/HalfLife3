using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class irisBehaviour : MonoBehaviour
{
    private Rigidbody2D irisRB;
    public enum EnemyState
    {
        Wandering,
        PathCorrection,
        Engaging,
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
    public List<Vector2> navPath = new List<Vector2>();
    public float maxSpeedForNextTarget = 0.5f;
    public float maxDistanceToTarget = 0.5f;
    private EnemyState currentState = EnemyState.Wandering;

    private Vector2 lastPointOnLine = Vector2.zero;
    private bool goingBack = false;

    [Range(0.0f, 180.0f)]
    public float angleThreshold = 5.0f;

    private int currentCheckpoint = 0;
    private int currentPathIndex = 0;

    [Header("Player Detection")]
    public Transform playerTransform;
    public float distanceToEngage = 10.0f;
    public Transform eyeTransform;
    public Vector2 combatPosition = new Vector2(0, 0);
    public float radius = 5.0f;
    bool inPosition = false;

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
    public float maxDistanceToCorrect = 2.0f;

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

    void lookAtObject(Vector2 targetPos)
    {
        Vector2 direction = ((Vector3)targetPos - transform.position).normalized;

        // PID controller for turning
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
        float torque = PIDController(targetAngle);

        // Clamp the torque to avoid excessive force
        torque = Mathf.Clamp(torque, -maximumTurnSpeed, maximumTurnSpeed);

        irisRB.AddTorque(torque);
    }

    bool goToPosition(Vector2 target, bool pathCorrection)
    {
        Vector2 direction = ((Vector3)target - transform.position).normalized;
        float angle = Vector2.SignedAngle(transform.up, direction);

        //Debug.Log(irisRB.linearVelocity.magnitude);

        Debug.DrawLine(transform.position, target, Color.red);
        Debug.DrawLine(startPos, target, Color.green);

        float maxDistance = pathCorrection ? maxDistanceToTarget : maxDistanceToCorrect;
        
        // Check if target is reached
        if (Vector2.Distance(transform.position, target) < maxDistance && irisRB.linearVelocity.magnitude < maxSpeedForNextTarget)
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
        lookAtObject(target);

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

            Vector2 u = target - startPos;
            float scalar = ((transform.position.x - startPos.x) * u.x + (transform.position.y - startPos.y) * u.y) / (Mathf.Pow(u.x, 2) + Mathf.Pow(u.y, 2)) * 2 + 0.2f;

            if (scalar > 1)
                scalar = 1;
            else if (scalar < 0)
                scalar = 0;

            Vector2 pointOnLine = new Vector2(startPos.x + scalar * u.x, startPos.y + scalar * u.y);

            Debug.DrawLine(transform.position, pointOnLine, Color.blue);

            if (angleInDegrees > pathCorrectionThresholdAngle)
            {
                

                currentState = EnemyState.PathCorrection;
                lastPointOnLine = pointOnLine;
                Debug.Log("Angle too large: " + angleInDegrees);
            }
        }

        return false;
    }

    bool followPath(List<Vector2> path)
    {
        if (path.Count > 0)
        {
            if (goToPosition(path[currentPathIndex], true))
            {
                ++currentPathIndex;

                if (currentPathIndex >= path.Count) // If we reached the end of the path
                {
                    currentPathIndex = 0;
                    return true;
                }
            }
        }

        return false;
    }

    List<Vector2> calculatePath(Transform targetPos)
    {
        if (targetPos == null)
            return new List<Vector2>();

        List<Vector2> path = new List<Vector2>();

        NavMesh.SamplePosition(transform.position, out NavMeshHit hitA, 10f, NavMesh.AllAreas);
        NavMesh.SamplePosition(targetPos.position, out NavMeshHit hitB, 10f, NavMesh.AllAreas);

        NavMeshPath navMeshPath = new NavMeshPath();
        if (NavMesh.CalculatePath(hitA.position, hitB.position, NavMesh.AllAreas, navMeshPath))
        {
            float distance = 0f;
            for (int i = 0; i < navMeshPath.corners.Length - 1; i++)
            {
                distance += (navMeshPath.corners[i] - navMeshPath.corners[i + 1]).magnitude;
                path.Add(navMeshPath.corners[i]);
                Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.green, 10f, true);
            }

            path.Add(navMeshPath.corners[navMeshPath.corners.Length]);

            Debug.Log($"Total distance {distance:F2}");
        }
        else
        {
            Debug.LogError("Mission Fail");
        }

        return path;
    }

    void wandering() // Go to each checkpoint
    {
        navPath = calculatePath(checkpoints[0]);

        if (followPath(navPath))
        {
            ++currentCheckpoint;

            if (currentCheckpoint >= checkpoints.Count)
            {
                currentCheckpoint = 0;
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

    void engaging()
    {
        if (combatPosition == Vector2.zero)
        {
            float r = radius * Mathf.Sqrt(Random.Range(0.0f, 1.0f));
            float theta = Random.Range(0.0f, 1.0f) * 2 * Mathf.PI;
            float x = playerTransform.position.x + r * MathF.Cos(theta);
            float y = playerTransform.position.y + r * MathF.Sin(theta);

            combatPosition = new Vector2(x, y);

            Debug.Log("Combat position: " + combatPosition);
        }

        Debug.DrawLine(transform.position, combatPosition, Color.yellow);

        Debug.Log(inPosition);

        if (!inPosition)
        {
            if (goToPosition(combatPosition, false))
                inPosition = true;

            return;
        }

        lookAtObject(playerTransform.position);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 direction = playerTransform.position - eyeTransform.position;
        RaycastHit2D hit = Physics2D.Raycast(eyeTransform.position, direction, distanceToEngage);
        Debug.DrawRay(eyeTransform.position, direction, Color.cyan);

        Debug.Log(checkpoints);

        if (hit.collider != null && currentState != EnemyState.Engaging)
        {
            if (hit.collider.CompareTag("Player"))
            {
                currentState = EnemyState.Engaging;
                Debug.Log("Engaging player");
            }
        }

        if (Input.GetKey(KeyCode.R)){
            gameObject.transform.position = checkpoints[3].transform.position;
            currentCheckpoint = 0;
            irisRB.linearVelocity = Vector2.zero;
            irisRB.angularVelocity = 0;
        }
        switch (currentState)
        {
            case EnemyState.Wandering:
                //wandering();
                break;

            case EnemyState.PathCorrection:
                pathCorrection();
                break;

            case EnemyState.Engaging:
                engaging();
                break;
        }
    }
}
