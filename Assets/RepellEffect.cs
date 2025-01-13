using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UIElements;

public class Repelleffect : MonoBehaviour
{
    private GameObject repellEffect;
    public GameObject player;
    private playerController _playerController;
    private Rigidbody2D rigidBody;
    private Vector3 thisPos;
    private Rigidbody2D lockedRigidBodyCopy;
    public float randomness;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        repellEffect = gameObject;

        player = GameObject.Find("robotBody");
        _playerController = player.GetComponent<playerController>();

    }

    // Update is called once per frame
    void Update()
    {
        if (_playerController.lockedRigidbody2D == null && lockedRigidBodyCopy == null)
        {
            return;
        }

        lockedRigidBodyCopy = _playerController.lockedRigidbody2D;

        Vector2 vectorToTarget = rigidBody.transform.position - lockedRigidBodyCopy.transform.position;

        if (vectorToTarget.magnitude < 0.4f)
        {
           
            lockedRigidBodyCopy = null;
            Destroy(gameObject);
            return;
        }

        Vector2 randomVector = Vector2.zero;

        float cos = 0.0f;

        while(cos < 0.8f)
        {
            Debug.Log("cos: " + cos);
            randomVector = new (Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));
            cos = Vector2.Dot(vectorToTarget, randomVector) / (vectorToTarget.magnitude * randomVector.magnitude);
        }

        repellEffect.transform.position = repellEffect.transform.position - new Vector3(randomVector.x, randomVector.y, 0);
        //Debug.Log(thisPos);
        
        Debug.DrawRay(thisPos, (rigidBody.linearVelocity + new Vector2(0, Random.Range(-1,1))));
    }
}
