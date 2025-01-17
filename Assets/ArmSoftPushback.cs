using UnityEngine;

public class WallPush : MonoBehaviour
{
    public float pushForce = 5f;

    void OnCollisionStay2D(Collision2D collision)
    {
        // Get the normal of the collision (direction pointing away from the wall)
        Vector2 collisionNormal = collision.contacts[0].normal;

        // Apply a force away from the wall
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(collisionNormal * pushForce, ForceMode2D.Force);
        }
    }
}