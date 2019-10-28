using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PeMap;
using Pathea;

namespace PeUIMap
{
    public abstract class UIMap : UIStaticWnd
    {
        [SerializeField]
        protected GameObject mMaskOpWndParent; //MaskOpWnd�ĸ���
        [SerializeField]
        protected GameObject mMaskOpWnd;
        [SerializeField]
        protected Transform mMaskOpWndCenter; //MaskOpWnd�����ĵ�λ��
        [SerializeField]
        protected Transform mMaskOpArray; //MaskOpWnd��ָ���ͷ
        [SerializeField]
        protected GameObject mWarpWnd;
        [SerializeField]
        protected Transform mWarpWndCenter; //WarpWnd�����ĵ�λ��
        [SerializeField]
        protected Transform mWarpArray;//WarpWnd��ָ���ͷ
        [SerializeField]
        protected GameObject mIconSelWnd;
        [SerializeField]
        protected GameObject mMapWnd;

        [SerializeField]
        protected GameObject mMapLabelPrefab;
        [SerializeField]
        protected UISprite mPlayerSpr;
        [SerializeField]
        protected UILabel mPosLabal;
        [SerializeField]
        protected Transform mPosLabelTrans;

        [SerializeField]
        protected UICheckbox mCkNpcMask;
        [SerializeField]
        protected UICheckbox mCkUserMask;
        [SerializeField]
        protected UICheckbox mCkVehicleMask;

        //mask wnd
        [SerializeField]
        protected UISprite mMaskSpr;
        [SerializeField]
        protected UISprite mMaskSpr_2;
        [SerializeField]
        protected UILabel mMaskDes;

        //Warp wnd
        [SerializeField]
        protected UILabel mItemCost;
        [SerializeField]
        protected UILabel mWarpDes;
        [SerializeField]
        protected UISprite mMoneySprite;
        [SerializeField]
        protected UISprite mMeatSprite;

        //lz-2016.09.06 ���﹥��Ч��
        [SerializeField]
        protected MaplabelMonsterSiegeEffect m_SiegeEffectPrefab;

        //lz-2016.07.19 ��ҿɼ���NPC��Χ�뾶,��λM
        public const float ShowNpcRadiusM = 500f;
        protected float ShowNpcRadiusPx=0f;


        [SerializeField]
        GameObject mOpbtns;

        protected Vector3 mMousePos = Vector3.zero;
        protected bool mShowDes = false;
        protected List<UIMapLabel> m_CurrentMapLabelList = new List<UIMapLabel>();
        protected Queue<UIMapLabel> m_MapLabelPool = new Queue<UIMapLabel>(); //Log:lz-2016.04.18 ���UIPool�����Ż�MapLabel,���ⷴ��ʵ��������
        protected List<MapIcon> mUserIconList = new List<MapIcon>();


        protected override void InitWindow()
        {
            base.InitWindow();

            //lz-2016.07.20 ����
            ShowNpcRadiusPx = ConvetMToPx(ShowNpcRadiusM);

            if (LabelMgr.Instance != null)
            {
                GetAllMapLabel();
                LabelMgr.Instance.eventor.Subscribe(AddOrRemoveLable);
                GetUserIcon();
            }
            if (mOpbtns != null) mOpbtns.SetActive(PeGameMgr.IsMulti);

            //			mMapPos = GetInitPos();
            //
            //
            //			mMapPosMin.x = (texSize*mScale - Screen.width)/2;
            //			mMapPosMin.y = (texSize*mScale - Screen.height)/2;
            //			mMapPos.x = Mathf.Clamp(mMapPos.x,-mMapPosMin.x,mMapPosMin.x);
            //			mMapPos.y = Mathf.Clamp(mMapPos.y,-mMapPosMin.y,mMapPosMin.y);
            //
            //			mMapWnd.transform.localPosition = new Vector3(mMapPos.x, mMapPos.y, -10);
        }

        public override void Show()
        {
            base.Show();
            Reflash();
            mShowDes = false;
        }

