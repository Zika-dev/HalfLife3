using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Doors :MonoBehaviour
{
    public GameObject player;
    public GameObject door1;
    public GameObject door2;
    public float doorPivotMultiplier;
    public float doorOpenDistance;
    public float moveSpeed = 1f;
    public bool doorOpen = false;

    private Vector3 door1StartPos;
    private Vector3 door2StartPos;
    private float factor = 0f;
    private float timeSinceLastActivation;
    private bool isMoving;
    private bool isOpen;
    private Vector3 pivotRotation;

    bool opening;

    private void Start()
    {
        door1StartPos = door1.transform.position;
        door2StartPos = door2.transform.position;
        pivotRotation = gameObject.transform.eulerAngles;
    }

    private void Update()
    {
        Debug.Log(pivotRotation);
        float distanceToPlayer = Vector3.Distance(gameObject.transform.position, player.transform.position) * doorPivotMultiplier;

        if (Vector3.Angle(new Vector3(0, 1, 0), gameObject.transform.position - player.transform.position) < 90 && pivotRotation.x == -90)
        {
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(distanceToPlayer - 90, 0, 0));
        }
        else if (Vector3.Angle(new Vector3(0, 1, 0), gameObject.transform.position - player.transform.position) > 90 && pivotRotation.x == -90)
        {
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(-distanceToPlayer - 90, 0, 0));
        }

        if (Vector3.Angle(new Vector3(1, 0, 0), gameObject.transform.position - player.transform.position) < 90 && pivotRotation.x == 0)
        {
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, distanceToPlayer, 0) + pivotRotation);
        }
        else if (Vector3.Angle(new Vector3(1, 0, 0), gameObject.transform.position - player.transform.position) > 90 && pivotRotation.x == -90)
        {
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, -distanceToPlayer, 0) + pivotRotation);
        }
    }

    private void FixedUpdate()
    {
        if (!isMoving && doorOpen != isOpen && Time.time - timeSinceLastActivation > moveSpeed)
        {
            isMoving = true;
            factor = 0f;
        }

        if (isMoving)
        {
            // Increase factor over time
            factor += Time.deltaTime * moveSpeed;
            factor = Mathf.Clamp01(factor);

            if (doorOpen)
            {
                // Open doors
                door1.transform.position = Vector3.Lerp(door1StartPos, door1StartPos + new Vector3(doorOpenDistance * 0.5f, 0, 0), factor);
                door2.transform.position = Vector3.Lerp(door2StartPos, door2StartPos + new Vector3(-doorOpenDistance * 0.5f, 0, 0), factor);
            }
            else
            {
                // Close doors
                door1.transform.position = Vector3.Lerp(door1StartPos + new Vector3(doorOpenDistance * 0.5f, 0, 0), door1StartPos, factor);
                door2.transform.position = Vector3.Lerp(door2StartPos + new Vector3(-doorOpenDistance * 0.5f, 0, 0), door2StartPos, factor);
            }

            // Check if the movement is complete
            if (factor >= 1f)
            {
                isMoving = false;
                isOpen = doorOpen;
                timeSinceLastActivation = Time.time;
            }
        }
    }
}
