using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class Doors : MonoBehaviour
{
    public GameObject player;
    public GameObject door1;
    public GameObject door2;
    public AudioSource audioSource;
    public AudioClip DoorSFX;
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
    private float doorTilt;
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
        if (pivotRotation.x == 0)
        {
            doorRotationY = doorTilt;
            doorRotationX = 0;
            checkSide = player.transform.position.x - gameObject.transform.position.x < 0;
            doorOpenDistanceVec = new Vector3(0, doorOpenDistance, 0);
            doorTilt = (gameObject.transform.position.x - player.transform.position.x) * doorPivotMultiplier;
        }
        else
        {
            doorRotationY = 0;
            doorRotationX = doorTilt;
            checkSide = player.transform.position.y - gameObject.transform.position.y < 0;
            doorOpenDistanceVec = new Vector3(-doorOpenDistance, 0, 0);
            doorTilt = (gameObject.transform.position.y - player.transform.position.y) * doorPivotMultiplier;
        }

        gameObject.transform.rotation = Quaternion.Euler(new Vector3(doorRotationX, -doorRotationY, 0) + pivotRotation);
    }

    private void FixedUpdate()
    {
        float distanceToPlayer = Vector3.Distance(gameObject.transform.position, player.transform.position) * 2;
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
            audioSource.PlayOneShot(DoorSFX, 0.5f);
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