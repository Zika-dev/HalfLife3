using UnityEngine;

public class ShowArms : MonoBehaviour
{

    public GameObject elbow;
    public GameObject forearm;
    public GameObject Elbow;
    public GameObject Forearm;
    public GameObject ElbowSolver;
    public GameObject Pivot;
    public GameObject ClipPreventor;

   

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
            elbow.SetActive(true);
            forearm.SetActive(true);
            Elbow.SetActive(true);
            ElbowSolver.SetActive(true);
            Pivot.SetActive(true);
            Forearm.SetActive(true);
            ClipPreventor.SetActive(true);

        }
    }
}
