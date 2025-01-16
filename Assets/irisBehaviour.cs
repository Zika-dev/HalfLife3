using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
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
        Fleeing,
        Searching,
    }

    [Header("Movement")]
    public float moveSpeed = 5.0f;
    public float breakSpeed = 20f;
    public float distanceToBreak = 3.0f;
    public float minSpeed = 1.0f;

    private float targetSpeed = 0.0f;

    [Header("Targeting")]
    public List<Transform> checkpoints = new List<Transform>();
    public List<Vector2> navPath = new List<Vector2>();
    public float maxSpeedForNextTarget = 0.5f;
    public float maxDistanceToTarget = 0.5f;
    public EnemyState currentState = EnemyState.Wandering;

    private Vector2 startPos;
    private Vector2 lastPointOnLine = Vector2.zero;

    [Range(0.0f, 180.0f)]
    public float angleThreshold = 5.0f;

    private int currentCheckpoint = 0;
    private int currentPathIndex = 0;

    [Header("Combat")]
    public Transform playerTransform;
    public float distanceToEngage = 10.0f;
    public Transform eyeTransform;
    public float searchRadius = 5.0f;
    public int maxAttempts = 10;
    public float repositionDelay = 10.0f;
    public float minimumDistanceToPlayer = 2.0f;
    public float minimumDistanceFromEnemy = 6.0f;
    public float maximumDistanceFromEnemy = 4.0f;
    public Transform muzzle;
    public Transform lastSeenPosition;

    private Vector2 combatPosition = new Vector2(0, 0);

    [Header("PID Controller (Turning)")]
    public float maximumTurnSpeed = 10.0f;
    public float pGain = 1.0f;  // Proportional gain
    public float iGain = 0.1f;  // Integral gain
    public float dGain = 0.5f;  // Derivative gain

    private float previousError = 0f;
    private float integral = 0f;
    private float previousDerivative = 0f;

    [Header("Path correction")]

    [Range(0.0f, 180.0f)]
    public float pathCorrectionThresholdAngle = 20.0f;
    public float lookAheadFactor = 2f;
    public float lookAheadOffset = 0.2f;
    public float maxDistanceToCorrect = 2.0f;

    [Header("Arm")]
    public HingeJoint2D forearmJoint;
    public float aimSpeed = 100.0f;

    private JointMotor2D motor;


    public Transform resetPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        irisRB = GetComponent<Rigidbody2D>();

        startPos = transform.position;

        StartCoroutine(updatePath());

        motor = forearmJoint.motor;
    }

    public float calculateVelocity(float vMax, float vMin, float distanceToBreak, float distanceToTarget, float breakSpeed = 10f)
    {
        if(distanceToTarget > distanceToBreak)
        {
            return vMax;
        }
        else // Break speed increases exponentially with distance to target
        {
            float t = distanceToTarget / distanceToBreak; // Normalize distance (0 to 1)
            return Mathf.Lerp(vMin, vMax, Mathf.Exp(-breakSpeed * t));
        }
    }

    float PIDController(float targetAngle)
    {
        // Calculate the angular error
        float currentAngle = irisRB.rotation; // Rigidbody2D rotation is in degrees
        float error = Mathf.DeltaAngle(currentAngle, targetAngle); // Difference in angles [-180, 180]

        // Calculate PID terms
        integral += error * Time.fixedDeltaTime;  // Integral term
        integral = Mathf.Clamp(integral, -10f, 10f);  // Prevent windup

        float derivative = (error - previousError) / Time.fixedDeltaTime;  // Derivative term
        previousError = error;

        // Compute the control signal (torque)
        float torque = pGain * error + iGain * integral + dGain * derivative;

        // Clamp the torque output
        torque = Mathf.Clamp(torque, -maximumTurnSpeed, maximumTurnSpeed);

        // Anti-windup: Adjust integral if output is clamped
        if ((torque == maximumTurnSpeed && error > 0) || (torque == -maximumTurnSpeed && error < 0))
        {
            integral -= error * Time.fixedDeltaTime;  // Undo last integral update
        }

        return torque;
    }

    bool lookAtObject(Vector2 targetPos)
    {
        Vector2 direction = ((Vector3)targetPos - transform.position).normalized;

        // PID controller for turning
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
        float torque = PIDController(targetAngle);

        // Clamp the torque to avoid excessive force
        torque = Mathf.Clamp(torque, -maximumTurnSpeed, maximumTurnSpeed);

        irisRB.AddTorque(torque);

        return torque > 0.05 ? false : true;
    }

    bool goToPosition(Vector2 target, bool pathCorrection)
    {
        Vector2 direction = ((Vector3)target - transform.position).normalized;
        float angle = Vector2.SignedAngle(transform.up, direction);
        float distanceToTarget = Vector2.Distance(transform.position, target);

        //Debug.DrawLine(transform.position, target, Color.red);
        Debug.DrawLine(startPos, target, Color.green);

        float maxDistance = pathCorrection ? maxDistanceToTarget : maxDistanceToCorrect;
        
        // Check if target is reached
        if (distanceToTarget < maxDistance && irisRB.linearVelocity.magnitude < maxSpeedForNextTarget)
        {
            if (pathCorrection)
            {
                startPos = transform.position;
                Debug.Log("Start position updated: " + startPos);
            }
                
            //Debug.Log("Target reached");
            return true;
        }

        // Check if angle is small enough to move forward
        if (Mathf.Abs(angle) < angleThreshold)
        {
            targetSpeed = moveSpeed;

            targetSpeed = calculateVelocity(moveSpeed, -moveSpeed, distanceToBreak, distanceToTarget, breakSpeed);

            if (irisRB.linearVelocity.magnitude < moveSpeed) // If we are not at max speed
            {
                if (targetSpeed < 0) // If we are breaking
                {
                    if(irisRB.linearVelocity.magnitude > minSpeed) // If we are not at half speed or less
                    {
                        irisRB.AddForce(transform.up * targetSpeed);
                    }
                    else
                    {
                        irisRB.AddForce(transform.up * minSpeed);
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
        if (pathCorrection && distanceToTarget >= 1.0)
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
            float scalar = ((transform.position.x - startPos.x) * u.x + (transform.position.y - startPos.y) * u.y) / (Mathf.Pow(u.x, 2) + Mathf.Pow(u.y, 2)) * lookAheadFactor + lookAheadOffset;

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

    bool followPath(List<Vector2> path, bool pathCorrection)
    {
        if (path.Count > 0)
        {
            if (goToPosition(path[currentPathIndex], pathCorrection))
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

    List<Vector2> calculatePath(Vector2 targetPos)
    {
        if (targetPos == null)
            return new List<Vector2>();

        List<Vector2> path = new List<Vector2>();

        NavMesh.SamplePosition(transform.position, out NavMeshHit hitA, 10f, NavMesh.AllAreas);
        NavMesh.SamplePosition(targetPos, out NavMeshHit hitB, 10f, NavMesh.AllAreas);

        NavMeshPath navMeshPath = new NavMeshPath();
        if (NavMesh.CalculatePath(hitA.position, hitB.position, NavMesh.AllAreas, navMeshPath))
        {
            float distance = 0f;
            for (int i = 1; i < navMeshPath.corners.Length - 1; i++)
            {
                distance += (navMeshPath.corners[i] - navMeshPath.corners[i + 1]).magnitude;
                path.Add(navMeshPath.corners[i]);
                Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.magenta, 1f, true);
            }

            path.Add(navMeshPath.corners[navMeshPath.corners.Length - 1]);

            //Debug.Log($"Total distance {distance:F2}");
        }
        else
        {
            Debug.LogError("Mission Fail");
        }

        return path;
    }

    void wandering() // Go to each checkpoint
    {
        if (followPath(navPath, true))
        {
            ++currentCheckpoint;

            if (currentCheckpoint > checkpoints.Count - 1)
            {
                currentCheckpoint = 0;
            }

            navPath = calculatePath(checkpoints[currentCheckpoint].position);
        }
    }

    bool rotateArm()
    {
        float targetAngle = Mathf.Atan2(playerTransform.position.y - forearmJoint.transform.position.y, playerTransform.position.x - forearmJoint.transform.position.x) * Mathf.Rad2Deg;

        float currentAngle = forearmJoint.transform.eulerAngles.z;

        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
        
        // Use the motor to rotate the arm towards the target
        if (Mathf.Abs(angleDifference) > 5)
        {
            forearmJoint.useMotor = true;
            JointMotor2D motor = forearmJoint.motor;
            motor.motorSpeed = aimSpeed * -Mathf.Sign(angleDifference);
            motor.maxMotorTorque = aimSpeed;
            forearmJoint.motor = motor;

            return false;
        }
        else
        {
            JointMotor2D motor = forearmJoint.motor;
            motor.motorSpeed = 0;
            forearmJoint.motor = motor;

            return true;
        }
    }

    void pathCorrection()
    {
        if (goToPosition(lastPointOnLine, false) == true)
        {
            currentState = EnemyState.Wandering;
        }
    }

    Vector2 getCombatPosition(bool seePlayer, float searchRadius, float maximumDistanceFromEnemy, float minimumDistanceFromEnemy, float minimumDistancefromPlayer)
    {
        List<Vector2> combatPositions = new List<Vector2>();

        for (int i = 0; i < maxAttempts; ++i)
        {
            Vector2 randomOffset = Random.insideUnitCircle * searchRadius;
            Vector2 candidatePosition = (Vector2)playerTransform.position + randomOffset;

            if (NavMesh.SamplePosition(candidatePosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                combatPosition = hit.position;

                if (isPositionStrategic(combatPosition, seePlayer, maximumDistanceFromEnemy, minimumDistanceFromEnemy, minimumDistancefromPlayer))
                {
                    combatPositions.Add(combatPosition);
                    Debug.DrawLine(combatPosition, (Vector2)playerTransform.position, Color.green, 5.0f);
                }
                else
                {
                    Debug.DrawLine(combatPosition, (Vector2)playerTransform.position, Color.red, 5.0f);
                }
            }
            else
            {
                Debug.DrawLine(hit.position, (Vector2)playerTransform.position, Color.black, 5.0f);
            }
        }

        // Return position furthest from player
        if (combatPositions.Count > 0)
        {
            Vector2 furthestPosition = combatPositions[0];
            float maxDistance = Vector2.Distance(playerTransform.position, furthestPosition);

            for (int i = 1; i < combatPositions.Count; ++i)
            {
                float distance = Vector2.Distance(playerTransform.position, combatPositions[i]);
                Debug.DrawLine(playerTransform.position, combatPositions[i], Color.green, 5.0f);
                Debug.Log("Distance: " + distance);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    furthestPosition = combatPositions[i];
                }
            }

            Debug.DrawLine(furthestPosition, (Vector2)playerTransform.position, Color.cyan, 5.0f);

            return furthestPosition;
        }
        else
        {
            Debug.Log("Failed to find strategic position, resorting to this position");

            return transform.position;
        }
    }

    bool isPositionStrategic(Vector2 position, bool seePlayer, float maximumDistanceFromEnemy, float minimumDistanceFromEnemy, float minimumDistancefromPlayer)
    {
        if (seePlayer)
        {
            // Draw raycast from position and see if it hits player
            RaycastHit2D hit = Physics2D.Raycast(position, (Vector2)playerTransform.position - position);
            Debug.DrawRay(position, (Vector2)playerTransform.position - position, Color.red);

            // Something is blocking the path
            if (hit.collider != null && !hit.collider.CompareTag("Player"))
            {
                //Debug.Log(hit.collider);
                return false;
            }
        }

        // Close enough to enemy
        if (Vector2.Distance(transform.position, position) >= maximumDistanceFromEnemy)
        {
            //Debug.Log("Too far from enemy");
            return false;
        }

        // Far enough from enemy
        if (Vector2.Distance(transform.position, position) <= minimumDistanceFromEnemy)
        {
            //Debug.Log("Too close to enemy");
            return false;
        }


        // Far enough from player
        if (Vector2.Distance(playerTransform.position, position) <= minimumDistanceToPlayer)
        {
            //Debug.Log("Too close to player");
            return false;
        }


        return true;
    }

    void engaging()
    {

        if (combatPosition == Vector2.zero)
        {
            StartCoroutine(updateCombatPosition());
        }

        if (navPath.Count != 0)
        {
            if (followPath(navPath, false))
            {
                if (lookAtObject(playerTransform.position))
                {
                    rotateArm();
                }
            }
                
        }

        return;
    }

    void fleeing()
    {
        if (Vector2.Distance(combatPosition, playerTransform.position) < 15.0f)
            combatPosition = getCombatPosition(false, 100.0f, 500.0f, 15.0f, 10.0f);

        StopCoroutine(updateCombatPosition());

        if (navPath.Count != 0)
        {
            followPath(navPath, false);
        }
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        //if (Input.GetKey(KeyCode.R)) // For PID tuning
        //{
        //    Debug.Log("Resetting iris");
        //    transform.position = resetPoint.position;
        //    irisRB.angularVelocity = 0;
        //    irisRB.linearVelocity = Vector2.zero;
        //    irisRB.rotation = -90;
        //    return;
        //}

        Vector2 direction = playerTransform.position - eyeTransform.position;
        RaycastHit2D hit = Physics2D.Raycast(eyeTransform.position, direction, distanceToEngage);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player") && currentState == EnemyState.Wandering) // If we see the player
            {
                currentState = EnemyState.Engaging;
                lastSeenPosition = playerTransform;
            }

            if (!hit.collider.CompareTag("Player") && currentState == EnemyState.Engaging) // If we lost sight of the player
            {
                currentState = EnemyState.Searching;
            }
        }

        switch (currentState)
        {
            case EnemyState.Wandering:
                wandering();
                break;

            case EnemyState.PathCorrection:
                pathCorrection();
                break;

            case EnemyState.Engaging:
                engaging();
                break;

            case EnemyState.Fleeing:
                fleeing();
                break;

            case EnemyState.Searching:

                break;
        }
    }

    private IEnumerator updatePath()
    {
        while (true)
        {
            if (currentState == EnemyState.Wandering)
                navPath = calculatePath(checkpoints[currentCheckpoint].position);

            else if ((currentState == EnemyState.Engaging || currentState == EnemyState.Fleeing) && (combatPosition != Vector2.zero))
                navPath = calculatePath(combatPosition);

            else if (currentState == EnemyState.PathCorrection)
                currentState = EnemyState.Wandering;

            Debug.Log("Path updated");

            currentPathIndex = 0;

            yield return new WaitForSeconds(1.0f);
        }
    }

    private IEnumerator updateCombatPosition()
    {
        while(currentState == EnemyState.Engaging)
        {
            combatPosition = getCombatPosition(true, searchRadius, maximumDistanceFromEnemy, minimumDistanceFromEnemy, minimumDistanceToPlayer);
            Debug.Log("Combat position updated");

            yield return new WaitForSeconds(repositionDelay);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanceToEngage);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        if(currentState == EnemyState.Engaging)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerTransform.position, minimumDistanceToPlayer);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, maximumDistanceFromEnemy);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, minimumDistanceFromEnemy);
        }
    }
}
