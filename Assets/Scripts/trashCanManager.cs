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
    public string WhatKindOfTrash = "";
    public Interaction interaction;
    private Texture Texture;
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
                  
                    if(tash == WhatKindOfTrash)
                    {
                        if (WhatKindOfTrash == "Fan")
                        {
                            fansCollected++;
                            interaction.StartTextInteraction($"Nice, there are {MaxFans - fansCollected} left!", new Vector2(579, 426), 0.1f, new Vector2(400, 80));
                        }
                        if(WhatKindOfTrash == "Cog")
                        {
                            cogsCollected++;
                            interaction.StartTextInteraction($"Nice, there are {MaxCogs - cogsCollected} left!", new Vector2(579, 426), 0.1f, new Vector2(400, 80));
                        }


                    }
                   
                    else
                    {
                        mistakes++;

                        switch (mistakes)
                        {
                            case 1:
                                interaction.StartTextInteraction("You need to be more carefull, these mistakes damage the ship's internal!", new Vector2(579, 426), 0.1f, new Vector2(400, 80));
                                break;
                            case 2:
                                interaction.StartTextInteraction("This is the second time doing this! Don't be shocked when you get a bill in the mail", new Vector2(579, 426), 0.1f, new Vector2(400, 80));
                                break;
                         //   case 3:
                              //  interaction.StartTextInteraction("Some safety messures are being sent out", new Vector2(579, 426), 0.1f, new Vector2(400, 80));
                               // break;
                            case 4:
                                interaction.StartTextInteraction("You'll never be forgiven for this", new Vector2(579, 426), 0.1f, new Vector2(400, 80));
                                break;
                            case 5:
                                interaction.StartImageInteraction(Texture, new Vector2(579, 426), new Vector2(400, 80));
                                break;
                        }
                       

                        
                    }



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
