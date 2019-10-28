using UnityEngine;
using System.Collections;

public class CSUI_MemberItemLocal : MonoBehaviour
{

    [SerializeField]
    private UISprite iconSpr;//头像sprite


    MyMemberInf mMemberInfo;
    public MyMemberInf MemberInfoLocal
    {
        get { return mMemberInfo; }
        set { mMemberInfo = value; InitLocalInfo(); }
    }



    void InitLocalInfo()//初始化本地信息
    {
        if (mMemberInfo == null)
            return;
        SetIcon(mMemberInfo.memberName);
    }

    void SetIcon(string s)//设置头像
    {
        if (iconSpr == null)
            return;
        iconSpr.spriteName = s;
        iconSpr.MakePixelPerfect();
    }







    void Start()
    {

    }


    void Update()
    {

    }
}
