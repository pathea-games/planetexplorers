using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class MyMemberInf   //一个成员的信息
{
    public string memberName;


}

public enum MyMemberType   //成员枚举类型
{
    Trainee,
    Instructor
}

public class MyMemberList  //传递成员数据的类
{
    public List<MyMemberInf> traineeList;
    public List<MyMemberInf> instructorList;
    public MyMemberList()
    {
        traineeList = new List<MyMemberInf>();
        instructorList = new List<MyMemberInf>();
    }
    public void ClearList()
    {
        traineeList.Clear();
        instructorList.Clear();
    }
}

public class CSUI_MemberMgr : MonoBehaviour
{
    [SerializeField]
    private UIGrid mMemberPageGrid;//成员Grid
    [SerializeField]
    private GameObject mMemberPrefab;//成员预制件
    //[SerializeField]
    //private GameObject mMemberPage;//成员节点
    [SerializeField]

    public Dictionary<MyMemberType, List<MyMemberInf>> mMemberDataList = new Dictionary<MyMemberType, List<MyMemberInf>>();//存放所有成员
    private List<CSUI_MemberItemLocal> mMemberObjList = new List<CSUI_MemberItemLocal>();//存放生成的成员
    private List<MyMemberInf> curMemList = new List<MyMemberInf>();//即将生成的成员
    private MyMemberType mPageType;//临时存放按钮类型

    private static CSUI_MemberMgr mInstance;
    public static CSUI_MemberMgr Instance { get { return mInstance; } }

    private MyMemberList passList;
    public MyMemberList PassList //传递数据的属性接口
    {
        get { return passList; }
        set { passList = value; InitList(); }
    }

    private List<CSPersonnel> mRandomNpcs;//随机Npc
    public List<CSPersonnel> RandomNpcs
    {
        get { return mRandomNpcs; }
        set { mRandomNpcs = value; SortNpcs(); }
    }

    private List<CSPersonnel> mInstructorNpcs = null;//教练Npc
    //public List<CSPersonnel> InstructorNpcs
    //{
    //    get { return mInstructorNpcs; }
    //    set { mInstructorNpcs = value; }
    //}

    private List<CSPersonnel> mOtherNpcs = null;//其他非教练Npc
    //public List<CSPersonnel> OtherNpcs
    //{
    //    get { return mOtherNpcs; }
    //    set { mOtherNpcs = value; }
    //}
    void SortNpcs()//NPC分类成教练和非教练
    {
        if (mRandomNpcs.Count <= 0)
            return;
        mInstructorNpcs.Clear();
        mOtherNpcs.Clear();
        for (int i = 0; i < mRandomNpcs.Count; i++)
        {
            //if (mRandomNpcs[i].Occupation == CSConst.potInstructor)
            //    mInstructorNpcs.Add(mRandomNpcs[i]);
            //else
            //    mOtherNpcs.Add(mRandomNpcs[i]);
        }
    }

    void InitList()
    {
        mMemberDataList[MyMemberType.Trainee] = passList.traineeList;
        mMemberDataList[MyMemberType.Instructor] = passList.instructorList;
        Refresh(MyMemberType.Trainee);
    }



    void Refresh(MyMemberType type)
    {
        ClearMemberList();
        foreach (KeyValuePair<MyMemberType, List<MyMemberInf>> i in mMemberDataList)
        {
            if (i.Key == type)
                curMemList = i.Value;
        }
        foreach (MyMemberInf mmi in curMemList)
            CreatMemberItem(mmi);
    }



    void CreatMemberItem(MyMemberInf Info)//创建一个成员
    {
        GameObject go = GameObject.Instantiate(mMemberPrefab) as GameObject;
        go.transform.parent = mMemberPageGrid.transform;
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        if (!go.activeSelf)
            go.SetActive(true);

        CSUI_MemberItemLocal mil = go.GetComponent<CSUI_MemberItemLocal>();
        mil.MemberInfoLocal = Info;
        // grid.e_ItemClick += ItemClick;
        mMemberObjList.Add(mil);
        mMemberPageGrid.repositionNow = true;
    }

    void ClearMemberList()//删除生成的所有成员
    {
        foreach (CSUI_MemberItemLocal mil in mMemberObjList)
        {
            if (mil != null)
            {
                GameObject.Destroy(mil.gameObject);
                mil.gameObject.transform.parent = null;
            }
        }
        mMemberObjList.Clear();
    }

    void PageTraineeOnActive(bool active)//Trainee按钮调用
    {
        if (active)
        {
            mPageType = MyMemberType.Trainee;
            Refresh(mPageType);
        }
    }

    void PageInstructorOnActive(bool active)//Instructor按钮调用
    {
        if (active)
        {
            mPageType = MyMemberType.Instructor;
            Refresh(mPageType);
        }
    }


    public void Test()
    {
        if (CSUI_MainWndCtrl.Instance == null)
            return;
//        /*CSPersonnel[] npcs = */CSUI_MainWndCtrl.Instance.Creator.GetNpcs();
//        foreach (CSPersonnel csp in npcs)
//        {
//
//        }
    }




    #region  UNITY_INNER
    void Awake()
    {
        mInstance = this;
    }

    void Update()
    {

    }
    #endregion
}
