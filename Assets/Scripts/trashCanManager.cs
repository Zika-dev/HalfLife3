using UnityEngine;

public class TrashCanManager : MonoBehaviour
{
    public float attractionForce = 10f;
    public float attractionRadius = 5f;
    public bool machineActive;

    private void FixedUpdate()
    {
        if (machineActive)
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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Interactable"))
        {
            if (machineActive)
            {
                Destroy(collision.gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }

    public void setMachineActive(bool input)
    {
        machineActive = input;
        Debug.Log(gameObject.name + " was just set " + machineActive);
    }
}
