using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UIElements;

public class Repelleffect : MonoBehaviour
{
    private GameObject repellEffect;
    public GameObject player;
    private playerController _playerController;
    private Vector3 thisPos;
    private Rigidbody2D lockedRigidBodyCopy;
    public int steps = 10;
    private Vector2 originalDifference;
    public float randomAngle = 20.0f;
    private static int stepsDone = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        repellEffect = gameObject;

        player = GameObject.Find("robotBody");
        _playerController = player.GetComponent<playerController>();
        originalDifference = gameObject.transform.position - _playerController.lockedRigidbody2D.transform.position;

        stepsDone = 0;
    }

    public static Vector2 rotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;

        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        float xNew = cos * vector.x - sin * vector.y;
        float yNew = sin * vector.x + cos * vector.y;

        return new Vector2(xNew, yNew);
    }

    void customDestroy()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerController.lockedRigidbody2D == null && lockedRigidBodyCopy == null)
        {
            return;
        }

        lockedRigidBodyCopy = _playerController.lockedRigidbody2D;

        Vector2 vectorToTarget = gameObject.transform.position - lockedRigidBodyCopy.transform.position;

        Vector2 randomVector = rotateVector(originalDifference / steps, Random.Range(-randomAngle, randomAngle));

        if (stepsDone == steps)
        {
            repellEffect.transform.position = lockedRigidBodyCopy.transform.position;
            Invoke("customDestroy", 0.2f);
            return;
        }
        else
        {
            ++stepsDone;
            repellEffect.transform.position = repellEffect.transform.position - new Vector3(randomVector.x, randomVector.y, 0);
        }
    }
}
