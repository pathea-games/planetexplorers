using UnityEngine;
using System.Collections;

public class MissionStateTipMgr : MonoBehaviour
{
    private static MissionStateTipMgr instance = null;
    public static MissionStateTipMgr Instance { get { return instance; } }

    [SerializeField]
    GameObject mPrefab;
    [SerializeField]
    MissionStateTipParent[] mParents;

    //private int mParentIndex;

    public void ShowMissionTip(string _state, string _content)
    {
        GameObject go = CreatItem(GetParent().mParent, mPrefab);
        MissionStateTipItem item = go.GetComponent<MissionStateTipItem>();
        item.SetContent(_state, _content);
    }

    GameObject CreatItem(Transform _parent, GameObject _prefab)
    {
        GameObject go = Instantiate(_prefab) as GameObject;
        go.transform.parent = _parent;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go;
    }

    MissionStateTipParent GetParent()
    {
        //if (mParentIndex > mParents.Length - 1)
        //    mParentIndex = 0;
        //mParents[mParentIndex].DeleteChild();
        //return mParents[mParentIndex++];

        foreach (MissionStateTipParent item in mParents)
        {
            if (item.IsFree())
                return item;
        }
        mParents[0].DeleteChild();
        return mParents[0];
    }

    #region UNITY FUN
    void Awake()
    {
        instance = this;
        //mParentIndex = 0;
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
