using System;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UIElements;

public class playerController : MonoBehaviour
{
    [SerializeField] FieldOfView fieldOfView;

    private Rigidbody2D rb2D;
    public Transform armTarget;
    public Transform armTip;
    public Transform rotationPoint;
    public GameObject elbowRaycast;
    public Camera camera;

    public float thrust = 1.0f;
    public bool movementEnabled = true;

    ParticleSystem targetParticles;

    public ParticleSystem thruster1;
    public ParticleSystem thruster2;
    public ParticleSystem thruster3;
    public ParticleSystem thruster4;
    public GameObject thrusterLight1;
    public GameObject thrusterLight2;
    public GameObject thrusterLight3;
    public GameObject thrusterLight4;

    private Vector3 mousePos;

    public float range = 3.0f;
    public float lockRange = 0.1f;
    public float attractStrength = 1f;
    private bool lockedItem = false;
    GameObject lockedObject;

    private bool canRelease = false;
    private bool canAttract = true;

    public LayerMask collisonIgnore;

    public GameObject clipPreventor;
    public GameObject pivot;

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
        mousePos = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 50.0f));

        Vector3 currentDirection = (mousePos - armTarget.position).normalized;

        RaycastHit2D collision = Physics2D.Raycast(armTip.position, currentDirection, 0.25f, ~collisonIgnore);

        Debug.DrawRay(armTip.position, currentDirection, Color.red);

        if (collision.collider != null/* && collision.collider.gameObject.layer != 9 && collision.collider.gameObject.layer != 10*/)
        {
            //Debug.Log(collision.collider.gameObject.layer);
            // Smoothly move the solver along the collision surface
            armTarget.position = Vector3.Lerp(armTarget.position, collision.point, 0.05f);
        }
        else
        {
            // If no collision, move normally towards the mouse position
            armTarget.position = Vector3.Lerp(armTarget.position, mousePos, 0.05f);
        }

        float distanceToArmTip = Vector2.Distance(pivot.transform.position, armTip.position);

        float angle = Mathf.Atan2(armTip.position.y - transform.position.y, armTip.position.x - transform.position.x) * Mathf.Rad2Deg - 90;
        pivot.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        clipPreventor.transform.localScale = new Vector3(clipPreventor.transform.localScale.x, distanceToArmTip, 1);
        clipPreventor.transform.localPosition = new Vector3(0, distanceToArmTip / 2, 0);

        Vector2 direction = (armTip.position - rotationPoint.position).normalized;

        //Debug.DrawLine(transform.position, armTarget.position, Color.red);
        //Debug.DrawRay(rotationPoint.position, direction, Color.blue);

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
            targetParticles.Stop();
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
                    targetParticles = obj.GetComponentInChildren<ParticleSystem>();

                    float distance = Vector2.Distance(armTip.position, obj.transform.position);
                    // Apply attraction force if within range
                    if (distance < range && distance > lockRange)
                    {
                        Debug.DrawLine(armTip.position, hit.point, Color.magenta);
                        targetParticles.transform.position = hit.point;
                        if(!targetParticles.isPlaying)
                            targetParticles.Play();

                        Vector2 directionToObject = (obj.transform.position - armTip.position).normalized;
                        float force =  attractStrength / distance;
                        rigidbody2D.linearVelocity = -directionToObject * force;

                        Debug.DrawRay(armTip.position, directionToObject, Color.blue);
                    }
                    // Lock object if within very close range
                    else if (distance < lockRange)
                    {
                        targetParticles.Stop();
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
        if (Input.GetMouseButtonUp(1))
        {
            
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
            thrusterLight4.SetActive(true);
        }

        if (Input.GetKey(KeyCode.S))
        {
            rb2D.AddForce(-transform.up * thrust);
            thruster2.Play();
            thrusterLight2.SetActive(true);
        }

        if (Input.GetKey(KeyCode.A))
        {
            rb2D.AddForce(-transform.right * thrust);
            thruster3.Play();
            thrusterLight3.SetActive(true);
        }

        if (Input.GetKey(KeyCode.D))
        {
            rb2D.AddForce(transform.right * thrust);
            thruster1.Play();
            thrusterLight1.SetActive(true);
        }

        // Stop all particle systems
        if (!Input.GetKey(KeyCode.W))
        {
            thruster4.Stop();
            thrusterLight4.SetActive(false);
        }

        if (!Input.GetKey(KeyCode.S))
        {
            thruster2.Stop();
            thrusterLight2.SetActive(false);
        }

        if (!Input.GetKey(KeyCode.A))
        {
            thruster3.Stop();
            thrusterLight3.SetActive(false);
        }

        if (!Input.GetKey(KeyCode.D))
        {
            thruster1.Stop();
            thrusterLight1.SetActive(false);
        }
    }

    void Update()
    {
        if (!movementEnabled) return;

        updateArm();

        fieldOfView.SetOrigin(transform.position);
    }

    private void FixedUpdate()
    {
        updateMovement();
    }
}
