using System.Collections.Generic;
using UnityEngine;

public class irisBehaviour : MonoBehaviour
{
    public List<Transform> checkpoints = new List<Transform>();
    private Rigidbody2D irisRB;

    public float turnSpeed = 10.0f;
    public float moveSpeed = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        irisRB = GetComponent<Rigidbody2D>();
    }

    void goForward()
    {
        irisRB.AddForce(transform.up * moveSpeed);
    }

    void turnLeft()
    {
        irisRB.AddTorque(turnSpeed);
        Debug.Log("Turning left");
    }

    void turnRight()
    {
        irisRB.AddTorque(-turnSpeed);
        Debug.Log("Turning left");
    }

    void followPath()
    {
        if (checkpoints.Count > 0)
        {
            Vector2 direction = (checkpoints[0].position - transform.position).normalized;
            float angle = Vector2.SignedAngle(transform.up, direction);
            Debug.Log(angle);
            Debug.DrawLine(transform.position, checkpoints[0].position, Color.red);
            if (angle > 0)
            {
                turnRight();
            }
            else
            {
                turnLeft();
            }
            //goForward(1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //followPath();

        turnLeft();
    }
}
