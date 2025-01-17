using UnityEngine;

public class DoneWithTask : MonoBehaviour
{
    public float TaskCounter = 0;
    public GameObject trigger;
    public GameObject trigger2;
    public Doors doors1;
    public Doors doors2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      if (TaskCounter == 5 && trigger != null) 
      {
            trigger.SetActive(true); 
            trigger2.SetActive(true);

            doors1.doorAutoOpen = true;
            doors2.doorAutoOpen = true;


        }   
    }
}
