using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GasCanister : MonoBehaviour
{
    public float gasReleaseRate = 5f;
    public float maxSpeed = 20f;
    public float accelerationFactor = 1.5f;
    public float destructionSpeedThreshold = 10f;
    public GameObject gasOutlet;
    public GameObject Player;
    private Rigidbody2D rb;


    public float explosionRadius;
    public float explosionForce;

    private bool isReleasingGas = false;
    private float currentSpeed = 0f;

    Rigidbody2D playerRb2;
    DamageBehavior damageController;

    void Start()
    {
        playerRb2 = Player.transform.GetComponent<Rigidbody2D>();
        damageController = Player.GetComponent<DamageBehavior>();

        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (isReleasingGas)
        {
            currentSpeed += Time.deltaTime * gasReleaseRate;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

            rb.linearVelocity = transform.up * currentSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.relativeVelocity.magnitude > destructionSpeedThreshold)
        {
            SelfDestruct();
        }
        else
        {
            StartReleasingGas();
        }
    }

    private void StartReleasingGas()
    {
        isReleasingGas = true;
        Debug.Log("Gas release started!");
    }

    private void StopReleasingGas()
    {
        isReleasingGas = false;
        currentSpeed = 0f;
        Debug.Log("Gas release stopped!");
    }

    private void SelfDestruct()
    {
        Debug.Log("boutta blow");

        CreateExplosion();

        Destroy(gameObject);
    }

    private void CreateExplosion()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D collider in colliders)
        {
            GameObject struckObject = collider.gameObject;
            Debug.Log("Struck object: " + struckObject.name);

            Rigidbody2D hitRb = struckObject.GetComponent<Rigidbody2D>();
            if (hitRb != null && struckObject != gameObject)
            {
                Vector2 direction = struckObject.transform.position - transform.position;
                direction.Normalize();

                hitRb.AddForce(direction * explosionForce);
            }
            else if (struckObject.name == "ClipPreventor")
            {
                // we found the clip, so we now try and push
                Vector2 direction = Player.transform.position - transform.position;
                direction.Normalize();

                playerRb2.AddForce(direction * explosionForce);
                damageController.health -= 1;
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Set the color of the Gizmo
        Gizmos.DrawWireSphere(transform.position, explosionRadius); // Draw a wire sphere at the position of the gas canister
    }

}
