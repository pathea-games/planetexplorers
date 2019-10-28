using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PEPathSingleton : Pathea.MonoLikeSingleton<PEPathSingleton>
{
    Dictionary<string, PEPathData> m_PathDic;

    protected override void OnInit()
    {
        base.OnInit();
        m_PathDic = new Dictionary<string, PEPathData>();
    }

    public PEPathData GetPathData(string pathName)
    {
        if (m_PathDic.ContainsKey(pathName))
            return m_PathDic[pathName];
        else
        {
            GameObject pathObj = Resources.Load(pathName) as GameObject;
            if (pathObj != null)
            {
                PEPath pePath = pathObj.GetComponent<PEPath>();
                if (pePath != null)
                {
                    PEPathData data;
                    data.warpMode = pePath.wrapMode;
                    data.path = GetPathWay(pePath.gameObject);

                    string name = PETools.PEUtil.ToPrefabName(pePath.name);
                    if(!m_PathDic.ContainsKey(name))
                        m_PathDic.Add(name, data);

                    return data;
                }
            }
        }

        return new PEPathData();
    }

    Vector3[] GetPathWay(GameObject obj)
    {
        List<Vector3> pathList = new List<Vector3>();
        foreach (Transform tr in obj.transform)
        {
            pathList.Add(tr.position);
        }

        return pathList.ToArray();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        m_PathDic.Clear();
    }
}

public enum EPathMode
{
    Once,
    Loop,
    Pingpong
}

public struct PEPathData
{
    public EPathMode warpMode;
    public Vector3[] path;
}

public class PEPath : MonoBehaviour 
{
    public EPathMode wrapMode;
    public bool isTerrain;

    int m_layer;

    void Awake()
    {
        m_layer = 1 << Pathea.Layer.VFVoxelTerrain
                    | 1 << Pathea.Layer.SceneStatic;
    }
    
    void Update()
    {
        if (!isTerrain)
            return;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tr = transform.GetChild(i);
            RaycastHit hitInfo;
            if(Physics.Raycast(tr.position + Vector3.up*256.0f, -Vector3.up, out hitInfo, 512.0f, m_layer))
            {
                tr.position = hitInfo.point;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        foreach (Transform tr in transform)
        {
            Gizmos.DrawSphere(tr.position, 0.5f);
        }

        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            int j = i + 1;
            if (j < childCount)
                Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(j).position);
            else
            {
                if (wrapMode == EPathMode.Loop)
                {
                    Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(0).position);
                }
            }
        }
    }
}
