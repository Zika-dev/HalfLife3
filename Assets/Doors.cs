using UnityEngine;

public class Doors :MonoBehaviour
{
    public GameObject player;
    public GameObject doorPivot;

    private void Start()
    {
        
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(gameObject.transform.position, player.transform.position);

        doorPivot.transform.rotation = Quaternion.Euler(distanceToPlayer - 90, 0, 0);
    }
}
