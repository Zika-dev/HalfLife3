using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FemaleConnector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject connectedTo;

    private Vector2 newTransform;
    public Quaternion newRotation;

    public playerController _playerController;

    public Vector2 newOffset;

    public List<string> acceptedCables;

    void Start()
    {
        connectedTo = null;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (acceptedCables.Contains(collision.name))
        {
            connectedTo = collision.gameObject;
            newTransform = (Vector2)transform.position - newOffset;
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
