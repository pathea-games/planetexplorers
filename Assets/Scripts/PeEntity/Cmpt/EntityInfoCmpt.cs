using UnityEngine;
using System.Collections;
using Pathea.PeEntityExt;

public class CharacterName
{
    const string DefaultFamilyName = "FamilyName";
    const string DefaultGivenName = "GivenName";

    static CharacterName sDefault = null;
    public static CharacterName Default
    {
        get
        {
            if (null == sDefault)
            {
                sDefault = new CharacterName(DefaultGivenName, DefaultFamilyName);
            }
            return sDefault;
        }
    }

    string mFamilyName;
    string mGivenName;
    string mFullName;

    CharacterName(string fullName, string givenName, string familyName)
    {
        mFullName = fullName;
        mGivenName = givenName;
        mFamilyName = familyName;
    }

    public CharacterName(string givenName, string familyName)
        : this(null, givenName, familyName)
    {
    }

    public CharacterName(string name)
        : this(name, name, null)
    {
    }

    public CharacterName() { }

	public CharacterName InitStoryNpcName(string fullName,string showName){
		mFullName = fullName;
		mGivenName = showName;
		return this;
	}
	
	public string OverHeadName{
		get{return givenName;}
	}



    public string givenName
    {
        get
        {
            return mGivenName;
        }
    }

    public string familyName
    {
        get
        {
            return mFamilyName;
        }
    }

    string mConcatFullName = null;
    public string fullName
    {
        get
        {
            if (null != mFullName)
            {
                return mFullName;
            }

            if (mConcatFullName == null)
            {
				if(familyName==null)
					mConcatFullName = givenName;
				else if(givenName == null)
					mConcatFullName = familyName;
				else
                	mConcatFullName = string.Concat(givenName, " ", familyName);
            }

            return mConcatFullName;
        }
    }

    public void Import(byte[] data)
    {
        PETools.Serialize.Import(data, (r) =>
        {
            mFullName = PETools.Serialize.ReadNullableString(r);
            mFamilyName = PETools.Serialize.ReadNullableString(r);
            mGivenName = PETools.Serialize.ReadNullableString(r);
        });
    }

    public byte[] Export()
    {
        return PETools.Serialize.Export((w) =>
        {
            PETools.Serialize.WriteNullableString(w, mFullName);
            PETools.Serialize.WriteNullableString(w, mFamilyName);
            PETools.Serialize.WriteNullableString(w, mGivenName);
        }, 20);
    }

    public override bool Equals(object obj)
    {
        CharacterName other = obj as CharacterName;

        if (null == obj)
        {
            return false;
        }

        if (base.Equals(obj))
        {
            return true;
        }

        if (string.Equals(other.mFullName, mFullName)
            && string.Equals(other.mGivenName, mGivenName))
        {
            return true;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return fullName;
    }
}

namespace Pathea
{
    public class EntityInfoCmpt : PeCmpt, IPeMsg
    {
        public const int VERSION_0000 = 0;
        public const int CURRENT_VERSION = VERSION_0000;
		public const string OverHeadPrefabPath = "Prefab/Npc/Component/OverHead";

        CommonCmpt mCommon;
        //PeTrans mView;
        NpcOverHead mHeadInfo;
        SkAliveEntity mAliveEntity;

        CharacterName mCharacterName;
        Texture mFaceTex;
        string mFaceIcon;
        string mFaceIconBig;
        string mShopIcon;
        NpcMissionState mMissionState = NpcMissionState.Max;
        int mMapIcon = 21;

        public static GameObject _overheadTmplGo = null;

        Transform GetOverHeadParentTrans()
        {
            return transform;
        }

