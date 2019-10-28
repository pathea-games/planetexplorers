using UnityEngine;
using System.Collections;

public class PathPoints : MonoBehaviour
{
    static GameObject obj;

    public bool drawGizmos;
    public bool isCube;

    void Awake()
    {
        Transform[] childs = GetComponentsInChildren<Transform>();
        for (int i = 0; i < childs.Length; i++)
        {
            if (childs[i] == transform)
                continue;

            childs[i].tag = "PathPoint";
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        MeshFilter filter = null;
        MeshRenderer renderer = null;

        Transform[] childs = GetComponentsInChildren<Transform>();

        if (isCube)
        {
            if (obj == null)
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            filter = obj.GetComponent<MeshFilter>();
            renderer = obj.GetComponent<MeshRenderer>();
        }
        else
        {
            if (obj != null)
                GameObject.DestroyImmediate(obj);
        }

        for (int i = 0; i < childs.Length; i++)
        {
            if (childs[i] == transform)
                continue;

            MeshFilter f = childs[i].gameObject.GetComponent<MeshFilter>();
            MeshRenderer r = childs[i].gameObject.GetComponent<MeshRenderer>();

            if(isCube)
            {
                if (filter != null && f == null)
                {
                    f = childs[i].gameObject.AddComponent<MeshFilter>();
                    f.sharedMesh = filter.sharedMesh;
                }

                if (renderer != null && r == null)
                {
                    r = childs[i].gameObject.AddComponent<MeshRenderer>();
                    r.sharedMaterials = renderer.sharedMaterials;
                }
            }
            else
            {
                if (f != null)
                    GameObject.DestroyImmediate(f);

                if (r != null)
                    GameObject.DestroyImmediate(r);
            }

            if(drawGizmos)
                Gizmos.DrawCube(childs[i].position, Vector3.one*0.5f);
        }
    }
}
