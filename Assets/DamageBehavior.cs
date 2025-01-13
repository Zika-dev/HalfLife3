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
    private int health = 3;
    private SpriteRenderer spriteRenderer;
    public int IFrames = 5;
    private int startvalue = 0;
    private bool invincible = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            // this is temporary, but just destroying gameObject works for now
            Destroy(gameObject);
        }
        else if (health == 1)
        {
            ChangeObjectColor(HealthPoint1, Color.red);
            ChangeObjectColor(HealthPoint2, Color.red);
            ChangeObjectColor(HealthPoint3, Color.cyan);
        }
        else if (health == 2)
        {
            ChangeObjectColor(HealthPoint1, Color.red);
            ChangeObjectColor(HealthPoint2, Color.cyan);
            ChangeObjectColor(HealthPoint3, Color.cyan);
        }
        else if (health >= 3)
        {
            ChangeObjectColor(HealthPoint1, Color.cyan);
            ChangeObjectColor(HealthPoint2, Color.cyan);
            ChangeObjectColor(HealthPoint3, Color.cyan);
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
            Debug.Log("im bad");
            invincible = false;
            yield return null;

        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.CompareTag("DamageObject"))
        {
            if (!invincible)
            {
                startvalue = 0;
                StartCoroutine(TickTimer(IFrames));
                health--;
            }
        }
    }
    void ChangeObjectColor(GameObject obj, Color newColor)
    {
        
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

        spriteRenderer.color = newColor;

    }
}