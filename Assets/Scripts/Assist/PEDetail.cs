using UnityEngine;
using System.Collections;

public class PEDetail : MonoBehaviour 
{
    public Transform master;

    void FixedUpdate()
    {
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().MovePosition(master.position);
            GetComponent<Rigidbody>().MoveRotation(master.rotation);
        }
    }
}
