using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class enemyMovement : MonoBehaviour
{
    public enum EnemyState { Roaming, Engaged }

    [Header("General Settings")]
    public Transform player;
    public float detectionRange = 10f;
    public Transform shoulder;
    public Transform forearm;
    private Rigidbody2D shoulderRb;
    private Rigidbody2D forearmRb;

    [Header("Roaming Settings")]
    public Transform[] patrolPoints;
    public float roamingSpeed = 2f;
    public float waitTime = 1f;
    public float arrivalThreshold = 0.1f;

    [Header("Engaged Settings")]
    public float engagedSpeed = 3f;
    public float optimalShootingDistance = 5f;
    public float shootingRangeMargin = 1f;
    public float armRotationSpeed = 1f;
    public float shootingInterval = 1f;

    private EnemyState currentState = EnemyState.Roaming;
    private Rigidbody2D rb;
    private int currentPatrolIndex = 0;
    private float lastShotTime;
    private bool isInShootingPosition = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        shoulderRb = shoulder.GetComponent<Rigidbody2D>();
        forearmRb = forearm.GetComponent<Rigidbody2D>();
        StartCoroutine(StateMachine());
    }

    IEnumerator StateMachine()
    {
        while (true)
        {
            yield return StartCoroutine(currentState == EnemyState.Roaming ? RoamingBehavior() : EngagedBehavior());
            yield return new WaitForEndOfFrame();
            CheckStateTransition();
        }
    }

    void CheckStateTransition()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (currentState == EnemyState.Roaming && distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Engaged;
        }
        else if (currentState == EnemyState.Engaged && distanceToPlayer > detectionRange)
        {
            currentState = EnemyState.Roaming;
        }
    }

    IEnumerator RoamingBehavior()
    {
        print("We roam");
        if (patrolPoints.Length == 0) yield break;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        while (Vector2.Distance(rb.position, targetPoint.position) > arrivalThreshold)
        {
            Vector2 direction = (targetPoint.position - transform.position).normalized;
            rb.linearVelocity = direction * roamingSpeed;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(waitTime);

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    IEnumerator EngagedBehavior()
    {
        print("were engaged!");
        if (player == null) yield break;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        isInShootingPosition = Mathf.Abs(distanceToPlayer - optimalShootingDistance) <= shootingRangeMargin;

        if (!isInShootingPosition)
        {
            if (distanceToPlayer > optimalShootingDistance + shootingRangeMargin)
            {
                rb.linearVelocity = directionToPlayer * engagedSpeed;
            }
            else if (distanceToPlayer < optimalShootingDistance - shootingRangeMargin)
            {
                rb.linearVelocity = -directionToPlayer * engagedSpeed;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            Shoot();
        }

        if (shoulderRb != null && forearmRb != null)
        {
            Vector2 armDirection = player.position - shoulder.position;
            float targetAngle = Mathf.Atan2(armDirection.y, armDirection.x) * Mathf.Rad2Deg;

            float shoulderToPlayerDistance = Vector2.Distance(shoulder.position, player.position);

            bool shouldBendForearm = shoulderToPlayerDistance > optimalShootingDistance;

            if (shouldBendForearm)
            {
                float currentForearmAngle = forearm.rotation.eulerAngles.z;
                float forearmTargetAngle = targetAngle;

                float newForearmAngle = Mathf.LerpAngle(currentForearmAngle, forearmTargetAngle, Time.deltaTime * (armRotationSpeed * 2f));
                forearm.rotation = Quaternion.Euler(0, 0, newForearmAngle);
            }
            else
            {
                float currentShoulderAngle = shoulder.rotation.eulerAngles.z;
                float newShoulderAngle = Mathf.LerpAngle(currentShoulderAngle, targetAngle, Time.deltaTime * armRotationSpeed);
                shoulder.rotation = Quaternion.Euler(0, 0, newShoulderAngle);

                float currentForearmAngle = forearm.rotation.eulerAngles.z;
                float forearmTargetAngle = newShoulderAngle;

                float newForearmAngle = Mathf.LerpAngle(currentForearmAngle, forearmTargetAngle, Time.deltaTime * (armRotationSpeed * 0.5f));
                forearm.rotation = Quaternion.Euler(0, 0, newForearmAngle);
            }

            float finalForearmTargetAngle = targetAngle;
            float finalCurrentForearmAngle = forearm.rotation.eulerAngles.z;

            float finalNewForearmAngle = Mathf.LerpAngle(finalCurrentForearmAngle, finalForearmTargetAngle, Time.deltaTime * (armRotationSpeed * 2f));
            forearm.rotation = Quaternion.Euler(0, 0, finalNewForearmAngle);
        }

        yield return null;
    }

    void Shoot()
    {
        Debug.Log("Enemy shoots at player!");
    }
}
