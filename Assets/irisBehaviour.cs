using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class SawEnemy : MonoBehaviour
{

    public Transform target;
    private float distanceFromTarget;

    public Transform body; // Body of the robot
    public Rigidbody2D rb; // Body rigidbody

    public float aimSpeed = 5.0f; // Speed at which the arm aims at the player
    public float maxAimDistance = 60.0f; // Maximum distance the arm can aim at the player
    public float maxFollowDistance = 200.0f; // Maximum distance the robot can follow the player
    public float movementSpeed = 20.0f; // Speed at which the enemy moves
    public float turnSpeed = 5.0f; // Speed at which the enemy turns
    public float targetAngularVelocity = 1f; // Angular velocity of the enemy

    public Transform forearm;
    public HingeJoint2D forearmJoint;
    public Transform axle; // Forearm joint location

    public Transform muzzle; // Muzzle of the gun
    public LayerMask hitLayers;

    public Rigidbody2D[] thrusters;
    private float angleDifference;

    private void RotateTowardsTarget(float angleDifference)
    {
        // Threshold to determine when to stop rotating
        float rotationThreshold = 4f; // Adjust as needed

        // Calculate the current rotational speed (angular velocity around the Y-axis)
        float currentAngularSpeed = rb.angularVelocity;

        if (Mathf.Abs(angleDifference) > rotationThreshold)
        {
            // Calculate the scaled thrust force based on angle difference
            float thrustScale = Mathf.Clamp01(Mathf.Abs(angleDifference) / 180.0f); // Scale between 0 and 1
            float thrustForce = turnSpeed * thrustScale;

            // Check if the current angular speed is below the target speed
            if (Mathf.Abs(currentAngularSpeed) < targetAngularVelocity)
            {
                // Apply thrust in the appropriate direction
                if (angleDifference > 0)
                {
                    applyThrustRight(thrustForce);
                }
                else
                {
                    applyThrustLeft(thrustForce);
                }
            }
        }
        else // Counter-thrust to slow down rotation
        {
            if (Mathf.Abs(currentAngularSpeed) > 2f) // Threshold for slowing down rotation
            {
                float counterThrustForce = turnSpeed * Mathf.Sign(currentAngularSpeed);
                if (currentAngularSpeed > 0)
                {
                    applyThrustRight(counterThrustForce);
                }
                else
                {
                    applyThrustLeft(-counterThrustForce);
                }
            }
        }
    }

    void applyThrustForwards(float thrustForce)
    {
        //thrusters[0].AddForce(thrusters[0].transform.up * thrustForce, ForceMode2D.Force);
        //thrusters[1].AddForce(thrusters[1].transform.up * thrustForce, ForceMode2D.Force);
        //Debug.DrawLine(thrusters[0].transform.position, thrusters[0].transform.position + -(thrusters[0].transform.up * thrustForce), Color.green);
        //Debug.DrawLine(thrusters[1].transform.position, thrusters[1].transform.position + -(thrusters[1].transform.up * thrustForce), Color.green);

        thrustForce *= 0.02f;
        rb.AddForce(body.forward * thrustForce, ForceMode2D.Force);
    }

    void applyThrustBackwards(float thrustForce)
    {
        //thrusters[2].AddForce(-thrusters[2].transform.up * thrustForce, ForceMode2D.Force);
        //thrusters[3].AddForce(-thrusters[3].transform.up * thrustForce, ForceMode2D.Force);
        //Debug.DrawLine(thrusters[2].transform.position, thrusters[2].transform.position + thrusters[2].transform.up * thrustForce, Color.green);
        //Debug.DrawLine(thrusters[3].transform.position, thrusters[3].transform.position + thrusters[3].transform.up * thrustForce, Color.green);

        thrustForce *= 0.02f;
        rb.AddForce(-body.forward * thrustForce, ForceMode2D.Force);
    }

    void applyThrustRight(float thrustForce)
    {
        //thrusters[0].AddForce(thrusters[0].transform.up * thrustForce, ForceMode2D.Force);
        //thrusters[3].AddForce(-thrusters[3].transform.up * thrustForce, ForceMode2D.Force);
        //Debug.DrawLine(thrusters[0].transform.position, thrusters[0].transform.position + -(thrusters[0].transform.up * thrustForce), Color.green);
        //Debug.DrawLine(thrusters[3].transform.position, thrusters[3].transform.position + thrusters[3].transform.up * thrustForce, Color.green);

        thrustForce *= 0.02f;
        rb.AddTorque(-thrustForce, ForceMode2D.Force);
    }

    void applyThrustLeft(float thrustForce)
    {
        //thrusters[1].AddForce(thrusters[1].transform.up * thrustForce, ForceMode2D.Force);
        //thrusters[2].AddForce(-thrusters[2].transform.up * thrustForce, ForceMode2D.Force);
        //Debug.DrawLine(thrusters[1].transform.position, thrusters[1].transform.position + -(thrusters[1].transform.up * thrustForce), Color.green);
        //Debug.DrawLine(thrusters[2].transform.position, thrusters[2].transform.position + thrusters[2].transform.up * thrustForce, Color.green);

        thrustForce *= 0.02f;
        rb.AddTorque(thrustForce, ForceMode2D.Force);
    }

    void FixedUpdate()
    {
        // Get the direction to the player
        distanceFromTarget = Vector2.Distance(target.position, body.position);


        if (distanceFromTarget < maxFollowDistance)
        {
            if (distanceFromTarget < maxAimDistance)
                updateGun();

            updateRotation();

            updateMovement();
        }
    }

    private void updateGun()
    {
        float targetAngle = Mathf.Atan2(target.position.y - axle.position.y, target.position.x - axle.position.x) * Mathf.Rad2Deg;

        // Get the current rotation angle of the forearm in degrees
        float currentAngle = forearmJoint.transform.eulerAngles.z;

        // Calculate the angle difference
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);

        Debug.DrawLine(axle.position, target.position, Color.red);

        // Use the motor to rotate the arm towards the target
        if (Mathf.Abs(angleDifference) > 5)
        {
            forearmJoint.useMotor = true;
            JointMotor2D motor = forearmJoint.motor;
            motor.motorSpeed = aimSpeed * -Mathf.Sign(angleDifference);
            motor.maxMotorTorque = 1000;
            forearmJoint.motor = motor;
        }
        else
        {
            JointMotor2D motor = forearmJoint.motor;
            motor.motorSpeed = 0;
            forearmJoint.motor = motor;
        }
    }

    private void updateRotation()
    {
        //Rotate robot towards player
        Vector2 directionToPlayer = (target.position - body.position).normalized;

        Debug.DrawLine(body.position, target.position, Color.red);

        Vector3 currentDirection = body.up;

        // Calculate the angle difference
        angleDifference = -Vector2.SignedAngle(currentDirection, directionToPlayer);

        Debug.DrawRay(body.position, directionToPlayer, Color.green);

        // Use the angle difference and current angular velocity to control thrusters
        RotateTowardsTarget(angleDifference);
    }

    private void updateMovement()
    {
        // Calculate the direction and desired velocity
        Vector2 directionToPlayer = (target.position - body.position).normalized;
        Vector2 desiredVelocity = directionToPlayer * movementSpeed;

        // Calculate the current velocity of the object
        Vector2 currentVelocity = rb.linearVelocity;

        // Calculate the velocity difference (only consider the perpendicular component for stabilization)
        Vector2 velocityDifference = currentVelocity - desiredVelocity;

        // Calculate and apply stabilization force
        Vector2 stabilizationForce = -velocityDifference; // Set stabilization strength as needed
        rb.AddForce(stabilizationForce, ForceMode2D.Force);

        Debug.DrawLine(body.position, new Vector2(body.position.x, body.position.y) + stabilizationForce, Color.blue);

        Debug.Log(angleDifference);

        // Standard forward/backward movement
        if (Mathf.Abs(angleDifference) < 20.0f)
        {
            if (distanceFromTarget > 30.0f)
            {
                applyThrustForwards(distanceFromTarget / movementSpeed);
                Debug.Log("Moving forwards");
            }
            if (distanceFromTarget < 20.0f)
            {
                applyThrustBackwards(50.0f);
                Debug.Log("Moving backwards");
            }
        }
    }
}
