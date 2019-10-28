using UnityEngine;
using System.Collections;

namespace PeUIEffect
{
	public class GameMenuScaleEffect : UIEffect 
	{
		[SerializeField] AcEffect effectScale_x;
		[SerializeField] AcEffect effectScale_y;
		[SerializeField] UIMenuList menulist;
		[SerializeField] GameObject mSpeularPrefab;
        [SerializeField] TweenScale mTweenScale;
        [SerializeField] TweenPosition mTweenPos;


        Transform target;
		float time = 0;
		Vector3 targetScale;
        bool tweenScaleing = false;


        #region mono methods
        void Start()
		{
			target = menulist.rootPanel.spBg.transform;
			menulist.rootPanel.spBg.pivot = UIWidget.Pivot.Bottom;
			target.localPosition = new Vector3(85,-menulist.rootPanel.spBg.transform.localScale.y/2,0);

			// root panel 加长31 并下移
			Vector3 v = menulist.rootPanel.spBg.transform.localScale;
			menulist.rootPanel.spBg.transform.localScale = new Vector3(v.x,v.y + 35,v.z);
		 	v = menulist.rootPanel.spBg.transform.localPosition; 
			menulist.rootPanel.spBg.transform.localPosition = new Vector3(v.x,v.y - 35,v.z);

			targetScale = target.localScale;

			foreach (UIMenuPanel panel in menulist.panels)
				AddMenuSpeular(panel.transform);

            mTweenScale.onFinished = TweenScaleFinishEvent;

        }
        void Update()
        {
            if (base.m_Runing)
            {
                float x = effectScale_x.bActive ? effectScale_x.GetAcValue(time) : 0;
                float y = effectScale_y.bActive ? effectScale_y.GetAcValue(time) : 0;
                target.transform.localScale = targetScale + new Vector3(x, y, 1);
                time += Time.deltaTime;
                if (time >= effectScale_x.EndTime)
                    End();
            }

            if (tweenScaleing)
            {
                UpdateListMenuItemAlpha(menulist.transform.localScale.y);
            }
        }

        #endregion


        #region private methods
        void AddMenuSpeular(Transform parent)
		{
			GameObject o = GameObject.Instantiate(mSpeularPrefab) as GameObject;
			o.transform.parent = parent;
			o.transform.localPosition = new Vector3(0,0,-2);
			o.transform.localScale = Vector3.one;
			o.SetActive(true);
		}
        Color _dstCol = new Color(1, 1, 1, -1);
        void UpdateListMenuItemAlpha(float alpha)
        {
            alpha *= alpha;
            if (!Mathf.Approximately(alpha, _dstCol.a))
            {
                _dstCol.a = Mathf.Clamp01(alpha);
                int n = menulist.Items.Count;
                for (int i = 0; i < n; i++)
                {
                    UIMenuListItem item = menulist.Items[i];
                    item.LbText.color = _dstCol;
                    item.mIcoSpr.color = _dstCol;
                    item.SpHaveChild.color = _dstCol;
                }
            }
        }

        void TweenScaleFinishEvent(UITweener tween)
        {
            tweenScaleing = false;
            UpdateListMenuItemAlpha(menulist.transform.localScale.y);
        }

        #endregion

        #region override methods
        public override void Play()
        {
            if (base.m_Runing) return;
            base.mForward = !base.mForward;
            mTweenScale.Play(base.mForward);
            mTweenPos.Play(base.mForward);
            tweenScaleing = true;
            time = 0;
            base.Play();
        }

        public override void Play(bool forward)
        {
            if (base.m_Runing) return;
            base.Play(forward);
        }

        public override void End ()
		{
            base.End();
            target.transform.localScale = targetScale;
        }
        #endregion
    }
}