        protected override void OnHide()
        {
            //lz-2016.09.01 �رյ�ͼ��ʱ��
            mMaskOpWndParent.SetActive(false);
            mWarpWnd.SetActive(false);
            base.OnHide();
        }

        void GetUserIcon()
        {
            mUserIconList.Clear();
            foreach (MapIcon icon in MapIcon.Mgr.Instance.iconList)
            {
                if (icon.iconType == EMapIcon.Custom)
                    mUserIconList.Add(icon);
            }
        }

        public virtual void Reflash()
        {
            GetAllMapLabel();
        }

        protected virtual Vector3 GetUIPos(Vector3 worldPos)
        {
            return Vector3.zero;
        }

        protected virtual Vector3 GetWorldPos(Vector2 mousePos)
        {
            return Vector3.zero;
        }

        protected virtual Vector3 GetInitPos()
        {
            return Vector3.zero;
        }

        protected virtual void Update()
        {
            if (Input.GetMouseButtonDown(1))
                mMousePos = Input.mousePosition;
            if (GameUI.Instance.mMainPlayer == null)
                return;
            // updatePlayerPos
			if(PeGameMgr.IsSingleAdventure&& PeGameMgr.yirdName==AdventureScene.Dungen.ToString()){
				mPlayerSpr.transform.localPosition = GetUIPos(RandomDungenMgrData.revivePos);
			}else{
				mPlayerSpr.transform.localPosition = GetUIPos(GameUI.Instance.mMainPlayer.position);
			}
			mPlayerSpr.transform.rotation = Quaternion.Euler(0, 0, -GameUI.Instance.mMainPlayer.rotation.eulerAngles.y);
            ChangeScale(Input.GetAxis("Mouse ScrollWheel") * 0.5f);
            UpdatePosLabel();

            UpdateMapLabelState();
        }

        protected virtual void UIMapBg_OnClick()
        {
            //when mouseMove cancel OpenMaskWnd opration
            if (Vector3.Distance(mMousePos, Input.mousePosition) > 5)
                return;
            if (Input.GetMouseButtonUp(1))
                OpenMaskWnd(null);
            if (Input.GetMouseButtonUp(0))
            {
                mMaskOpWndParent.SetActive(false);
            }
        }


        //lz-2016.07.20  ͨ���׻������
        protected abstract float ConvetMToPx(float m);

        #region MapScale

        protected float mScale = 1f;
        protected float mScaleMin = 1f;
        protected Vector2 mMapPos = Vector2.zero;
        protected Vector2 mMapPosMin = Vector3.zero;
		protected float texSize { get { return PeGameMgr.IsAdventure||PeGameMgr.IsBuild ? 3140 : 4096; } }

        protected void ChangeScale(float delta)
        {
            if (Mathf.Abs(delta) < PETools.PEMath.Epsilon)
                return;
            float oldScale = mScale;
            mScale = Mathf.Clamp(mScale + delta, mScaleMin, 1);
            mMapWnd.transform.localScale = new Vector3(mScale, mScale, 1);
            mMapPosMin.x = (texSize * mScale - Screen.width) / 2;
            mMapPosMin.y = (texSize * mScale - Screen.height) / 2;
            mMapPos *= mScale / oldScale;
            if (Mathf.Abs(mScale - oldScale) > PETools.PEMath.Epsilon)
            {
                mWarpWnd.SetActive(false);
                mMaskOpWndParent.SetActive(false);
            }
            OnMapDrag(Vector2.zero);
        }


        public void OnMapDrag(Vector2 delta)
        {
            mMapPos.x = Mathf.Clamp(mMapPos.x + delta.x, -mMapPosMin.x, mMapPosMin.x);
            mMapPos.y = Mathf.Clamp(mMapPos.y + delta.y, -mMapPosMin.y, mMapPosMin.y);
            mMapWnd.transform.localPosition = new Vector3(mMapPos.x, mMapPos.y, -10);
            if (delta.magnitude > PETools.PEMath.Epsilon)
            {
                mWarpWnd.SetActive(false);
                mMaskOpWndParent.SetActive(false);
            }
        }
        #endregion


