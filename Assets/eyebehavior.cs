using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class eyebehavior : MonoBehaviour
{
    private Vector3 mousePos;
    public Camera camera;
    public GameObject player;
    public Sprite eyesBaseSprite;
    public Sprite eyesPain;
    private SpriteRenderer test;

    void Start()
    {
        test = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        mousePos = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 50.0f));

        Vector3 posrelative = mousePos - gameObject.transform.position;

        
        if (posrelative.x > 0)
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, player.transform.position + new Vector3(1, 0, 0), 0.05f);
        }
        else
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, player.transform.position + new Vector3(-1, 0, 0), 0.05f);
        }

        if (posrelative.y > 0)
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, player.transform.position + new Vector3(0, 0.3f, 0), 0.05f);
        }
        else
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, player.transform.position + new Vector3(0, -0.3f, 0), 0.05f);
        }


    }



    //this function is for changing eyes to different emotions, just import a sprite and define a duration
    //example:  StartCoroutine(changeEyes(eyesPain, 200));
    public IEnumerator changeEyes(Sprite eyes, int duration)
    {
        test.sprite = eyes;
        float timer = 0;
        while (timer <= duration)
        {
            timer++;
            yield return null;
           
        }
        if (timer >= duration)
        {
            Debug.Log("changed back");
            test.sprite = eyesBaseSprite;
        }


    }

}
