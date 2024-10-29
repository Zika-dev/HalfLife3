using UnityEngine;

public class playerController : MonoBehaviour
{

    public Rigidbody2D rb2D;

    public float thrust = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            rb2D.AddForce(transform.up * thrust);
        }

        if (Input.GetKey(KeyCode.S))
        {
            rb2D.AddForce(-transform.up * thrust);
        }

        if (Input.GetKey(KeyCode.A))
        {
            rb2D.AddForce(-transform.right * thrust);
        }

        if (Input.GetKey(KeyCode.D))
        {
            rb2D.AddForce(transform.right * thrust);
        }

        // Lock rotation
        rb2D.rotation = 0;
        transform.eulerAngles = new Vector3(0, 0, 0);
    }
}
