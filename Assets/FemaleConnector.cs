using System;
using UnityEngine;
using UnityEngine.UIElements;

public class FemaleConnector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    GameObject connectedTo;
    public Transform plugPos;

    private Vector2 newTransform;
    private Quaternion newRotation;

    void Start()
    {
        connectedTo = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collided with " + collision.name);
        if (collision.name == "MaleConnector")
        {
            Debug.Log("Connected");
            connectedTo = collision.gameObject;

            newTransform = (Vector2)transform.position - new Vector2(0, 6.0f);
            newRotation = Quaternion.Euler(55, transform.rotation.y, transform.rotation.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (connectedTo != null)
        {
            connectedTo.transform.position = newTransform;
            connectedTo.transform.rotation = newRotation;
        }   
    }
}
