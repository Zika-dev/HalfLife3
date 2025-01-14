using UnityEngine;

public class ObjectSpinner : MonoBehaviour
{
    public float rotationSpeed;
    public float torqueForce;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (IsSpinning())
        {
            float xRotation = Mathf.Sin(Time.time) * rotationSpeed * Time.deltaTime;
            float yRotation = Mathf.Cos(Time.time) * rotationSpeed * Time.deltaTime;

            transform.Rotate(xRotation, yRotation, 0);
        }
    }

    private bool IsSpinning()
    {
        
        return Mathf.Abs(rb.angularVelocity) > 0.001f;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 impactPoint = collision.GetContact(0).point;
        Vector2 objectCenter = rb.position;
        Vector2 impactDirection = impactPoint - objectCenter;
        float torqueDirection = Vector3.Cross(impactDirection, Vector3.forward).z;
        rb.AddTorque(torqueForce * torqueDirection, ForceMode2D.Impulse);
        Vector2 collisionNormal = collision.contacts[0].normal;
        transform.Rotate(collisionNormal.x * torqueForce * 0.1f, collisionNormal.y * torqueForce * 0.1f, 0);
    }
}