        #region MapLabel
        void AddOrRemoveLable(object sender, LabelMgr.Args arg)
        {
            if (arg == null || arg.label == null)
                return;
            if (arg.add)
                AddMapLabel(arg.label);
            else
                RemvoeMapLabel(arg.label);                   
        }

        void GetAllMapLabel()
        {
            foreach (UIMapLabel uiLabel in m_CurrentMapLabelList)
            {
                if (uiLabel != null)
                {
                    TryRemoveMonsterSiegeEffect(uiLabel);
                    uiLabel.gameObject.SetActive(false);
                    this.m_MapLabelPool.Enqueue(uiLabel);
                }
            }

            this.m_CurrentMapLabelList.Clear();
            for (int i = 0; i < LabelMgr.Instance.mList.Count; i++)
				AddMapLabel(LabelMgr.Instance.mList[i]);
		}

        void AddMapLabel(ILabel label)
        {
            if (label.GetShow() == EShow.All || label.GetShow() == EShow.BigMap)
            {
                UIMapLabel uiLabel;
                if (this.m_MapLabelPool.Count > 0)
                {
                    uiLabel = this.m_MapLabelPool.Dequeue();
                    uiLabel.gameObject.SetActive(true);
                }
                else
                {
                    GameObject obj = GameObject.Instantiate(mMapLabelPrefab) as GameObject;
                    obj.transform.parent = mMapWnd.transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localScale = Vector3.one;
                    obj.transform.localRotation = Quaternion.identity;
                    uiLabel = obj.GetComponent<UIMapLabel>();
                }

                if (uiLabel != null)
                {
                    uiLabel.transform.localScale = Vector3.one;
                    uiLabel.SetLabel(label);
                    uiLabel.e_OnMouseOver += UIMapLabel_OnMouseOver;
                    uiLabel.e_OnClick += UIMapLabel_OnClick;
                    RepositionMapLabel(uiLabel);
                    uiLabel.gameObject.name = string.Format("MapLabel_{0}_{1}", uiLabel._ILabel.GetType().ToString(),uiLabel._ILabel.GetText());
                    this.m_CurrentMapLabelList.Add(uiLabel);

					UpdateMapLabel(uiLabel);
				}

                TryAddMonsterSiegeEffect(uiLabel);

                if (label.GetType() == ELabelType.Mission)
                {
                    if (label.GetIcon() != 13)
                        return;
                    MissionLabel ml = label as MissionLabel;
                    uiLabel.transform.localScale *= 0.5f;

                    if(ml.m_attachOnID != 0)
                        uiLabel.SetLabelPosByNPC(ml.m_attachOnID);
                }
                //if (missionID == 0 || label.GetIcon() != 13)
                //    return;

                //uiLabel.transform.localScale *= 0.5f;

                //if (MissionRepository.m_TypeFollow.ContainsKey(targetID))
                //{
                //    TypeFollowData fol = MissionManager.GetTypeFollowData(targetID);
                //    if (fol.m_LookNameID != 0)
                //        uiLabel.SetLabelPosByNPC(fol.m_LookNameID);
                //}
                //else if (MissionRepository.m_TypeSearch.ContainsKey(targetID))
                //{
                //    TypeSearchData ser = MissionManager.GetTypeSearchData(targetID);
                //    if (ser.m_NpcID != 0)
                //        uiLabel.SetLabelPosByNPC(ser.m_NpcID);
                //}
                //else if (AdRMRepository.m_AdTypeFollow.ContainsKey(targetID))
                //{
                //    TypeFollowData fol = MissionManager.GetTypeFollowData(targetID);
                //    if (fol.m_LookNameID != 0)
                //        uiLabel.SetLabelPosByNPC(fol.m_LookNameID);
                //}
                //else if (AdRMRepository.m_AdTypeSearch.ContainsKey(targetID))
                //{
                //    TypeSearchData ser = MissionManager.GetTypeSearchData(targetID);
                //    if (ser.m_NpcID != 0)
                //        uiLabel.SetLabelPosByNPC(ser.m_NpcID);
                //}
            }
        }

