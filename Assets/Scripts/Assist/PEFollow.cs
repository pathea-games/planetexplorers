using UnityEngine;
using System.Collections;

public class PEFollow : MonoBehaviour
{
    public Transform master;

    public static void Follow(Transform follower, Transform master)
    {
        if(follower != null && master != null)
        {
            PEFollow follow = follower.gameObject.AddComponent<PEFollow>();
            follow.master = master;
        }
    }

    void LateUpdate()
    {
        if (master != null)
        {
            transform.position = master.position;
            transform.rotation = master.rotation;

            if (transform.parent == null || transform.parent != master.parent)
                transform.parent = master.parent;
        }
    }
}
