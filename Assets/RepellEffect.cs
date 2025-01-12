using UnityEngine;

public class Repelleffect : MonoBehaviour
{
    public GameObject repellEffect;

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

        //Instantiate(repellEffect, this.transform.position, this.transform.rotation);

        //repellEffect.transform.position = repellEffect.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
        Debug.Log(thisPos);
        
        Debug.DrawRay(thisPos, (rigidBody.linearVelocity + new Vector2(0, Random.Range(-1,1))));
    }
}