        NpcOverHead LoadHeadInfo(Transform parentTrans)
        {
            // Transform parentTrans = GetOverHeadParentTrans();
            if (null == parentTrans)
            {
                return null;
            }

            if (_overheadTmplGo == null)
            {
                _overheadTmplGo = Resources.Load(OverHeadPrefabPath) as GameObject;
                if (null == _overheadTmplGo)
                {
                    Debug.LogError("Load [" + OverHeadPrefabPath + "] error.");
                    return null;
                }
            }

            GameObject overHeadObj = GameObject.Instantiate(_overheadTmplGo) as GameObject;
            if (null == overHeadObj)
            {
                Debug.LogError("Load [" + OverHeadPrefabPath + "] error.");
                return null;
            }
            overHeadObj.transform.parent = parentTrans;
            return overHeadObj.GetComponent<NpcOverHead>();
        }

        bool GetNameColor()
        {
            return true;
        }

        void SyncObjectName()
        {
#if true //UNITY_EDITOR
            GameObject obj = Entity.GetGameObject();
            if (null != obj)
            {
                obj.name = characterName.givenName + "_" + Entity.Id;
            }
#endif
        }

        public override void Awake()
        {
			base.Awake ();
            // mHeadInfo = LoadHeadInfo();

            //string Icon = StoreRepository.GetStoreNpcIcon(npcid);
        }

        void IPeMsg.OnMsg(EMsg msg, params object[] args)
        {
            switch (msg)
            {
                case EMsg.View_Model_Build:
                    {
                        GameObject obj = args[0] as GameObject;
                        if (null != obj)
                        {
                            mHeadInfo = LoadHeadInfo(obj.transform.parent);
                            InitOverHead();
                        }
                    }
                    break;
                case EMsg.View_Model_Destroy:
                    {

                    } break;

                default:
                    break;
            }
        }


        #region override

        public override void Start()
        {
			base.Start ();
			//mView = Entity.peTrans;
            mCommon = Entity.GetCmpt<CommonCmpt>();
            mAliveEntity = Entity.GetCmpt<SkAliveEntity>();

            if (mAliveEntity != null)
            {
                //mAliveEntity.onHpChange += mHeadInfo.OnHpChange;
                mAliveEntity.deathEvent += OndeathEnvent;
            }

            // mHeadInfo.SetTheEntity(Entity);
            //mHeadInfo.InitTheentity(Entity);
            SyncObjectName();
            SetVisiable(true);
            // SyncInfo();
        }

        bool bShowrevival = false;
        void OndeathEnvent(SkillSystem.SkEntity self, SkillSystem.SkEntity carster)
        {
            bShowrevival = true;
        }

        void OnHpChange(SkillSystem.SkEntity caster, float hpChange)
        {
            if (mHeadInfo != null)
                mHeadInfo.HpChange(caster, hpChange);
        }

        //public void UpdateHeadPos()
        //{
        //    if (mHeadInfo != null && mView != null)
        //    {
        //        mHeadInfo.transform.position = mView.headTop;
        //        mHeadInfo.SetBouds(mView.bound);
        //    }
        //}

        public NpcOverHead OverHead { get { return this.mHeadInfo;} }

        public override void OnUpdate()
        {
            Updatarevival();
        }

        public override void Deserialize(System.IO.BinaryReader r)
        {
            int Version = r.ReadInt32();
            if (Version > CURRENT_VERSION)
            {
                Debug.LogError("version error");
                return;
            }

            byte[] data = PETools.Serialize.ReadBytes(r);
            if (data != null)
            {
                mCharacterName = new CharacterName();
                mCharacterName.Import(data);
            }

            mFaceIcon = PETools.Serialize.ReadNullableString(r);
            mFaceIconBig = PETools.Serialize.ReadNullableString(r);
            mShopIcon = PETools.Serialize.ReadNullableString(r);
            mMissionState = (NpcMissionState)r.ReadInt32();
            mMapIcon = r.ReadInt32();

            Invoke("RefreshState", 2f);
        }

        void RefreshState()
        {
            NpcMissionData missionData = mCommon.Entity.GetUserData() as NpcMissionData;
            if (missionData == null)
                mMissionState = NpcMissionState.Max;
            else
                MissionManager.Instance.m_PlayerMission.UpdateNpcMissionTex(mCommon.Entity);
        }

