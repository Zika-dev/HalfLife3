using Unity.VisualScripting;
using UnityEngine;

public class FunctionBox : MonoBehaviour
{
    private bool isActive;
    public GameObject connectedBox;
    public GameObject connectedWire;
    public GameObject connectedMachine;

    void Start()
    {
        if (connectedMachine == null)
        {
            connectedMachine = null;
        }
    }

    void Update()
    {
        //if (isActive && connectedMachine != null)
        //{
        //    connectedMachine.SendMessage("setMachineActive", true);
        //    Debug.Log("Just sent a true from " + gameObject.name + " and my isActive is set to " + isActive);
        //}
        //else
        //{
        //    Debug.Log("No connected machine");
        //}
    }

    void setActive(Rigidbody2D pluggedWire)
    {
        WireScript conBoxScript = connectedBox.GetComponentInChildren<WireScript>();
        if (conBoxScript != null)
        {
            GameObject wireFriend = pluggedWire.GetComponent<WireTracker>().otherEnd;
            if (wireFriend != null)
            {
                if (conBoxScript.usablePluggedWire != null)
                {
                    if (wireFriend.name == conBoxScript.usablePluggedWire.name)
                    {
                        if (conBoxScript.isPluggedIn)
                        {
                            isActive = true;
                            if (connectedMachine != null)
                            {
                                Debug.Log("I just sent a message, and it was true");
                                connectedMachine.SendMessage("setMachineActive", true);
                            }
                            Debug.Log("My connected box is plugged in, and so am I! And my isActive is set to " + isActive + " and my name is " + gameObject.name);
                        }
                        else
                        {
                            isActive = false;
                            Debug.Log("connected box is not plugged in");
                        }
                    }
                    else
                    {
                        Debug.Log("wirefriend is not = to usablepluggedwire of conbox\nwireFriend = " + wireFriend.name + "\nusablePluggedWire = " + conBoxScript.usablePluggedWire.name);
                    }
                }
                else
                {
                    Debug.Log("Currently, there is nothing plugged into my connected box");
                }
            }
            else
            {
                print("no wirefriend found");
            }
        }
    }

    void setInActive()
    {
        if (connectedMachine != null)
        {
            Debug.Log("I just sent a message, and it was true");
            connectedMachine.SendMessage("setMachineActive", false);
            isActive = true;
        }
    }
}
