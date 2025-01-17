using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DamageBehavior : MonoBehaviour
{
    public GameObject HealthPoint1;
    public float health = 3;
    private SpriteRenderer spriteRenderer;
    public int IFrames = 5;
    private int startvalue = 0;
    private bool invincible = false;

    public bool Death = false;
    public GameObject playerCorpse;
    
    private bool triggerOnce;
    public Vector3 respawnLocation;
    public GameObject endscreen;
    public playerController PlayerController;
    public CinemachineCamera cinemachineCamera;


    public float MaxHealth = 100;
    public float CurrentHealth;
    private Color customcolor = new Color(0.3f, 0.87f, 1);
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentHealth = MaxHealth;
        

    }

    // Update is called once per frame
    void Update()
    {
        
        //debug code
        /*
        if (Input.GetKey(KeyCode.Space))
        {
            CurrentHealth = CurrentHealth - 1;

            float healthRatio = Mathf.Clamp01(CurrentHealth / MaxHealth);

            float r = Mathf.Lerp(1.0f, customcolor.r, healthRatio);
            float g = Mathf.Lerp(0.0f, customcolor.g, healthRatio);
            float b = Mathf.Lerp(0.0f, customcolor.b, healthRatio);

            ChangeObjectColor(HealthPoint1, new Color(r, g, b));
        }
        */

        if (Input.GetKey(KeyCode.Alpha1))
        {
            Debug.Log("current health: " + CurrentHealth);
            //respawn();
            

        }


        if (CurrentHealth <= 0 || Death == true)
        {
            triggerOnce = !triggerOnce;

            if (triggerOnce = false)
            {
                Debug.Log("player died");
                //triggerOnce = true;
                //Death = true;
                //CurrentHealth = 0;
                




            }
            
       

        }
    }



    // work in progress system for death and respawning
    /*
    public void respawn()
    {
        PlayerController.movementEnabled = false;

        gameObject.transform.position = respawnLocation;
        Instantiate(endscreen,respawnLocation + new Vector3(0,0,-40), gameObject.transform.rotation);

    }
    */

    IEnumerator TickTimer(float duration)
    {
        
        invincible = true;
        while (duration > startvalue)
        {
            ++startvalue;
            yield return null;

        }
        if (duration <= startvalue)
        {
            invincible = false;
            yield return null;

        }
        
    }

    public void DealDamage(float damage)
    {
        if (!invincible)
        {
            startvalue = 0;
            StartCoroutine(TickTimer(IFrames));


            CurrentHealth = CurrentHealth - damage;


            float healthRatio = Mathf.Clamp01(CurrentHealth / MaxHealth);

            float r = Mathf.Lerp(1.0f, customcolor.r, healthRatio);
            float g = Mathf.Lerp(0.0f, customcolor.g, healthRatio);
            float b = Mathf.Lerp(0.0f, customcolor.b, healthRatio);

            ChangeObjectColor(HealthPoint1, new Color(r, g, b));


            
        }
    }

    public void HealDamage(float healAmount, Vector3 chargepodLocation)
    {
        respawnLocation = chargepodLocation;
        Debug.Log("respawn updated at: " + respawnLocation);
        
        if(CurrentHealth + healAmount <= MaxHealth) { CurrentHealth = CurrentHealth + healAmount; }
        else { CurrentHealth = MaxHealth;}


        float healthRatio = Mathf.Clamp01(CurrentHealth / MaxHealth);

        float r = Mathf.Lerp(1.0f, customcolor.r, healthRatio);
        float g = Mathf.Lerp(0.0f, customcolor.g, healthRatio);
        float b = Mathf.Lerp(0.0f, customcolor.b, healthRatio);

        ChangeObjectColor(HealthPoint1, new Color(r, g, b));


    }
    
    
    
    
   
    void ChangeObjectColor(GameObject obj, Color newColor)
    {
        
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

        spriteRenderer.color = newColor;

    }
}
