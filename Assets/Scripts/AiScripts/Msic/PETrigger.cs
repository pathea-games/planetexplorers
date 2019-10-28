using UnityEngine;
using System.Collections;

public delegate void TriggerDelegate(Collider src, Collider dst);
public delegate void CollisionDelegate(Collider src, Collision dst);

public class PETrigger : MonoBehaviour
{
    event TriggerDelegate TriggerEnterEvent;
    event TriggerDelegate TriggerStayEvent;
    event TriggerDelegate TriggerExitEvent;

    bool m_AddRigidBody;
    Rigidbody m_Rigidbody;

    public static void AttachTriggerEvent(GameObject obj, TriggerDelegate enter, TriggerDelegate stay, TriggerDelegate exit)
    {
        if (obj != null)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                AttachTriggerEvent(colliders[i], enter, stay, exit);
            }
        }
    }

    public static void AttachTriggerEvent(Collider collider, TriggerDelegate enter, TriggerDelegate stay, TriggerDelegate exit)
    {
        if (collider != null && collider.isTrigger)
        {
            PETrigger trigger = collider.gameObject.GetComponent<PETrigger>();
            if (trigger == null)
                trigger = collider.gameObject.AddComponent<PETrigger>();

            if (trigger != null)
                trigger.AttachTrigger(enter, stay, exit);
        }
    }

    public static void DetachTriggerEvent(GameObject obj, TriggerDelegate enter, TriggerDelegate stay, TriggerDelegate exit)
    {
        if (obj != null)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                DetachTriggerEvent(colliders[i], enter, stay, exit);
            }
        }
    }

    public static void DetachTriggerEvent(Collider collider, TriggerDelegate enter, TriggerDelegate stay, TriggerDelegate exit)
    {
        if (collider == null && collider.isTrigger)
        {
            PETrigger trigger = collider.gameObject.GetComponent<PETrigger>();
            if (trigger != null)
            {
                trigger.DetachTrigger(enter, stay, exit);
            }

            GameObject.Destroy(trigger);
        }
    }

    public void AttachTrigger(TriggerDelegate enter, TriggerDelegate stay, TriggerDelegate exit)
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        if (m_Rigidbody == null)
        {
            m_AddRigidBody = true;
            m_Rigidbody = gameObject.AddComponent<Rigidbody>();
            m_Rigidbody.useGravity = false;
            m_Rigidbody.isKinematic = true;
        }

        TriggerEnterEvent += enter;
        TriggerStayEvent += stay;
        TriggerExitEvent += exit;
    }

    public void DetachTrigger(TriggerDelegate enter, TriggerDelegate stay, TriggerDelegate exit)
    {
        if (m_AddRigidBody && m_Rigidbody != null)
        {
            m_AddRigidBody = false;
            GameObject.Destroy(m_Rigidbody);
        }

        TriggerEnterEvent -= enter;
        TriggerStayEvent -= stay;
        TriggerExitEvent -= exit;
    }

    void OnTriggerEnter(Collider other)
    {
        if (TriggerEnterEvent != null)
        {
            TriggerEnterEvent(GetComponent<Collider>(), other);
        }
    }

    //void OnTriggerStay(Collider other)
    //{
    //    if (TriggerStayEvent != null)
    //    {
    //        TriggerStayEvent(GetComponent<Collider>(), other);
    //    }
    //}

    void OnTriggerExit(Collider other)
    {
        if (TriggerExitEvent != null)
        {
            TriggerExitEvent(GetComponent<Collider>(), other);
        }
    }
}
