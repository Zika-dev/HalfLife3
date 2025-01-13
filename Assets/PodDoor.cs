using UnityEngine;

public class PodDoor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void fullyClosed()
    {
        SendMessageUpwards("doorFullyClosed");
    }

    public void fullyOpen()
    {
        SendMessageUpwards("doorFullyOpen");
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
