using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;


public class TrashCanManager : MonoBehaviour
{
    public float attractionForce = 10f;
    public float attractionRadius = 5f;
    public bool machineActive;
    public int MaxCogs;
    public int MaxFans;
    private int fansCollected;
    private int cogsCollected;
    private int mistakes;
    private int DoneWithTask;
    public string WhatKindOfTrash = "";
    public Interaction interaction;
    private Texture Texture;
    public DoneWithTask doneWithTask;
    void Start()
    {

        string[] guids = AssetDatabase.FindAssets("download (1) t:Texture");

        if (guids.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);

        }

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

                }

            }

        }





        GameObject[] cogs = Resources.FindObjectsOfTypeAll<GameObject>();
        int numberOfCogs = cogs.Count(cog => cog.name == "Cog");
        MaxCogs = numberOfCogs;
        GameObject[] fans = Resources.FindObjectsOfTypeAll<GameObject>();
        int numberOffans = fans.Count(fans => fans.name == "Fan");
        MaxFans = numberOffans;


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
                    doneWithTask.TaskCounter++;
                    if (tash == WhatKindOfTrash)
                    {
                        
                        if (WhatKindOfTrash == "Fan")
                        {
                            interaction.StartTextInteraction("object recognized: Fan\n object correctly sorted", new Vector2(1000, 400), 0.1f, new Vector2(700, 100));
                        }
                        if (WhatKindOfTrash == "Cog")
                        {
                            interaction.StartTextInteraction("object recognized: Cog\n object correctly sorted", new Vector2(1000, 400), 0.1f, new Vector2(700, 100));
                        }


                    }

                    else
                    {
                        interaction.StartTextInteraction("object incorrectly sorted \n this will negatively affect your end of day performance review", new Vector2(1000, 400), 0.1f, new Vector2(1000, 100));

                    }



                }
                else
                {
                    Debug.Log("error");
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
