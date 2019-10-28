using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestroyAI : MonoBehaviour 
{
    public int[] destroyIds;

    List<Collider> colliders = new List<Collider>();

    public void DestroyMatchAI()
    {
        foreach (Collider iter in colliders)
        {
            DestroyMatch(iter);
        }
    }

    void DestroyMatch(Collider other)
    {
        if (other == null)
            return;

        //AiDataObject ai = other.GetComponent<AiDataObject>();

        //if (ai != null)
        //{
        //    foreach (int id in destroyIds)
        //    {
        //        if (id != 0 && id == ai.dataId)
        //        {
        //            ai.Delete();
        //        }
        //    }
        //}
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        if (!colliders.Contains(other))
        {
            colliders.Add(other);
        }

        DestroyMatch(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (other == null)
            return;

        if (colliders.Contains(other))
        {
            colliders.Remove(other);
        }
    }
}
