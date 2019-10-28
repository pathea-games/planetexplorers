using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class PointGraphTool : MonoBehaviour
{

    public string caveName;

    Transform player;

    // Use this for initialization
    void Start()
    {
        if (player == null && PeCreature.Instance.mainPlayer != null)
            player = PeCreature.Instance.mainPlayer.GetComponent<PeTrans>().trans;

        AddMesh();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && Input.GetKeyUp(KeyCode.O) && player != null)
        {
            GameObject obj = CreatePoint(player.position + Vector3.up, player.rotation);
            if (obj != null)
                obj.transform.parent = GetCaveTransform(caveName);
        }
    }

    void AddMesh()
    {
        foreach (Transform tr in transform)
        {
            List<GameObject> tmpList = new List<GameObject>();

            foreach (Transform t in tr)
            {
                tmpList.Add(CreatePoint(t.position, t.rotation));

                GameObject.Destroy(t.gameObject);
            }

            foreach (GameObject obj in tmpList)
            {
                obj.transform.parent = GetCaveTransform(tr.name);
            }
        }
    }

    GameObject CreatePoint(Vector3 position, Quaternion rotation)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject.Destroy(obj.GetComponent<BoxCollider>());

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.localScale = Vector3.one * 0.5f;

        return obj;
    }

    Transform GetCaveTransform(string name)
    {
        Transform tmp = transform.FindChild(name);

        if (tmp == null)
        {
            tmp = new GameObject(name).transform;
            tmp.parent = transform;
        }

        return tmp;
    }

    //public void OnDrawGizmosSelected()
    //{
    //    foreach (Transform tr in transform)
    //    {
    //        foreach (Transform it in tr)
    //        {
    //            Gizmos.DrawCube(it.position, Vector3.one * 0.2f);
    //        }
    //    }
    //}
}
