using UnityEngine;

public class playerController : MonoBehaviour
{

    private Rigidbody2D rb2D;
    public Transform armTarget;
    public Transform armTip;
    public Transform rotationPoint;
    public Camera camera;

    public float thrust = 1.0f;

    public ParticleSystem thruster1;
    public ParticleSystem thruster2;
    public ParticleSystem thruster3;
    public ParticleSystem thruster4;

    private Vector3 mousePos;

    private bool lockedItem = false;
    GameObject lockedObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();

        // Stop all particle systems
        thruster1.Stop();
        thruster2.Stop();
        thruster3.Stop();
        thruster4.Stop();
    }


    void updateArm()
    {
        // Put target at mouse position
        mousePos = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f)); // mouse position in world space

        armTarget.position = mousePos;

        Vector2 direction = (armTip.position - rotationPoint.position).normalized;

        Debug.DrawLine(transform.position, armTarget.position, Color.red);
        Debug.DrawRay(rotationPoint.position, direction, Color.blue);

        if (lockedItem)
        {
            lockedObject.transform.position = armTip.position;
            Rigidbody2D rigidbody2D = lockedObject.GetComponent<Rigidbody2D>();
            rigidbody2D.simulated = false;
        }

        // Attract nearby objects with tag "Interactable" that has a rigidbody2D
        if (Input.GetMouseButton(1))
        {
            if (!lockedItem)
            {
                GameObject[] interactableObjects = GameObject.FindGameObjectsWithTag("Interactable");

                // Loop through each object and get their Transform component
                foreach (GameObject obj in interactableObjects)
                {
                    Transform objTransform = obj.transform;
                    Rigidbody2D rigidbody2D = obj.GetComponent<Rigidbody2D>();
                    if (rigidbody2D != null)
                    {
                        float distance = Vector2.Distance(armTip.position, objTransform.position);
                        if (distance < 5)
                        {
                            if (distance < 1) // Lock object to the arm tip
                            {
                                lockedItem = true;
                                lockedObject = obj;
                            }
                            else
                            {
                                Vector2 directionToObject = (objTransform.position - armTip.position);
                                Vector2 directionToObjectNormalized = directionToObject.normalized;
                                float force = 10.0f / distance;
                                rigidbody2D.AddForce(-directionToObjectNormalized * force);

                                Debug.DrawRay(armTip.position, directionToObject, Color.blue);
                            }
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButton(0)) // Shoot object away from the arm tip
        {
            if (lockedItem)
            {
                Rigidbody2D lockedRigidbody2D = lockedObject.GetComponent<Rigidbody2D>();
                lockedRigidbody2D.simulated = true;
                lockedRigidbody2D.AddForce(direction * 25.0f, ForceMode2D.Impulse);
                lockedItem = false;
            }
        }
    }

    void updateMovement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            rb2D.AddForce(transform.up * thrust);
            thruster4.Play();
        }

        if (Input.GetKey(KeyCode.S))
        {
            rb2D.AddForce(-transform.up * thrust);
            thruster2.Play();
        }

        if (Input.GetKey(KeyCode.A))
        {
            rb2D.AddForce(-transform.right * thrust);
            thruster3.Play();
        }

        if (Input.GetKey(KeyCode.D))
        {
            rb2D.AddForce(transform.right * thrust);
            thruster1.Play();
        }

        // Stop all particle systems
        if (!Input.GetKey(KeyCode.W))
        {
            thruster4.Stop();
        }

        if (!Input.GetKey(KeyCode.S))
        {
            thruster2.Stop();
        }

        if (!Input.GetKey(KeyCode.A))
        {
            thruster3.Stop();
        }

        if (!Input.GetKey(KeyCode.D))
        {
            thruster1.Stop();
        }

        // Lock rotation
        rb2D.rotation = 0;
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    void FixedUpdate()
    {
        updateMovement();

        updateArm();
    }
}
