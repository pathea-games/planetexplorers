using UnityEngine;
using System.Collections;

public class UIPlayerBuildMoveCtrl : MonoBehaviour 
{

	[HideInInspector] public int mCameraState = 0;

	[System.Serializable]
	public class MoveMenu
	{
		public GameObject	  	mWnd;
		public UICheckbox		mChcekedBox;
		[HideInInspector]
		public Collider 		mCollider;
		[HideInInspector]
		public UIButtonTween 	mBtnTween;
        private System.Action<GameObject, bool> m_CheckBoxEvent;

        public void Init(System.Action<GameObject, bool> checkBoxEvent)
		{
            m_CheckBoxEvent = checkBoxEvent;
            mCollider = mChcekedBox.GetComponent<BoxCollider>();
			mBtnTween = mChcekedBox.GetComponent<UIButtonTween>();
            UIEventListener.Get(mCollider.gameObject).onActivate = CheckBoxEvent;
            mBtnTween.onFinished = TweenFinish;
        }

        void TweenFinish(UITweener tween)
        {
            if(!mCollider.enabled)
                mCollider.enabled = true;
        }

        void CheckBoxEvent(GameObject go, bool isActive)
        {
            if (!isActive)
            {
                mBtnTween.Play(false);
            }
            mCollider.enabled = false;
            if (null != m_CheckBoxEvent)
            {
                m_CheckBoxEvent(go,isActive);
            }
        }
    }

	[SerializeField]
	public MoveMenu mMenu_Root;
	[SerializeField]
	public MoveMenu mMenu_Head;
	[SerializeField]
	public MoveMenu mMenu_Face;
	[SerializeField]
	public MoveMenu mMenu_Body;
	[SerializeField]
	public MoveMenu mMenu_Save;
    private UICheckbox m_CheckboxBack;


    void Start()
	{
		mMenu_Root.Init(CheckBoxEvent);
		mMenu_Head.Init(CheckBoxEvent);
		mMenu_Face.Init(CheckBoxEvent);
     	mMenu_Body.Init(CheckBoxEvent);
		mMenu_Save.Init(CheckBoxEvent);
	}
	
    void CheckBoxEvent(GameObject go, bool isActive)
    {
        UICheckbox checkBox = go.GetComponent<UICheckbox>();
        if (null != m_CheckboxBack && m_CheckboxBack != checkBox)
        {
            m_CheckboxBack.isChecked = false;
        }
        if (isActive)
        {
            m_CheckboxBack = checkBox;
        }

        //lz-2016.07.11 唐小力说Root选中的时候摄像头也要拉近
        if (isActive && (mMenu_Root.mChcekedBox== checkBox || mMenu_Head.mChcekedBox== checkBox|| mMenu_Face.mChcekedBox == checkBox))
        {
            mCameraState = 1;
        }
        else
        {
            //lz-2016.10.09 错误 #4119 创建人物面板，右边的图标提供点击收回面板的功能，并在收回面板后切回全景图
            mCameraState = 0;
        }
    }
}