        public override void Serialize(System.IO.BinaryWriter w)
        {
            w.Write(CURRENT_VERSION);

            byte[] data = null;
            if (mCharacterName != null)
            {
                data = mCharacterName.Export();
            }
            PETools.Serialize.WriteBytes(data, w);

            PETools.Serialize.WriteNullableString(w, mFaceIcon);
            PETools.Serialize.WriteNullableString(w, mFaceIconBig);
            PETools.Serialize.WriteNullableString(w, mShopIcon);
            w.Write((int)mMissionState);
            w.Write(mMapIcon);

        }

        #endregion

        #region public function
        public Texture faceTex
        {
            get
            {
                if (mFaceTex == null)
                {
                    mFaceTex = TakePhoto();
                }

                return mFaceTex;
            }
            set
            {
                mFaceTex = value;
            }
        }

        Texture TakePhoto()
        {
            BiologyViewCmpt v = Entity.biologyViewCmpt;
            if (v == null/*|| v.modelTrans == null*/) //lz-2016.07.23 PeViewStudio.TakePhoto 里面处理了没有模型的情况，这里不用返回
            {
                return null;
            }

            CommonCmpt c = Entity.commonCmpt;
            if (c == null)
            {
                return null;
            }

            return PeViewStudio.TakePhoto(v, 64, 64, PeViewStudio.s_HeadPhotoPos, PeViewStudio.s_HeadPhotoRot);//PhotoStudio.Instance.TakePhoto(v.modelTrans.gameObject, (int)c.sex);
        }

        public string faceIcon
        {
            get
            {
                return mFaceIcon;
            }
            set
            {
                mFaceIcon = value;
            }
        }

        public string faceIconBig
        {
            get
            {
                if (string.IsNullOrEmpty(mFaceIconBig))
                {
                    return "";
                }

                return mFaceIconBig;
            }
            set
            {
                mFaceIconBig = value;
            }
        }

        public CharacterName characterName
        {
            get
            {
                return null == mCharacterName ? CharacterName.Default : mCharacterName;
            }
            set
            {
                mCharacterName = value;

                if (mHeadInfo != null)
                    mHeadInfo.SetNpcShowName(characterName.givenName);

                SyncObjectName();
            }
        }

        public string shopIcon
        {
            get
            {
                return mShopIcon;
            }
            set
            {
                mShopIcon = value;
            }
        }

        public NpcMissionState MissionState
        {
            get
            {
                return mMissionState;
            }
            //set
            //{
            //    mMissionState = value;
            //}
        }

        public void SetVisiable(bool flag)
        {
            if (null == mHeadInfo)
            {
                return;
            }

            mHeadInfo.Visiable = flag;
            if (flag)
            {
                SyncInfo();
            }
            else
            {
                mHeadInfo.Reset();
            }
        }

        void InitOverHead()
        {
            if (mHeadInfo == null)
                return;


            mAliveEntity.onHpChange += OnHpChange;
            mHeadInfo.SetTheEntity(Entity);
            mHeadInfo.InitTheentity(Entity);
            if (mCommon != null && mCommon.entityProto != null)
            {
                if (mCommon.entityProto.proto != EEntityProto.Npc && mCommon.entityProto.proto != EEntityProto.RandomNpc)
                    mMissionState = NpcMissionState.Max;
            }
            SyncInfo();

        }

        public void SyncInfo()
        {
            if (null == mHeadInfo || mCommon == null)
            {
                return;
            }

            if (mCommon.entityProto != null)
            {
//                if (Entity.entityProto.proto == EEntityProto.RandomNpc)
//                {
//                    mHeadInfo.SetNpcShowName(characterName.givenName);
//                }
//                else if (Entity.entityProto.proto == EEntityProto.Player)
//                {
//                    mHeadInfo.SetNpcShowName(characterName.givenName);
//                }
//                else
//                {
			    mHeadInfo.SetNpcShowName(characterName.OverHeadName);
//                    if (characterName.familyName != null)
//                        mHeadInfo.SetNpcShowName(characterName.familyName);
//                    else
//                        mHeadInfo.SetNpcShowName(characterName.givenName);
//                }
                mHeadInfo.SetProto(mCommon.entityProto.proto);
                mHeadInfo.CurEEntityProto = mCommon.entityProto.proto;
            }

            mHeadInfo.SetNpcShopIcon(shopIcon);
            mHeadInfo.SetNameColord(GetNameColor());
            mHeadInfo.SetState(MissionState);

            if (Entity.entityProto.proto == EEntityProto.RandomNpc)
            {
                string Icon = StoreRepository.GetStoreNpcIcon(Entity.entityProto.protoId);
                mHeadInfo.SetShowIcon(Icon);
            }
            else
            {
                string Icon = StoreRepository.GetStoreNpcIcon(Entity.Id);
                mHeadInfo.SetShowIcon(Icon);
            }
        }


