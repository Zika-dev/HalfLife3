using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;

public class ChargingPod : MonoBehaviour
{
    public DamageBehavior damageBehavior;
    public bool startopen = true;
    public float resetdistance = 10;
    public GameObject podIdle;
    public GameObject podCharging;
    public GameObject player;
    public GameObject chargingPoles;
    public Transform armRestPos;
    public Transform exitPoint;

    public AudioSource audioSource;
    public AudioClip ChargePodGas;

    public HingeJoint2D armLeft;
    public HingeJoint2D armRight;

    public BoxCollider2D beginChargeZone;

    public playerController PlayerController;

    public Animator podDoor;

    public ParticleSystem smoke;

    bool playerLocked = false;
    bool playerExiting = false;

    float timeWent;
    bool doorClosed;

    private void openArms()
    {
        
        // Rotate arms using motor
        JointMotor2D motor = armLeft.motor;
        armLeft.useLimits = true;
        motor.motorSpeed = 100;
        motor.maxMotorTorque = 100000;
        armLeft.motor = motor;

        motor = armRight.motor;
        armRight.useLimits = true;
        motor.motorSpeed = -100;
        motor.maxMotorTorque = 100000;
        armRight.motor = motor;
    }

    private void closeArms()
    {
       
        // Rotate arms using motor
        JointMotor2D motor = armLeft.motor;
        armLeft.useLimits = false;
        motor.motorSpeed = -100;
        motor.maxMotorTorque = 50;
        armLeft.motor = motor;

        motor = armRight.motor;
        armRight.useLimits = false;
        motor.motorSpeed = 100;
        motor.maxMotorTorque = 50;
        armRight.motor = motor;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (startopen == true)
        {
            openArms();
        }
        
        smoke.Stop();

        podIdle.SetActive(true);
        podCharging.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Check if the player is in the charging zone
        if (!playerLocked && !playerExiting) { 
            if (beginChargeZone.IsTouching(player.GetComponent<Collider2D>()))
            {
                PlayerController.movementEnabled = false;
                PlayerController.armTarget.position = Vector3.Lerp(PlayerController.armTarget.position, armRestPos.position, 0.05f);
                // Pull player towards charging poles
                player.transform.position = Vector3.Lerp(player.transform.position, chargingPoles.transform.position, 0.05f);

                // Debug.Log(player.GetComponent<Rigidbody2D>().linearVelocity.magnitude);

                // If the players velocity is less than 0.1, lock the player in place
                if (player.GetComponent<Rigidbody2D>().linearVelocity.magnitude < 0.1)
                {
                  
                    playerLocked = true;
                    podDoor.SetBool("PlayerLocked", true);
                }
            }
        }
        
       

        // If the player is locked, start charging
        if (playerLocked && !playerExiting)
        {
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            closeArms();
        }
    }

    void Update()
    {
        if (doorClosed && timeWent <= 4.5)
        {
            timeWent += Time.deltaTime;
        }
        else if (doorClosed && timeWent >= 4.5)
        {
            doorFullyOpen();
        }


        if (Input.GetKeyDown(KeyCode.Space) && playerLocked)
        {
            podDoor.SetBool("PlayerLocked", false);
          
            doorClosed = true;
            playerExiting = true;
        }


        float distance = Vector2.Distance(gameObject.transform.position, player.transform.position);
        
        if (distance >= resetdistance && playerExiting == true)
        {
            playerExiting = false;
        }
    }

    IEnumerator stopSmoke()
    {
        yield return new WaitForSeconds(2); // Waits for 2 seconds
        
        smoke.Stop();
    }

    public void doorFullyClosed()
    {
        // Start charging
        if (playerLocked) {
            podIdle.SetActive(false);
            podCharging.SetActive(true);
            smoke.Play();

            audioSource.PlayOneShot(ChargePodGas, 0.5f);

            damageBehavior.health = 3;
            StartCoroutine(stopSmoke());

            timeWent = 0f;
        }
    }

    public void doorFullyOpen()
    {
        // Stop charging
        podIdle.SetActive(true);
        podCharging.SetActive(false);


        openArms();

        playerLocked = false;
        PlayerController.movementEnabled = true;


    }

   
}
