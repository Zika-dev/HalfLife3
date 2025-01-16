using UnityEngine;

public class PromtTrigger : MonoBehaviour
{
    public string text;
    public Vector2 location;
    public float typespeed;
    public Vector2 size;
    public Interaction interaction;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "robotBody")
        {
            interaction.StartTextInteraction(text, location, typespeed, size);

        }
    }

}
       
        
        
