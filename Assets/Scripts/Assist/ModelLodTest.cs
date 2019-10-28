using UnityEngine;
using System.Collections;
using PETools;

public class ModelLodTest : MonoBehaviour 
{
    public GameObject asset;

    GameObject m_Obj;

    //Vector3 position;

    void Start()
    {
        Load();
    }

    void Update()
    {
        //if (m_Obj != null)
        //    position = m_Obj.transform.position;
    }

    void Load()
    {
        if (asset != null)
        {
            m_Obj = Instantiate(asset, transform.position, Quaternion.identity) as GameObject;
            m_Obj.transform.parent = transform;
            m_Obj.transform.localPosition = Vector3.zero;
        }
    }

    void OnBorderEnter(IntVector2 node)
    {
        if(m_Obj != null)
        {
            Vector3 pos;
            if (PEUtil.GetFixedPosition(LODOctreeMan.self.GetNodes(node), transform.position, out pos))
            {
                m_Obj.transform.position = pos;
                m_Obj.SetActive(true);
            }
        }
    }

    void OnBorderExit(IntVector2 node)
    {
        if (m_Obj != null)
        {
            m_Obj.SetActive(false);
        }
    }

    void OnColliderCreate(IntVector4 node)
    {
        if (m_Obj != null)
        {
            m_Obj.SetActive(true);
        }
    }

    void OnColliderDestroy(IntVector4 node)
    {
        if (m_Obj != null)
        {
            m_Obj.SetActive(false);
        }
    }

    void OnMeshCreated(IntVector4 node)
    {

    }

    void OnMeshDestroy(IntVector4 node)
    {
        if (m_Obj != null)
        {
            GameObject.Destroy(m_Obj);
        }
    }
}
