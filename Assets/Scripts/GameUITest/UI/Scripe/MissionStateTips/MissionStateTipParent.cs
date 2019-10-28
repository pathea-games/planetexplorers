using UnityEngine;
using System.Collections;

public class MissionStateTipParent : MonoBehaviour
{
    [HideInInspector]
    public Transform mParent;

    /// <summary>
    /// 节点是否可用
    /// </summary>
    /// <returns></returns>
    public bool IsFree()
    {
        return mParent.childCount == 0 ? true : false;
    }

    /// <summary>
    /// 删除子物体
    /// </summary>
    public void DeleteChild()
    {
        for (int i = 0; i < mParent.childCount; i++)
        {
            GameObject.Destroy(mParent.GetChild(i));
        }
    }


    #region UNITY FUN

    void Awake()
    {
        mParent = this.transform;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    #endregion
}