        /// <summary> lz-2016.10.09 ������ӹ��﹥����Ч</summary>
        void TryAddMonsterSiegeEffect(UIMapLabel uiLabel)
        {
            ILabel label = uiLabel._ILabel;
            //lz-2016.09.06 ����ǹ��﹥�ǣ������һ������Ч��
            if (null != label && label is MonsterBeaconMark)
            {
                MonsterBeaconMark mark = label as MonsterBeaconMark;
                if (mark.IsMonsterSiege)
                {
                    TryRemoveMonsterSiegeEffect(uiLabel);
                    GameObject go = (GameObject)Instantiate(m_SiegeEffectPrefab.gameObject);
                    go.transform.parent = uiLabel.transform;
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localScale = Vector3.one;
                    go.transform.localRotation = Quaternion.identity;
                    go.GetComponent<MaplabelMonsterSiegeEffect>().Run = true;
                }
            }
        }

        /// <summary> lz-2016.10.09 �����Ƴ����﹥����Ч</summary>
        void TryRemoveMonsterSiegeEffect(UIMapLabel uiLabel)
        {
            ILabel label = uiLabel._ILabel;
            //lz-2016.09.06 ����ǹ��﹥�ǣ����Ƴ���Ч��
            if (null!=label&&label is MonsterBeaconMark)
            {
                MonsterBeaconMark mark = label as MonsterBeaconMark;
                if (mark.IsMonsterSiege)
                {
                    MaplabelMonsterSiegeEffect[] effects = uiLabel.GetComponentsInChildren<MaplabelMonsterSiegeEffect>(true);
                    if (null != effects && effects.Length > 0)
                    {
                        for (int i = 0; i < effects.Length; i++)
                        {
                            effects[i].Run = false;
                            Destroy(effects[i].gameObject);
                        }
                    }
                }
            }
        }

        void RemvoeMapLabel(ILabel label)
        {
            UIMapLabel uiLabel = m_CurrentMapLabelList.Find(itr => (itr._ILabel == label));
            if (uiLabel != null)
            {
                TryRemoveMonsterSiegeEffect(uiLabel);
                uiLabel.gameObject.SetActive(false);
                this.m_CurrentMapLabelList.Remove(uiLabel);
                this.m_MapLabelPool.Enqueue(uiLabel);
            }
        }


        void RepositionMapLabel(UIMapLabel uiLabel)
        {
            uiLabel.transform.localPosition = GetUIPos(uiLabel.worldPos);
        }

        protected virtual void UIMapLabel_OnMouseOver(UIMapLabel sender, bool isOver)
        {
            if (isOver){
				if(sender.descText=="")
					mPosLabal.text = ((int)sender.worldPos.x).ToString() + "," + ((int)sender.worldPos.z).ToString();
				else
                	mPosLabal.text = sender.descText + "\n" + ((int)sender.worldPos.x).ToString() + "," + ((int)sender.worldPos.z).ToString();
			}
            mShowDes = isOver;
        }

        protected virtual void UIMapLabel_OnClick(UIMapLabel sender)
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (sender.type == ELabelType.User)
                    OpenMaskWnd(sender);
                else if (sender.fastTrval)
                {
                    OpenWarpWnd(sender);
                }
                else
                {
                    TryClickNextLable(sender,true);
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                if (sender.type == ELabelType.User)
                {
                    if (PeGameMgr.IsMulti)
                    {
                        UserLabel userLb = sender._ILabel as UserLabel;
                        if (userLb != null && userLb.playerID == PlayerNetwork.mainPlayerId)
                            PlayerNetwork.mainPlayer.RequestRemoveMask(userLb.index);
                    }
                    else
                        UserLabel.Mgr.Instance.Remove(sender._ILabel as UserLabel);
                    //lz-2016.10.24 删除标记，鼠标提示变回位置
                    mShowDes = false;
                }
                else
                {
                    TryClickNextLable(sender, false);
                }
            }
        }

