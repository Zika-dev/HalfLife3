using UnityEngine;

public class Repelleffect : MonoBehaviour
{

    private Rigidbody2D rigidBody;
    private Vector3 thisPos;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        thisPos = this.transform.position;
        Vector2 velocity = rigidBody.linearVelocity;
        //rigidBody.linearVelocity = 
        Debug.Log(rigidBody.linearVelocity);
        Debug.DrawRay(thisPos, (rigidBody.linearVelocity + new Vector2(0, Random.Range(-2, 2))));
    }
}
