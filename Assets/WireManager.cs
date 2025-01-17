using System.Collections.Generic;
using UnityEngine;

public class WireManager : MonoBehaviour
{

    public List<FemaleConnector> connectors;
    bool allWiresConnected = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!allWiresConnected)
        {
            allWiresConnected = true;
            for (int i = 0; i < connectors.Count; i++)
            {
                if (connectors[i].connectedTo == null)
                {
                    allWiresConnected = false;
                    return;
                }
            }

            Debug.Log("All wires connected");
        }
    }
}
