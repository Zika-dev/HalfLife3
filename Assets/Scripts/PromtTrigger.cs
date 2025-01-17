using UnityEngine;

public class PromtTrigger : MonoBehaviour
{
    public string text;

    public Vector2 location;
    public float typespeed;
    public Vector2 size;
    private Interaction interaction;
  
    
    

    void Start()
    {
        GameObject prompt = GameObject.Find("Promt");
        if (prompt != null)
        {
            Transform mainCanvas = prompt.transform.Find("MainCanvas");
            if (mainCanvas != null)
            {
                Transform border = mainCanvas.Find("Border");
                if (border != null)
                {
                    Transform promptFrame = border.Find("PromptFrame");
                    if (promptFrame != null)
                    {
                        interaction = promptFrame.GetComponent<Interaction>();
                    }
                    else
                    {
                        Debug.Log(";/");
                    }
                }
                else
                {
                    Debug.Log("Hm :(");
                }
            }
            else
            {
                Debug.Log(":(");

            }
        }
        else
        {
            Debug.Log("asudhai");

        }

        if (interaction == null)
        {
            Debug.LogError("Interaction component not found!");
        }
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "robotBody")
        {
            interaction.StartTextInteraction(text, location, typespeed, size);
            Destroy(gameObject);


        }
       
    }

}
       
        
        
