using UnityEngine;

public class WireScript : MonoBehaviour
{
    public float plugForce;
    public float plugRadius;
    public float unplugForce = 10f; // Force required to unplug the wire
    public Vector2 connectionPoint = Vector2.up * 0.5f; // Point at the top of the box
    public float unplugRadius = 1f; // Distance at which the wire is considered unplugged

    private bool isPluggedIn = false;
    private Rigidbody2D pluggedWire;

    private void FixedUpdate()
    {
        Vector2 worldConnectionPoint = (Vector2)transform.position + connectionPoint;

        if (!isPluggedIn)
        {
            Collider2D[] interactables = Physics2D.OverlapCircleAll(worldConnectionPoint, plugRadius);

            foreach (Collider2D interactable in interactables)
            {
                if (interactable.gameObject.name.Contains("Wire"))
                {
                    Rigidbody2D rb = interactable.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 direction = worldConnectionPoint - rb.position;
                        rb.AddForce(direction.normalized * plugForce);
                    }
                }
            }
        }
        else if (pluggedWire != null)
        {
            // Check if there's enough force to unplug the wire
            Vector2 force = pluggedWire.linearVelocity * pluggedWire.mass;
            if (force.magnitude > unplugForce)
            {
                Unplug();
            }
            else if (Vector2.Distance(worldConnectionPoint, pluggedWire.position) > unplugRadius)
            {
                Unplug();
            }
            else
            {
                // Keep the wire in place at the connection point
                pluggedWire.MovePosition(worldConnectionPoint);
                pluggedWire.linearVelocity = Vector2.zero;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isPluggedIn && collision.gameObject.name.Contains("Wire"))
        {
            Vector2 worldConnectionPoint = (Vector2)transform.position + connectionPoint;
            Vector2 collisionPoint = collision.GetContact(0).point;

            // Check if the collision is close to the connection point
            if (Vector2.Distance(worldConnectionPoint, collisionPoint) < plugRadius)
            {
                PlugIn(collision.gameObject.GetComponent<Rigidbody2D>());
            }
        }
    }

    private void PlugIn(Rigidbody2D wire)
    {
        isPluggedIn = true;
        pluggedWire = wire;
        Debug.Log("Wire plugged in!");
    }

    private void Unplug()
    {
        isPluggedIn = false;
        pluggedWire = null;
        Debug.Log("Wire unplugged!");
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 worldConnectionPoint = (Vector2)transform.position + connectionPoint;

        // Draw plug radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(worldConnectionPoint, plugRadius);

        // Draw unplug radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(worldConnectionPoint, unplugRadius);

        // Draw connection point
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, worldConnectionPoint);
        Gizmos.DrawSphere(worldConnectionPoint, 0.1f);
    }
}