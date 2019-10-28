using UnityEngine;
using System.Collections;

namespace PeUIMap
{
	public class UIStroyMap : UIMap 
	{
        [SerializeField]
        private UISprite m_PlayerFindNpcRangeSpr;  //lz-2016.07.19 显示玩家的可以看到的npc范围图
        [SerializeField]
        private UICheckbox m_FindNpcRangeCheck;

		public const float	StoryWorldSize = 18432f;
		Vector2 mapSize { get {return new Vector2(StoryWorldSize , StoryWorldSize); }}

        [HideInInspector]
        public bool ShowFindNpcRange = false;
        //lz-2016.07.19 显示玩家的可以看到的npc范围的任务ID
        public const int ShowFindNpcRangeMissionID = 383;

		protected override void InitWindow ()
		{
			base.InitWindow ();

			mScale = 1f;
			mScaleMin = 1f;
//			mMapPos = Vector2.zero;
			mMapPosMin = Vector3.zero;

			mScaleMin = Screen.width/texSize;
			mMapPosMin.x = (texSize - Screen.width)/2;
			mMapPosMin.y = (texSize - Screen.height)/2;

			mMapPos = GetInitPos();
			
			mMapPos.x = Mathf.Clamp(mMapPos.x,-mMapPosMin.x,mMapPosMin.x);
			mMapPos.y = Mathf.Clamp(mMapPos.y,-mMapPosMin.y,mMapPosMin.y);
			
			mMapWnd.transform.localPosition = new Vector3(mMapPos.x, mMapPos.y, -10);

			mMeatSprite.gameObject.SetActive(true);
			mMoneySprite.gameObject.SetActive(false);
            m_PlayerFindNpcRangeSpr.gameObject.SetActive(false);

            if (MissionManager.Instance != null)
            {
                this.ShowFindNpcRange = MissionManager.Instance.HadCompleteMissionAnyNum(ShowFindNpcRangeMissionID);
            }
            else
            {
                Debug.Log("MissionManager is null!");
            }
        }

        //lz-2016.09.01 如果是Sotry模式，开始使用肉,只有过了314任务才会使用货币
        protected override void OpenWarpWnd(UIMapLabel label)
        {
            if (Pathea.Money.Digital)
            {
                mMeatSprite.gameObject.SetActive(false);
                mMoneySprite.gameObject.SetActive(true);
            }
            else
            {
                mMeatSprite.gameObject.SetActive(true);
                mMoneySprite.gameObject.SetActive(false);
            }
            base.OpenWarpWnd(label);
        }


        protected override Vector3 GetUIPos (Vector3 worldPos)
		{
			Vector3 pos = Vector3.zero;
			pos.x = ((worldPos.x - mapSize.x/2)*texSize/mapSize.x);
			pos.y = (worldPos.z - mapSize.y/2)*texSize/mapSize.y;
			pos.z = -1;
			return pos;
		}

        protected override float ConvetMToPx(float m)
        {
            return texSize/StoryWorldSize*m;
        }

		protected override Vector3 GetInitPos ()
		{
			if (GameUI.Instance.mMainPlayer != null)
			{
				Vector3 player_pos = GameUI.Instance.mMainPlayer.position;
				Vector3 pos = Vector3.zero;
				pos.x = -(player_pos.x - mapSize.x/2)*texSize/mapSize.x;
				pos.y = -(player_pos.z - mapSize.y/2)*texSize/mapSize.y;
				pos.z = -1;

				return pos;
			}
			else
				return Vector3.zero;
		}


		protected override Vector3 GetWorldPos (Vector2 mousePos)
		{
			Vector3 pos = Vector3.zero;

			Vector3 mapCenterPos = Vector3.zero;
			mapCenterPos.x = -mMapPos.x * mapSize.x/(texSize*mScale) + mapSize.x/2;
			mapCenterPos.z = -mMapPos.y * mapSize.y/(texSize*mScale) + mapSize.y/2;
			
			Vector3 OffPos = Vector3.zero;
			OffPos.x = (mousePos.x - Screen.width/2)*mapSize.x/(texSize*mScale);
			OffPos.z = (mousePos.y - Screen.height/2)*mapSize.y/(texSize*mScale);
	
			pos = mapCenterPos + OffPos;
			pos.y = 0;
			return pos;
		}

        protected override void Update()
        {
            base.Update();
            if (ShowFindNpcRange && m_FindNpcRangeCheck.isChecked)
            {
                if (!m_PlayerFindNpcRangeSpr.gameObject.activeSelf)
                {
                    m_PlayerFindNpcRangeSpr.gameObject.SetActive(true);
                    float spriteSize=ShowNpcRadiusPx*2;
                    m_PlayerFindNpcRangeSpr.transform.localScale = new Vector3(spriteSize, spriteSize, 0);
                }
                m_PlayerFindNpcRangeSpr.transform.localPosition = base.mPlayerSpr.transform.localPosition;
            }
            else
            {
                if (m_PlayerFindNpcRangeSpr.gameObject.activeSelf)
                    m_PlayerFindNpcRangeSpr.gameObject.SetActive(false);
            }
        }

	}
}
