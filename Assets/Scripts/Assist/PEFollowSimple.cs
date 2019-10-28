using UnityEngine;
using System.Collections;

public class PEFollowSimple : MonoBehaviour
{
    public Transform master;

    void LateUpdate()
    {
        if (master != null)
        {
            transform.position = master.position;
            transform.rotation = master.rotation;
        }
    }
}
