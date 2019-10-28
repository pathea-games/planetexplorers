using UnityEngine;
using System.Collections;
using System;

public class PECollision : MonoBehaviour
{
    public Action<Collider, Collision> enter;
    public Action<Collider, Collision> exit;

    public static void Attach(  GameObject obj,
                                Action<Collider, Collision> _enter, 
                                Action<Collider, Collision> _exit)
    {
        if (obj == null) return;

        PECollision col = obj.GetComponent<PECollision>();
        if (col == null) col = obj.AddComponent<PECollision>();

        col.enter += _enter;
        col.exit += _exit;
    }

    public static void Dettach(  GameObject obj,
                                 Action<Collider, Collision> _enter, 
                                 Action<Collider, Collision> _exit)
    {
        if (obj == null) return;

        PECollision col = obj.GetComponent<PECollision>();
        if(col != null)
        {
            col.enter -= _enter;
            col.exit -= _exit;
        }

        GameObject.Destroy(col);
    }

    Collider m_Collider;

    void Awake()
    {
        m_Collider = GetComponent<Collider>();
    }
    
    void OnCollisionEnter(Collision info)
    {
        if (enter != null)
            enter(m_Collider, info);
    } 

    void OnCollisionExit(Collision info)
    {
        if (exit != null)
            exit(m_Collider, info);
    }
}
