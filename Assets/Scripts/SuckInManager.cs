using Unity.VisualScripting;
using UnityEngine;

public class SuckInManager : MonoBehaviour
{

    public float attractionForce = 10f;

    private void OnTriggerStay2D(Collider2D collision)
    {

      if(LayerMask.LayerToName(collision.gameObject.layer) == "Trash")
        {

        }
    }


}
