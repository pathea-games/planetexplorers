using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PEInjuredController : MonoBehaviour 
{
    public GameObject master;

    struct InjuresPair
    {
        public Transform key;
        public Transform value;
    }

    ulong m_Frames;
    ulong m_Frame;

    bool m_Injured;

    List<InjuresPair> m_InjuredPairs;
    List<Collider> m_Coliders;

    public void SetInjuredActive(bool value)
    {
        if (m_Injured != value)
        {
//            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in m_Coliders)
            {
                collider.enabled = value;
            }

            m_Injured = value;
        }
    }

	void Start () 
    {
        m_Frame = 3;
        m_Injured = true;

        m_InjuredPairs = new List<InjuresPair>();
        m_Coliders = new List<Collider>(GetComponentsInChildren<Collider>());
        
        if (master != null)
        {
            foreach (Collider collider in m_Coliders)
            {
                Transform tr = PETools.PEUtil.GetChild(master.transform, collider.name);
                if (tr != null)
                {
                    InjuresPair pair = new InjuresPair();
                    pair.key = collider.transform;
                    pair.value = tr;

                    m_InjuredPairs.Add(pair);
                }

                if(collider.GetComponent<Rigidbody>() == null)
                    collider.gameObject.AddComponent<Rigidbody>().isKinematic = true;

                collider.isTrigger = true;
            }
        }

        //StartCoroutine(SyncTransform());

        SetInjuredActive(false);
	}

    void Update()
    {
        if ((++m_Frames) % m_Frame != 0)
            return;

        m_Frame = (ulong)Random.Range(3, 6);

        foreach (InjuresPair pair in m_InjuredPairs)
        {
            if (pair.key != null && pair.value != null)
            {
                if ((pair.key.position - pair.value.position).sqrMagnitude > 0.15f * 0.15f
                    || Quaternion.Angle(pair.key.rotation, pair.value.rotation) > 5.0f)
                {
                    pair.key.position = pair.value.position;
                    pair.key.rotation = pair.value.rotation;
                }
            }
        }
    }

    //IEnumerator SyncTransform()
    //{
    //    while (true)
    //    {
    //        foreach (InjuresPair pair in m_InjuredPairs)
    //        {
    //            if (pair.key != null && pair.value != null)
    //            {
    //                pair.key.position = pair.value.position;
    //                pair.key.rotation = pair.value.rotation;
    //            }
    //        }
    //        yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
    //    }
    //}
}
