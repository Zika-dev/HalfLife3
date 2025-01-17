using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class playerController : MonoBehaviour
{
    
    [SerializeField] private FieldOfView fieldOfView;

    [Header("Movement and Physics")]
    private Rigidbody2D rb2D;
    public float thrust = 1.0f;
    public bool movementEnabled = true;
    [Space]
    [Header("Arm Control")]
    public Transform armTarget;
    public Transform armTip;
    public Transform rotationPoint;
    public GameObject elbowRaycast;
    [Space]
    [Header("Camera Control")]
    public Camera camera;
    public GameObject cameraTarget;
    public CinemachineCamera CinemachineCamera;
    public float maxCameraDistanceX;
    public float maxCameraDistanceY;
    public float minCameraDistanceX;
    public float minCameraDistanceY;
    private bool cameraLock;
    [Space]

    [Header("Mouse Input")]
    private Vector3 mousePos;

    [Header("Thrusters and Effects")]
    public ParticleSystem[] thrusters;
    public GameObject[] thrusterLights;
    public ParticleSystem targetParticles;
    public GameObject attractionParticles;
    private GameObject instantiatedAttractionParticles;
    public GameObject repellEffect;
    private GameObject instantiatedRepellEffect;
    [Space]
    [Header("Object Interaction")]
    public float range = 3.0f;
    public float lockRange = 0.1f;
    public float attractStrength = 1f;
    private bool lockedItem = false;
    private bool canRelease = false;
    private bool canAttract = true;
    private GameObject lockedObject;
    public Rigidbody2D lockedRigidbody2D;
    [Space]
    [Header("Collision and Preventers")]
    public LayerMask collisonIgnore;
    public GameObject clipPreventor;
    public GameObject pivot;
    [Space]
    [Header("Miscellaneous")]
    public Typing cutSceneScript;
   



    void Start()
    {
        CinemachineCamera.Follow = gameObject.transform;

        if (SettingsManager.Instance == null)
        {
            Debug.Log("SettingsManager.Instance is null");
            cameraLock = true;
        }
        else
        {
            cameraLock = SettingsManager.Instance.cameraLock;
        }

        rb2D = GetComponent<Rigidbody2D>();

        cameraTarget.transform.position = gameObject.transform.position;

        // Stop all particle systems

        foreach (var thruster in thrusters)
        {
            thruster.Stop();
        }
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
            //rigidbody2D.simulated = false;  // Disable physics simulation while holding
        }

        // Check if the player has released the right mouse button
        if (Input.GetMouseButtonUp(1))
        {
            canRelease = true;
            canAttract = true;
            if (instantiatedAttractionParticles != null)
            {
                Destroy(instantiatedAttractionParticles);
            }
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
                    if (distance < range && distance > lockRange)
                    {
                        Debug.DrawLine(armTip.position, hit.point, Color.magenta);

                        if (targetParticles == null)
                        {
                            instantiatedAttractionParticles = Instantiate(attractionParticles, obj.transform.position, obj.transform.rotation);
                            targetParticles = instantiatedAttractionParticles.GetComponent<ParticleSystem>();
                        }

                        targetParticles.transform.position = obj.transform.position;

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
                else if (targetParticles != null)
                {
                    targetParticles.Stop();
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
            lockedRigidbody2D = null;
            if (lockedItem && canRelease)
                lockedRigidbody2D = lockedObject.GetComponent<Rigidbody2D>();
            else // Shoot raycast from arm tip
            {
                RaycastHit2D hit = Physics2D.Raycast(armTip.position, direction, range);
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
                if(instantiatedRepellEffect == null) instantiatedRepellEffect = Instantiate(repellEffect, armTip.position, armTip.rotation);
                //Debug.Log(distance);
                lockedRigidbody2D.simulated = true;
                lockedRigidbody2D.linearVelocity = Vector2.zero;
                lockedRigidbody2D.AddForce(direction * 25.0f, ForceMode2D.Impulse);
                lockedObject = null;
                lockedItem = false;
                canAttract = false;  // Prevent immediate re-attraction
            }
        }
        else
        {
            instantiatedRepellEffect = null;
        }
    }

    void updateMovement()
    {
        // Lock rotation
        rb2D.rotation = 0;
        transform.eulerAngles = new Vector3(0, 0, 0);

        if (!movementEnabled) {
        foreach(var thruster in thrusters)
            {
                thruster.Stop();
            }

            return;
        };

        if (Input.GetKey(KeyCode.W))
        {
            rb2D.AddForce(transform.up * thrust);
            thrusters[3].Play();
            thrusterLights[3].SetActive(true);
        }

        if (Input.GetKey(KeyCode.S))
        {
            rb2D.AddForce(-transform.up * thrust);
            thrusters[1].Play();
            thrusterLights[1].SetActive(true);
        }

        if (Input.GetKey(KeyCode.A))
        {
            rb2D.AddForce(-transform.right * thrust);
            thrusters[2].Play();
            thrusterLights[2].SetActive(true);
        }

        if (Input.GetKey(KeyCode.D))
        {
            rb2D.AddForce(transform.right * thrust);
            thrusters[0].Play();
            thrusterLights[0].SetActive(true);
        }

        // Stop all particle systems
        if (!Input.GetKey(KeyCode.W))
        {
            thrusters[3].Stop();
            thrusterLights[3].SetActive(false);
        }

        if (!Input.GetKey(KeyCode.S))
        {
            thrusters[2].Stop();
            thrusterLights[2].SetActive(false);
        }

        if (!Input.GetKey(KeyCode.A))
        {
            thrusters[2].Stop();
            thrusterLights[2].SetActive(false);
        }

        if (!Input.GetKey(KeyCode.D))
        {
            thrusters[0].Stop();
            thrusterLights[0].SetActive(false);
        }
    }

    void updateCamera()
    {
        if (!cameraLock)
        {
            Vector3 cameraGoal = ClampCameraGoal(mousePos, transform.position);
            Vector3 targetPosition = Vector3.Lerp(gameObject.transform.position, cameraGoal, 0.75f);

            Vector3 offset = targetPosition - gameObject.transform.position;
            if (Mathf.Abs(offset.x) > minCameraDistanceX || Mathf.Abs(offset.y) > minCameraDistanceY)
            {
                cameraTarget.transform.position = targetPosition;
                CinemachineCamera.Follow = cameraTarget.transform;
                //print("Camera locked and following target");
            }
            else
            {
                CinemachineCamera.Follow = gameObject.transform;
                //print("Camera locked but too close to follow target");
            }
        }
        else
        {
            CinemachineCamera.Follow = gameObject.transform;
            //print("Camera unlocked");
        }
    }

    Vector3 ClampCameraGoal(Vector3 mousePos, Vector3 playerPos)
    {
        Vector3 cameraGoal = mousePos - playerPos;
        return new Vector3(
            Mathf.Clamp(cameraGoal.x, -maxCameraDistanceX, maxCameraDistanceX),
            Mathf.Clamp(cameraGoal.y, -maxCameraDistanceY, maxCameraDistanceY),
            0) + playerPos;
    }



    void Update()
    {
        if (!movementEnabled) return;

        updateArm();



        fieldOfView.SetOrigin(transform.position);

    }

    private void FixedUpdate()
    {
       /* if (cutSceneScript.doneTyping)
        {
            
        }
       */
        updateCamera();

        updateMovement();
    }
}