        /// <summary>�����������ģ����MapLabel,�����Ǳ���û�е���¼���MapLabel�ڸ�ס�˿��Ե����MapLabel</summary>
        void TryClickNextLable(UIMapLabel sender, bool leftClick)
        {
            Ray ray = new Ray();
            ray.origin = sender.transform.position;
            ray.direction = ray.origin;
            RaycastHit[] hitArray = Physics.RaycastAll(ray);
            if (null != hitArray && hitArray.Length > 0)
            {
                for (int i = 0; i < hitArray.Length; i++)
                {
                    UIMapLabel hitLable = hitArray[i].collider.GetComponent<UIMapLabel>();
                    if (null == hitLable|| hitLable==sender) continue;
                    if (leftClick)
                    {
                        if (hitLable.type == ELabelType.User)
                            OpenMaskWnd(hitLable);
                        else if (hitLable.fastTrval)
                        {
                            OpenWarpWnd(hitLable);
                        }
                        else
                        {
                            continue; 
                        }
                    }
                    else
                    {
                        if (hitLable.type == ELabelType.User)
                        {
                            if (PeGameMgr.IsMulti)
                            {
                                UserLabel userLb = hitLable._ILabel as UserLabel;
                                if (userLb != null && userLb.playerID == PlayerNetwork.mainPlayerId)
                                    PlayerNetwork.mainPlayer.RequestRemoveMask(userLb.index);
                            }
                            else
                                UserLabel.Mgr.Instance.Remove(hitLable._ILabel as UserLabel);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    break;
                }
            }
        }

        public void UpdateMapLabelState()
        {
            //lz-2016.07.16 ��Ϊforѭ��������foreach��gc
            for (int i = 0; i < m_CurrentMapLabelList.Count;i++ )
				UpdateMapLabel(m_CurrentMapLabelList[i]);
        }

		void UpdateMapLabel(UIMapLabel lb)
		{
			if (lb == null)
			{
				m_CurrentMapLabelList.Remove(lb);
				return;
			}

			if (lb.type == ELabelType.Npc)
			{
				Vector2 v1 = new Vector2(lb.worldPos.x, lb.worldPos.z);
                if (null != GameUI.Instance && null != GameUI.Instance.mMainPlayer) //lz-2017.07.31 错误 #11490 Crash
                {
                    Vector2 v2 = new Vector2(GameUI.Instance.mMainPlayer.position.x, GameUI.Instance.mMainPlayer.position.z);

                    if (PeGameMgr.IsMulti && lb._ILabel.GetIcon() == MapIcon.AllyPlayer)
                    {
                        RepositionMapLabel(lb);
                        //lz-2018.01.03 队友更新朝向角度
                        MapCmpt mapCmpt = (lb._ILabel as MapCmpt);
                        if (mapCmpt && mapCmpt.Entity && mapCmpt.Entity.peTrans)
                        {
                            lb.transform.rotation = Quaternion.Euler(0, 0, -mapCmpt.Entity.peTrans.rotation.eulerAngles.y);
                        }
                        lb.gameObject.SetActive(true);
                    }
                    else if (lb._ILabel.GetIcon() != PeMap.MapIcon.ServantDeadPlace)
                    {
                        if (Vector2.Distance(v1, v2) < ShowNpcRadiusPx && mCkNpcMask.isChecked)
                        {
                            RepositionMapLabel(lb);
                            lb.gameObject.SetActive(true);
                        }
                        else
                            lb.gameObject.SetActive(false);
                    }
                }
			}

			else if (lb.type == ELabelType.User)
				lb.gameObject.SetActive(mCkUserMask.isChecked);
			else if (lb.type == ELabelType.Vehicle)
			{
				lb.gameObject.SetActive(mCkVehicleMask.isChecked);
				//lz-2016.08.01 �����ؾ�λ��
				if (mCkVehicleMask.isChecked)
				{
					RepositionMapLabel(lb);
				}
			}

			//lz-2016.06.15 npc����������Ż����ʹӾͲ���ʾ���ͼ��ͼ�������ͼ��
			if (lb.type == ELabelType.Mission)
			{
				//lz-2016.06.16 ������������͵�����ͼ�겻���أ�npc�����ΪUIMapLabel���ڵ�npc���󣬶���������Ĺ���npc����
				MissionLabel missionLabel = (MissionLabel)lb._ILabel;
				//lz-2016.07.16 ���ﲻ����ֱ��return�ų��������ᵼ�º����UIMapLabelû�б����������Ը�Ϊ!�ų�
				if (missionLabel.m_type != MissionLabelType.misLb_target && lb.NpcID != -1)
				{
					PeEntity entity = EntityMgr.Instance.Get(lb.NpcID);
					if (null != entity && null != entity.NpcCmpt && entity.NpcCmpt.IsFollower)
					{
						if (lb.gameObject.activeSelf)
							lb.gameObject.SetActive(false);
					}
					else
					{
						if (!lb.gameObject.activeSelf)
							lb.gameObject.SetActive(true);
					}
				}

				//lz-2016.10.27 多人那边有时候misLb_target需要先添加，后刷新位置
				if (missionLabel.NeedOneRefreshPos)
				{
					RepositionMapLabel(lb);
					missionLabel.NeedOneRefreshPos = false;
				}
			}
		}
        #endregion



        #region User Mask
        protected virtual void OpenMaskWnd(UIMapLabel label)
        {
            if (null != label && label.type != ELabelType.User)
                return;

            mMaskOpWndParent.SetActive(true);
            mWarpWnd.SetActive(false);
            mIconSelWnd.SetActive(false);

            Vector3 arrayAtPos = Vector3.zero;
            if (label == null)
            {
                if (mUserIconList.Count > 0)
                {
                    mMaskSpr.spriteName = mUserIconList[maskIcoIndex].iconName;
                    mMaskSpr_2.spriteName = mUserIconList[maskIcoIndex].iconName;
                    mMaskSpr_2.enabled = true;
                }
                mMaskDes.text = PELocalization.GetString(8000702);
                newUserLabelPos.x = Input.mousePosition.x;
                newUserLabelPos.y = Input.mousePosition.y;
                mMaskSpr_2.transform.localPosition = new Vector3(Input.mousePosition.x - Screen.width / 2
                                                             , Input.mousePosition.y - Screen.height / 2, -78);
                arrayAtPos = mMaskSpr_2.transform.position;
            }
            else
            {
                mMaskSpr.spriteName = label.iconStr;
                mMaskSpr_2.enabled = false;
                mMaskDes.text = label.descText;
                arrayAtPos = label.transform.position;
            }
            //lz-2016.07.18 WarpWnd���ݵ����λ����ʾ�ڵ�ĸ���
            mMaskOpWnd.transform.SetTransInScreenByMousePos(UIToolFuncs.InScreenOffsetMode.OffsetBounds);
            //lz-2016.07.18 ������ʾ��λ����ʾ��Ӧ�ļ�ͷָ������
            RotateArrayByWnd(mMaskOpWndCenter.position, arrayAtPos, mMaskOpArray, 266,54);
            selectMapLabel = label;
        }

        int maskIcoIndex = 0;
        UIMapLabel selectMapLabel = null;
        Vector2 newUserLabelPos = Vector2.zero;

        public void MaskYes()
        {
            if (selectMapLabel == null)
            {
                if (!PeGameMgr.IsMulti)
                {
                    UserLabel useLabel = new UserLabel(); ;
                    if (maskIcoIndex < 0 || maskIcoIndex >= mUserIconList.Count)
                        maskIcoIndex = 0;
                    useLabel.icon = mUserIconList[maskIcoIndex].id;
                    useLabel.pos = GetWorldPos(newUserLabelPos);
                    useLabel.text = mMaskDes.text;

                    UserLabel.Mgr.Instance.Add(useLabel);
                }
                else
                {
                    PlayerNetwork.mainPlayer.RequestMakeMask((byte)0xFF, GetWorldPos(newUserLabelPos), mUserIconList[maskIcoIndex].id, mMaskDes.text);
                }
            }
            else
            {
                UserLabel useLabel = selectMapLabel._ILabel as UserLabel;
                if (useLabel == null)
                    return;

                if (!PeGameMgr.IsMulti)
                {
                    useLabel.icon = mUserIconList[maskIcoIndex].id;
                    useLabel.text = mMaskDes.text;
                    //lz-2016.09.06 ˢ�µ�ǰͼ��
                    selectMapLabel.UpdateIcon();
                }
                else
                {
                    PlayerNetwork.mainPlayer.RequestMakeMask(useLabel.index, useLabel.pos, mUserIconList[maskIcoIndex].id, mMaskDes.text);
                }
            }
        }

        public void ChangeMaskIcon(int index)
        {
            if (index < 0 || index >= mUserIconList.Count)
                return;
            maskIcoIndex = index;
            mMaskSpr.spriteName = mUserIconList[maskIcoIndex].iconName;
            mMaskSpr_2.spriteName = mUserIconList[maskIcoIndex].iconName;
        }

        void UpdatePosLabel()
        {
            if (!mMaskOpWndParent.gameObject.activeSelf && !mWarpWnd.gameObject.activeSelf)
            {
                if (!mMaskOpWndParent.gameObject.activeSelf)
                    mPosLabelTrans.gameObject.SetActive(true);

                Vector3 finalPos = GetWorldPos(new Vector2(Input.mousePosition.x, Input.mousePosition.y));

                if (!mShowDes)
                    mPosLabal.text = ((int)finalPos.x).ToString() + "," + ((int)finalPos.z).ToString();
                mPosLabelTrans.localPosition = new Vector3(Input.mousePosition.x - Screen.width / 2 + 20
                                                                , Input.mousePosition.y - Screen.height / 2 - 10, -20);
            }
            else if (mMaskOpWndParent.gameObject.activeSelf)
                mPosLabelTrans.gameObject.SetActive(false);
        }

        #endregion



        #region FastTravel
        int mMoneyCost = 0;
        Vector3 travelPos = Vector3.zero;
        //int campId = -1;
        int iconId = -1;
        protected virtual void OpenWarpWnd(UIMapLabel label)
        {
            if (label == null)
                return;

            if (RandomDungenMgrData.InDungeon) return;  //lz-2018.01.10 副本中不可以传送

            mWarpWnd.SetActive(true);
            mMaskOpWndParent.SetActive(false);

            float dis = (label.worldPos - GameUI.Instance.mMainPlayer.position).magnitude;
            mMoneyCost = 2 + (int)(dis * 0.02f);

            mItemCost.text = mMoneyCost.ToString();
            mWarpDes.text = label.descText;
            //lz-2016.07.18 WarpWnd���ݵ����λ����ʾ�ڵ�ĸ���
            mWarpWnd.transform.SetTransInScreenByMousePos(UIToolFuncs.InScreenOffsetMode.OffsetBounds);
            //lz-2016.07.18 ���ݴ�����ʾ��λ����ʾ��ͷָ����λ��
            RotateArrayByWnd(mWarpWndCenter.position,label.transform.position,mWarpArray,264,70);
            travelPos = label.worldPos;
            iconId = -1;
            if (!Equals(label._ILabel, null))
                iconId = label._ILabel.GetIcon();

            if (label._ILabel is StaticPoint)
            {
                //StaticPoint sp = (StaticPoint)label._ILabel;
                //campId = sp.campId;
            }
        }

        public System.Action onTravel;
        public void OnWarpYes()
        {
            string strMes;
            if (MissionManager.Instance != null)
            {
                if (MissionManager.Instance.HasTowerDifMission())
                {
                    strMes = PELocalization.GetString(8000002);
                    MessageBox_N.ShowOkBox(strMes);
                    return;
                }

                if (GameUI.Instance.playerMoney < mMoneyCost)
                {
                    strMes = PELocalization.GetString(8000003);
                    MessageBox_N.ShowOkBox(strMes);
                    return;
                }

                if (PeCreature.Instance.mainPlayer.passengerCmpt.IsOnCarrier())
                {
                    strMes = PELocalization.GetString(8000004);
                    MessageBox_N.ShowOkBox(strMes);
                    return;
                }

                int misID = -1;
                if (PeGameMgr.IsMulti)
                {
                    misID = MissionManager.Instance.HasFollowMissionNet();
                }
                else
                    misID = MissionManager.Instance.HasFollowMission();
                if (misID != -1)
                {
                    strMes = PELocalization.GetString(8000005);
                    mOpMissionID = misID;
                    MessageBox_N.ShowYNBox(strMes, FailureMission);
                    return;
                }
            }

            if (!PeGameMgr.IsMulti)
            {
                if (onTravel != null)
                    onTravel.Invoke();
                FastTravel();
            }
            else
            {
                if (null != PlayerNetwork.mainPlayer)
                {
                    //if (-1 == campId || campId == PlayerNetwork.MainPlayer.TeamId)
                    //	PlayerNetwork.MainPlayer.RequestFastTravel(0, travelPos, mMoneyCost);
                    //else
                    //	MessageBox_N.ShowOkBox("This flag is not yours");

                    GameUI.Instance.mUIWorldMap.Hide();
                    int type = iconId == PeMap.MapIcon.FlagIcon ? 1 : 0;
                    PlayerNetwork.mainPlayer.RequestFastTravel(type, travelPos, mMoneyCost);
                    Hide();
                }
            }
        }

        int mOpMissionID = -1;
        void FailureMission()
        {
            if (GameUI.Instance.mMainPlayer != null)
            {
                if (PeGameMgr.IsMulti)
                {
                    PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, mOpMissionID);
                }
                MissionManager.Instance.FailureMission(mOpMissionID);
                if (PeGameMgr.IsMulti)
                {
                    PlayerNetwork.mainPlayer.RequestFastTravel(0, travelPos, 0);
                    Hide();
                }
                else
                    FastTravel();
                mOpMissionID = -1;
            }
        }

        void FastTravel()
        {
            GameUI.Instance.mUIWorldMap.Hide();
            GameUI.Instance.playerMoney -= mMoneyCost;
            Pathea.FastTravelMgr.Instance.TravelTo(travelPos);
        }

        //��ת��ͷ�����ݴ������ڵ�λ��
        void RotateArrayByWnd(Vector3 wndCenterPos, Vector3 labPos, Transform array,float width,float height)
        {
            float x, y, rotate;
            x = y = rotate = 0;
            float offsetX = 4.5f;
            float offsetY = 4.5f;
            if (wndCenterPos.x > labPos.x && wndCenterPos.y < labPos.y) //���������½ǣ���ͷӦ�������Ͻ�
            {
                x = offsetX;
                y = -offsetY;
                rotate = 0;
            }
            else if (wndCenterPos.x < labPos.x && wndCenterPos.y < labPos.y) //���������½ǣ���ͷӦ�������Ͻ�
            {
                x = width - offsetX;
                y = -offsetY;
                rotate = -90;
            }
            else if (wndCenterPos.x < labPos.x && wndCenterPos.y > labPos.y) //���������Ͻǣ���ͷӦ�������½�
            {
                x = width - offsetX;
                y = -height + offsetY;
                rotate = -180;
            }
            else if (wndCenterPos.x > labPos.x && wndCenterPos.y > labPos.y) //���������Ͻǣ���ͷӦ�������½�
            {
                x = offsetX;
                y = -height + offsetY;
                rotate = -270;
            }
            array.transform.localPosition = new Vector3(x, y,-82);
            array.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, rotate));
        }

        #endregion
    }
}
