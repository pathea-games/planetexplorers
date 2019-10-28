using UnityEngine;
using System.Collections;


public class FollowPoint : MonoBehaviour {
    const float rowDistance = 1f;
    const float lineDistance = 2f;

    public float mHeight;

    public static Transform CreateFollowTrans(Transform target, float height = 0f)
    {
        FollowPoint[] fpArray = target.GetComponentsInChildren<FollowPoint>(true);
        int followIndex = fpArray.Length;

        GameObject followObj = new GameObject("FollowPoint", typeof(FollowPoint));
        followObj.transform.parent = target;
        followObj.transform.localRotation = Quaternion.identity;
        followObj.transform.localScale = Vector3.one;
        followObj.transform.localPosition = GetLocalPos(followIndex) + Vector3.up * height;
        FollowPoint fp = followObj.GetComponent<FollowPoint>();
        fp.mHeight = height;

        return followObj.transform;
    }

    static Vector3 GetLocalPos(int pointIndex)
    {
        int row = pointIndex / 2;
        int line = pointIndex % 2;

        return Vector3.back * rowDistance * (row + 1) + Vector3.right * lineDistance * (line * 2 - 1);
    }

    static void ResortPoint(Transform target)
    {
        FollowPoint[] fpArray = target.GetComponentsInChildren<FollowPoint>(true);

        for(int i = 0; i < fpArray.Length; i++)
        {
            fpArray[i].transform.localPosition = GetLocalPos(i) + Vector3.up * fpArray[i].mHeight;
        }
    }

    public static void DestroyFollowTrans(Transform followTrans)
    {
        if (null == followTrans || null == followTrans.GetComponent<FollowPoint>())
        {
            return;
        }

        Transform target = followTrans.parent;

        GameObject.DestroyImmediate(followTrans.gameObject);

        ResortPoint(target);
    }
}
