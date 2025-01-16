using System.Collections;
using TMPro;
using UnityEngine;

public class Typing : MonoBehaviour
{
    public float TypingSpeed = 0.8f;
    public TextMeshProUGUI textLabel;
    private string text = "Booting....\n \n  All systems operational\n \n  Machine ID: RE-03,VER:3.26\n \n  Objective: maintenance\n  Location: main charging depot\n  Power: 100%\n  Status: awaiting assignemt\n \n  Releasing constraints\n \n  press: [space]";
    public bool doneTyping = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(typing());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && doneTyping == true)
        {
            Destroy(gameObject);


        }
    }



    IEnumerator typing()
    {
        bool includeArrow = true;
        for (int i = 0; i <= text.Length; i++)
        {
            textLabel.text = includeArrow ? $"> {text.Substring(0, i)}" : $"  {text.Substring(0, i)}";
            includeArrow = !includeArrow;

            if (i == text.Length)
            {
                doneTyping = true;
            }
            yield return new WaitForSeconds(TypingSpeed);
        }
        
    }
}
