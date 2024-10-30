using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour
{

    private Rigidbody2D rb2D;
    public Transform armTarget;
    public Transform armTip;
    public Transform rotationPoint;
    public Camera camera;

    public float thrust = 1.0f;
    bool movementEnabled = true;

    public ParticleSystem thruster1;
    public ParticleSystem thruster2;
    public ParticleSystem thruster3;
    public ParticleSystem thruster4;

    private Vector3 mousePos;

    public float range = 2.0f;
    private bool lockedItem = false;
    GameObject lockedObject;

    private bool canRelease = false;
    private bool canAttract = true;

    public void disableMovement()
    {
        movementEnabled = false;
    }

    public void enableMovement()
    {
        movementEnabled = true;
    }

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
        // Update the target position to follow the mouse
        mousePos = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));
        // Lerp arm target position to mouse position
        armTarget.position = Vector3.Lerp(armTarget.position, mousePos, 0.05f);

        Vector2 direction = (armTip.position - rotationPoint.position).normalized;

        Debug.DrawLine(transform.position, armTarget.position, Color.red);
        Debug.DrawRay(rotationPoint.position, direction, Color.blue);

        if (lockedItem)
        {
            // Hold object at the arm tip
            lockedObject.transform.position = armTip.position;
            Rigidbody2D rigidbody2D = lockedObject.GetComponent<Rigidbody2D>();
            rigidbody2D.simulated = false;  // Disable physics simulation while holding
        }

        // Check if the player has released the right mouse button
        if (Input.GetMouseButtonUp(1))
        {
            canRelease = true;
            canAttract = true;
        }

        // Right mouse button pressed for attracting or releasing objects
        if (Input.GetMouseButton(1))
        {
            // Attraction mode if there's no locked item, attraction is allowed, and the mouse is still held
            if (!lockedItem && canAttract)
            {
                // Shoot raycast from arm tip
                RaycastHit2D hit = Physics2D.Raycast(armTip.position, direction, 10.0f);
                if (hit.collider != null && hit.collider.CompareTag("Interactable"))
                {
                    GameObject obj = hit.collider.gameObject;
                    Rigidbody2D rigidbody2D = obj.GetComponent<Rigidbody2D>();
                    if (rigidbody2D == null) return;

                    float distance = Vector2.Distance(armTip.position, obj.transform.position);

                    // Apply attraction force if within range
                    if (distance < range && distance > 1)
                    {
                        Vector2 directionToObject = (obj.transform.position - armTip.position).normalized;
                        float force = 10.0f / distance;
                        rigidbody2D.AddForce(-directionToObject * force);

                        Debug.DrawRay(armTip.position, directionToObject, Color.blue);
                    }
                    // Lock object if within very close range
                    else if (distance < 1)
                    {
                        lockedObject = obj;
                        lockedItem = true;
                        canRelease = false;  // Require mouse release before allowing release
                        Debug.Log("Object locked");
                    }
                }
            }
            else if (lockedItem && canRelease)  // Release locked object if allowed
            {
                Debug.Log("Release object");
                Rigidbody2D lockedRigidbody2D = lockedObject.GetComponent<Rigidbody2D>();
                lockedRigidbody2D.simulated = true;  // Re-enable physics
                lockedRigidbody2D.linearVelocity = Vector2.zero;
                lockedRigidbody2D.AddForce(direction * 1.0f, ForceMode2D.Impulse);
                lockedObject = null;
                lockedItem = false;
                canAttract = false;  // Require mouse release before re-attracting
            }
        }

        // Left mouse button to shoot the object away from the arm tip
        if (Input.GetMouseButton(0))
        {
            float distance = 0.0f;
            Rigidbody2D lockedRigidbody2D = null;
            if (lockedItem && canRelease)
                lockedRigidbody2D = lockedObject.GetComponent<Rigidbody2D>();
            else // Shoot raycast from arm tip
            {
                RaycastHit2D hit = Physics2D.Raycast(armTip.position, direction, 10.0f);
                if (hit.collider != null && hit.collider.CompareTag("Interactable"))
                {
                    GameObject obj = hit.collider.gameObject;
                    lockedRigidbody2D = obj.GetComponent<Rigidbody2D>();
                    if (lockedRigidbody2D == null) return;

                    distance = Vector2.Distance(armTip.position, obj.transform.position);
                }
            }
            if (lockedRigidbody2D != null && distance < range)
            {
                Debug.Log(distance);
                lockedRigidbody2D.simulated = true;
                lockedRigidbody2D.linearVelocity = Vector2.zero;
                lockedRigidbody2D.AddForce(direction * 25.0f, ForceMode2D.Impulse);
                lockedObject = null;
                lockedItem = false;
                canAttract = false;  // Prevent immediate re-attraction
            }
        }
    }

    void updateMovement()
    {
        // Lock rotation
        rb2D.rotation = 0;
        transform.eulerAngles = new Vector3(0, 0, 0);

        if (!movementEnabled) {
            thruster1.Stop();
            thruster2.Stop();
            thruster3.Stop();
            thruster4.Stop();

            return;
        };

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
    }

    void Update()
    {
        if (!movementEnabled) return;

        updateArm();
    }

    private void FixedUpdate()
    {
        updateMovement();
    }
}
