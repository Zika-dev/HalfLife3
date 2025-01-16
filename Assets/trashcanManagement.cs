using UnityEngine;

public class trashcanManagement : MonoBehaviour
{
    public float attractionForce = 10f;
    public float attractionRadius = 5f;

    private void FixedUpdate()
    {
        Collider2D[] interactables = Physics2D.OverlapCircleAll(transform.position, attractionRadius, LayerMask.GetMask("Interactable"));

        foreach (Collider2D interactable in interactables)
        {
            Rigidbody2D rb = interactable.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (Vector2)transform.position - rb.position;
                rb.AddForce(direction.normalized * attractionForce);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Interactable"))
        {
            Destroy(collision.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}
