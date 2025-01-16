using UnityEngine;

public class Doors : MonoBehaviour
{
    public GameObject player;
    public GameObject door1;
    public GameObject door2;
    public float doorPivotMultiplier;
    public float doorOpenDistance;
    public float moveSpeed = 1f;
    public float doorAutoOpenDistance;
    public bool doorOpen = false;
    public bool doorAutoOpen = false;

    private Vector3 door1StartPos;
    private Vector3 door2StartPos;
    private Vector3 doorOpenDistanceVec;
    private float factor = 0f;
    private float timeSinceLastActivation;
    private float doorRotationY;
    private float doorRotationX;
    private float distanceToPlayer;
    private bool isMoving;
    private bool isOpen;
    private bool checkSide;
    private Vector3 pivotRotation;

    private void Start()
    {
        door1StartPos = door1.transform.position;
        door2StartPos = door2.transform.position;
        pivotRotation = gameObject.transform.eulerAngles;
    }

    private void Update()
    {
        float angleToPlayer = Vector3.Angle(new Vector3(0, 1, 0), gameObject.transform.position - player.transform.position);
        distanceToPlayer = Vector3.Distance(gameObject.transform.position, player.transform.position) * doorPivotMultiplier;

        if (pivotRotation.x == 0)
        {
            doorRotationY = distanceToPlayer;
            doorRotationX = 0;
            checkSide = player.transform.position.x - gameObject.transform.position.x < 0;
            doorOpenDistanceVec = new Vector3(0, doorOpenDistance, 0);
        }
        else
        {
            doorRotationY = 0;
            doorRotationX = distanceToPlayer;
            checkSide = angleToPlayer < 90;
            doorOpenDistanceVec = new Vector3(-doorOpenDistance, 0, 0);
        }

        if (checkSide == true)
        {
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(doorRotationX, -doorRotationY, 0) + pivotRotation);
        }
        else
        {
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(-doorRotationX, doorRotationY, 0) + pivotRotation);
        }
    }

    private void FixedUpdate()
    {
        if (doorAutoOpen == true && distanceToPlayer < doorAutoOpenDistance)
        {
            doorOpen = true;
        }
        else if (doorAutoOpen == true && distanceToPlayer > doorAutoOpenDistance)
        {
            doorOpen = false;
        }

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
                door1.transform.position = Vector3.Lerp(door1StartPos, door1StartPos - doorOpenDistanceVec, factor);
                door2.transform.position = Vector3.Lerp(door2StartPos, door2StartPos + doorOpenDistanceVec, factor);
            }
            else
            {
                // Close doors
                door1.transform.position = Vector3.Lerp(door1StartPos - doorOpenDistanceVec, door1StartPos, factor);
                door2.transform.position = Vector3.Lerp(door2StartPos + doorOpenDistanceVec, door2StartPos, factor);
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
