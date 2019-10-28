using UnityEngine;
using System;

namespace WhiteCat
{
    public class SleepingUI : BaseBehaviour
    {
        [SerializeField]
        UIFilledSprite timeSprite;

        Func<float> sleepTime;


        public void Show(Func<float> sleepTime01)
        {
            timeSprite.fillAmount = 0;
            gameObject.SetActive(true);
            this.sleepTime = sleepTime01;
            enabled = true;
            Update();
        }


        public void Hide()
        {
            gameObject.SetActive(false);
            enabled = false;
        }



        void Update()
        {
            timeSprite.fillAmount = sleepTime();
            GameUI.Instance.mItemOp.mMainWnd.SetActive(false);//加这一句是为了让睡觉UI显示的时候，菜单UI必须关闭
        }
    }
}