        //float m_startTime = 0;
        float m_delaytime = 0;
        float m_endtime = 0;
        bool SetTime = false;
        public void SetDelaytime(float startTime, float delaytime)
        {
            //m_startTime = startTime;
            m_delaytime = delaytime;

            m_endtime = startTime + delaytime;

            SetTime = true;
        }

        void Updatarevival()
        {
            if (!bShowrevival)
                return;

            if (!SetTime)
                return;

            float Repersent = 1 - (m_endtime - Time.time) / m_delaytime;
            if (Repersent > 1)
            {
                bShowrevival = false;
                SetTime = false;
            }

            SetRevivalMark(bShowrevival, Repersent);


        }

        public void ShowName(bool show)
        {
            if (mHeadInfo != null)
            {
                mHeadInfo.NameLbShow(show);
            }
        }

        public void ShowBlood(bool show)
        {
            if (mHeadInfo != null)
            {
                mHeadInfo.BloodShow(show);
            }
        }

        public void ShowMissionMark(bool show)
        {
			if(mHeadInfo != null)
			{
				mHeadInfo.ShowMissionMark(show);
			}
        }

        public void SetMissionState(NpcMissionState state)
        {
            mMissionState = state;

            SyncInfo();
        }

        public void SetRevivalMark(bool show, float percent)
        {
            if (null != mHeadInfo)
            {
                mHeadInfo.SetRevivalMark(mCommon.entityProto.proto, show, percent);
                mHeadInfo.SetNameShow(!show);
            }
        }

        public int mapIcon
        {
            get
            {
                return mMapIcon;
            }

            set
            {
                mMapIcon = value;
            }
        }

        public void NpcSayOneWord(int _contentId, float _interval, ENpcSpeakType _type)
        {
            Texture tex = faceTex;
            string _content = "";
            string _npcName = "";
            TalkData talkdata = TalkRespository.GetTalkData(_contentId); ;

//            if (Entity.entityProto.proto == EEntityProto.RandomNpc)
//                _npcName = characterName.givenName + ":";
//            else if (Entity.entityProto.proto == EEntityProto.Npc)
//                _npcName = characterName.familyName + ":";
			_npcName = characterName.fullName + ":";

            if (talkdata != null)
            {
                _content = talkdata.m_Content;
                //lz-2016.11.04 替换npc说话内容中的玩家名字
                _content=_content.Replace("\"name%\"", Pathea.PeCreature.Instance.mainPlayer.ToString());
            }

            if (_type == ENpcSpeakType.Topleft)
                new PeTipMsg(_npcName + _content, tex, PeTipMsg.EMsgLevel.Norm);
            else if (_type == ENpcSpeakType.TopHead)
            {
                //Log:lz-2016.04.29:唐小力 错误 #1851仆从头上的提示信息取消名字开头
                if (mHeadInfo != null)
                    mHeadInfo.SayOneWord(_content, _interval);
            }
            else if (_type == ENpcSpeakType.Both)
            {
                new PeTipMsg(_npcName+_content, tex, PeTipMsg.EMsgLevel.Norm);
                //Log:lz-2016.04.29:唐小力 错误 #1851仆从头上的提示信息取消名字开头
                if (mHeadInfo != null)
                    mHeadInfo.SayOneWord(_content, _interval);
            }
        }
        #endregion
    }
}