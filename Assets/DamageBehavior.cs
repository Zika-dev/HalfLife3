using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DamageBehavior : MonoBehaviour
{
    public GameObject HealthPoint1;
    public GameObject HealthPoint2;
    public GameObject HealthPoint3;
    public float health = 3;
    private SpriteRenderer spriteRenderer;
    public int IFrames = 5;
    private int startvalue = 0;
    private bool invincible = false;

    public float MaxHealth = 100;
    private float CurrentHealth;
    public DamageObjectSettings damageObjectSettings;
    private Color customcolor = new Color(0.3f, 0.87f, 1);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentHealth = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            CurrentHealth = CurrentHealth - 1;

            float healthRatio = Mathf.Clamp01(CurrentHealth / MaxHealth);

            float r = Mathf.Lerp(1.0f, customcolor.r, healthRatio);
            float g = Mathf.Lerp(0.0f, customcolor.g, healthRatio);
            float b = Mathf.Lerp(0.0f, customcolor.b, healthRatio);

            ChangeObjectColor(HealthPoint1, new Color(r, g, b));
            Debug.Log(CurrentHealth);
        }



        
        if (CurrentHealth <= 0)
        {
            Debug.Log("death");
            
        }
        

    }


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
            Debug.Log(damage);
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
    
    
    
    
   
    void ChangeObjectColor(GameObject obj, Color newColor)
    {
        
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

        spriteRenderer.color = newColor;

    }
}
