using UnityEngine;

public class Doors :MonoBehaviour
{
    public GameObject player;
    public GameObject door1;
    public GameObject door2;
    public float doorPivotMultiplier;
    public float doorOpenDistance;
    public float moveSpeed = 1f;

    private Vector3 door1StartPos;
    private Vector3 door2StartPos;
    private float factor = 0f;

    bool opening;
    private void Start()
    {
        door1StartPos = door1.transform.position;
        door2StartPos = door2.transform.position;
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(gameObject.transform.position, player.transform.position) * doorPivotMultiplier;

        if (Vector3.Angle(new Vector3(0,1,0), gameObject.transform.position - player.transform.position) < 90)
        {
            gameObject.transform.rotation = Quaternion.Euler(distanceToPlayer - 90, 0, 0);
        }
        else
        {
            gameObject.transform.rotation = Quaternion.Euler(-distanceToPlayer - 90, 0, 0);
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            opening = true;
        }
        if (opening) 
        {
            // Increase factor over time
            factor += Time.deltaTime * moveSpeed;
            factor = Mathf.Clamp01(factor);

            // Move doors
            door1.transform.position = Vector3.Lerp(door1StartPos, door1StartPos + new Vector3(doorOpenDistance * 0.5f, 0, 0), factor);
            door2.transform.position = Vector3.Lerp(door2StartPos, door2StartPos + new Vector3(-doorOpenDistance * 0.5f, 0, 0), factor);
        }
    }
}
