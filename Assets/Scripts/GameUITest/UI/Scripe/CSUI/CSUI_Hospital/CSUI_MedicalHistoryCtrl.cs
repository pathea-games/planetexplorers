using UnityEngine;
using System.Collections;

public class CSUI_MedicalHistoryCtrl : MonoBehaviour
{


    [System.Serializable]
    public class ToolMenu
    {
        public GameObject mWnd;
        public UICheckbox mChcekedBox;
        [HideInInspector]
        public bool mTweenState = false;
        [HideInInspector]
        public Collider mCollider;
        [HideInInspector]
        public UIButtonTween mBtnTween;

        public void Init()
        {
            mCollider = mChcekedBox.GetComponent<BoxCollider>();
            mBtnTween = mChcekedBox.GetComponent<UIButtonTween>();
        }

        public void UpdateBtnTweenState()
        {

            if (mBtnTween == null || mWnd == null || mChcekedBox == null || mCollider == null)
                return;

            if (!mChcekedBox.isChecked && mWnd.activeSelf && mTweenState == true)
            {
                mBtnTween.Play(false);
                mTweenState = false;
                mCollider.enabled = false;
            }
            else if (!mWnd.activeSelf)
            {
                mTweenState = true;
                mCollider.enabled = true;
            }
        }
    }


    [SerializeField]
    public ToolMenu mMedicalHistoryMenu;

    void Start()
    {
        mMedicalHistoryMenu.Init();
    }

    void Update()
    {
        mMedicalHistoryMenu.UpdateBtnTweenState();
    }
}
