using UnityEngine;
using System.Collections.Generic;

public class SuckInManager : MonoBehaviour
{
    public float attractionForce = 10f;
    private HashSet<GameObject> suckedObjects = new HashSet<GameObject>();

   




    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Trashh>() != null)
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (!suckedObjects.Contains(collision.gameObject))
                {
                    suckedObjects.Add(collision.gameObject);
                    Debug.Log("Started sucking: " + collision.gameObject.name);
                }

                Vector2 direction = (transform.parent.position - collision.transform.position).normalized;
                rb.AddForce(direction * attractionForce, ForceMode2D.Force);
            }
            else
            {
                Debug.Log("Rigidbody2D not found.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (suckedObjects.Contains(collision.gameObject))
        {
            suckedObjects.Remove(collision.gameObject);
            Debug.Log("Stopped sucking: " + collision.gameObject.name);
        }
    }
}
