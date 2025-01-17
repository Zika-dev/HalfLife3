using UnityEngine;

public class TrashCanManager : MonoBehaviour
{
    public float attractionForce = 10f;
    public float attractionRadius = 5f;
    public bool machineActive;

    public string WhatKindOfTrash = "";
    public Interaction interaction;
    void Start()
    {
        GameObject prompt = GameObject.Find("Promt");
        if (prompt != null)
        {
            Transform mainCanvas = prompt.transform.Find("MainCanvas");
            if (mainCanvas != null)
            {
                Transform border = mainCanvas.Find("Border");
                if (border != null)
                {
                    Transform promptFrame = border.Find("PromptFrame");
                    if (promptFrame != null)
                    {
                        interaction = promptFrame.GetComponent<Interaction>();
                    }
                    else
                    {
                        Debug.Log(";/");
                    }
                }
                else
                {
                    Debug.Log("Hm :(");
                }
            }
            else
            {
                Debug.Log(":(");

            }
        }
        else
        {
            Debug.Log("asudhai");

        }

        if (interaction == null)
        {
            Debug.LogError("Interaction component not found!");
        }
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.CompareTag("Interactable"))
        {
            if (machineActive)
            {
                var tr = collision.gameObject.GetComponent<Trashh>();

                if (tr != null)
                {

                    string tash = tr.Trash;


                    Destroy(collision.gameObject);
                    if(tash != WhatKindOfTrash)
                        interaction.StartTextInteraction("Go Fuck Yourself", new Vector2(579, 426), 0.1f, new Vector2(400, 80));




                }
                else
                {
                    Debug.Log(":(");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }

    public void setMachineActive(bool input)
    {
        machineActive = input;
        Debug.Log(gameObject.name + " was just set " + machineActive);
    }
}
