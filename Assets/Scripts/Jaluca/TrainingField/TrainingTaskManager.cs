using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

namespace TrainingScene
{
    public class TrainingTaskManager : MonoBehaviour
    {
        private static TrainingTaskManager s_instance;
        public static TrainingTaskManager Instance { get { return s_instance; } }
        private GameObject mMissionArrow;

        void Awake()
        {
            s_instance = this;
            mWareHouse = Instantiate(mWareHouseObj);
            mMissionArrow = GameUI.Instance.mUIMinMapCtrl.mSubInfoPanel.gameObject;
        }
        
        //lz-2016.08.03 销毁的时候取消注册的事件
        void OnDestroy()
        {
            if (null!=UICompoundWndControl.OnShow)
            {
                UICompoundWndControl.OnShow -= Replicator;
            }

            if (GameUI.Instance != null && GameUI.Instance.mBuildBlock.onSaveIsoClick != null)
            {
                GameUI.Instance.mBuildBlock.onSaveIsoClick -= CompleteMission;
            }

            if (BSIsoBrush.onBrushDo != null)
            {
                BSIsoBrush.onBrushDo -= CompleteMission;
            }
        }

        /// <summary>
        /// 关闭EnterArea的指示箭头
        /// </summary>
        public void CloseMissionArrow()
        {
            if (mMissionArrow == null)
                return;
            if (mMissionArrow.transform.FindChild("MissionArrow(Clone)") == null)
                return;
            Transform misArrowTr = mMissionArrow.transform.FindChild("MissionArrow(Clone)");
            misArrowTr.gameObject.SetActive(false);
        }

        /// <summary>
        /// 关闭小地图怪的点
        /// </summary>
        public void CloseMonsterPoint()
        {
            UISprite[] mMapPoints;
            if (mMissionArrow == null)
                return;
            mMapPoints = mMissionArrow.GetComponentsInChildren<UISprite>();
            if (mMapPoints.Length == 0)
                return;
            for (int i = 0; i < mMapPoints.Length; i++)
            {
                if (mMapPoints[i].spriteName == "sign_monster")
                {
                    mMapPoints[i].transform.parent.gameObject.SetActive(false);
                }
            }
        }

        [HideInInspector]   MissionManager mm;
        [HideInInspector]   public TrainingTaskType currentMission;
        [HideInInspector]   public static bool isNewGame;
        [HideInInspector]   public GameObject mWareHouse;
        [SerializeField]    GameObject mWareHouseObj;

        void SetNPCsAtrribType() 
        {
            List<int> NpcId = new List<int> { 9004, 9005, 9006, 9009, 9020, 9031, 9041 };
            PeEntity npc;
            foreach (var item in NpcId)
            {
                npc = EntityMgr.Instance.Get(item);
                if (npc == null)
                    continue;
                npc.skEntity.SetAttribute((int)AttribType.CampID, 0);
                npc.skEntity.SetAttribute((int)AttribType.DamageID,0);
            }
        }

        void Start()
        {
            SetNPCsAtrribType();
            StartCoroutine(MissionStart());
            //Invoke("MissionStartSelf", 5);

            CreationMgr.Init();
            ForceSetting.Instance.Load(Resources.Load("ForceSetting/ForceSettings_Story") as TextAsset);
            mWareHouse.SetActive(false);
        }

        void MissionStartSelf()
        {
            PeEntity mNpc = EntityMgr.Instance.Get(9041);
            if (mNpc == null)
                return;
            GameUI.Instance.mNpcWnd.SetCurSelNpc(mNpc);
            GameUI.Instance.mNpcWnd.OnMutexBtnClick(739);
        }

        public void CompleteMission(int missionId) 
        {
            switch (missionId)
            {
                case TrainingRoomConfig.GetMed:
                    GameUI.Instance.mWarehouse.Hide();
                    break;
                case TrainingRoomConfig.PutMed:
                    GameUI.Instance.mItemPackageCtrl.Hide();

                    //if (MissionRepository.HaveTalkOP(TrainingRoomConfig.UseJuice))
                    //{
                    //    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    //    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(TrainingRoomConfig.UseJuice, 1);
                    //    GameUI.Instance.mNPCTalk.PreShow();
                    //}
                    //else if (MissionManager.Instance.IsGetTakeMission(TrainingRoomConfig.UseJuice))
                    //    MissionManager.Instance.SetGetTakeMission(TrainingRoomConfig.UseJuice, null, MissionManager.TakeMissionType.TakeMissionType_Get);
                    break;
                case TrainingRoomConfig.UseJuice:
                    GameUI.Instance.mWarehouse.Hide();

                    //if (MissionRepository.HaveTalkOP(TrainingRoomConfig.EquipKnife))
                    //{
                    //    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    //    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(TrainingRoomConfig.EquipKnife, 1);
                    //    GameUI.Instance.mNPCTalk.PreShow();
                    //}
                    //else if (MissionManager.Instance.IsGetTakeMission(TrainingRoomConfig.EquipKnife))
                    //    MissionManager.Instance.SetGetTakeMission(TrainingRoomConfig.EquipKnife, null, MissionManager.TakeMissionType.TakeMissionType_Get);
                    break;
                case TrainingRoomConfig.EquipKnife:
                    GameUI.Instance.mItemPackageCtrl.Hide();

                    //if (MissionRepository.HaveTalkOP(TrainingRoomConfig.Fighting))
                    //{
                    //    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    //    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(TrainingRoomConfig.Fighting, 1);
                    //    GameUI.Instance.mNPCTalk.PreShow();
                    //}
                    //else if (MissionManager.Instance.IsGetTakeMission(TrainingRoomConfig.Fighting))
                    //    MissionManager.Instance.SetGetTakeMission(TrainingRoomConfig.Fighting, null, MissionManager.TakeMissionType.TakeMissionType_Get);
                    break;
                case TrainingRoomConfig.CutID:
                    //if (MissionRepository.HaveTalkOP(TrainingRoomConfig.Replicator))
                    //{
                    //    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    //    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(TrainingRoomConfig.Replicator, 1);
                    //    GameUI.Instance.mNPCTalk.PreShow();
                    //}
                    //else if (MissionManager.Instance.IsGetTakeMission(TrainingRoomConfig.Replicator))
                    //    MissionManager.Instance.SetGetTakeMission(TrainingRoomConfig.Replicator, null, MissionManager.TakeMissionType.TakeMissionType_Get);
                    break;
                case TrainingRoomConfig.Replicator:
                    GameUI.Instance.mItemPackageCtrl.Hide();

                    //if (MissionRepository.HaveTalkOP(TrainingRoomConfig.SynID))
                    //{
                    //    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    //    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(TrainingRoomConfig.SynID, 1);
                    //    GameUI.Instance.mNPCTalk.PreShow();
                    //}
                    //else if (MissionManager.Instance.IsGetTakeMission(TrainingRoomConfig.SynID))
                    //    MissionManager.Instance.SetGetTakeMission(TrainingRoomConfig.SynID, null, MissionManager.TakeMissionType.TakeMissionType_Get);
                    break;
                case TrainingRoomConfig.SynID:
                    GameUI.Instance.mCompoundWndCtrl.Hide();

                    //if (MissionRepository.HaveTalkOP(TrainingRoomConfig.DigID))
                    //{
                    //    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    //    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(TrainingRoomConfig.DigID, 1);
                    //    GameUI.Instance.mNPCTalk.PreShow();
                    //}
                    //else if (MissionManager.Instance.IsGetTakeMission(TrainingRoomConfig.DigID))
                    //    MissionManager.Instance.SetGetTakeMission(TrainingRoomConfig.DigID, null, MissionManager.TakeMissionType.TakeMissionType_Get);
                    break;
                case TrainingRoomConfig.DigID:
                    TerrainDigTask.Instance.DestroyScene();

                    //if (MissionRepository.HaveTalkOP(TrainingRoomConfig.BuildInID))
                    //{
                    //    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    //    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(TrainingRoomConfig.BuildInID, 1);
                    //    GameUI.Instance.mNPCTalk.PreShow();
                    //}
                    //else if (MissionManager.Instance.IsGetTakeMission(TrainingRoomConfig.BuildInID))
                    //    MissionManager.Instance.SetGetTakeMission(TrainingRoomConfig.BuildInID, null, MissionManager.TakeMissionType.TakeMissionType_Get);
                    break;
                case TrainingRoomConfig.BuildInID:
                    TerrainDigTask.Instance.DestroyScene();

                    //if (MissionRepository.HaveTalkOP(TrainingRoomConfig.BuildPoint))
                    //{
                    //    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    //    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(TrainingRoomConfig.BuildPoint, 1);
                    //    GameUI.Instance.mNPCTalk.PreShow();
                    //}
                    //else if (MissionManager.Instance.IsGetTakeMission(TrainingRoomConfig.BuildPoint))
                    //    MissionManager.Instance.SetGetTakeMission(TrainingRoomConfig.BuildPoint, null, MissionManager.TakeMissionType.TakeMissionType_Get);
                    break;
                case TrainingRoomConfig.BuildPoint:
                    EmitlineTask.Instance.CloseMission_buildPoint();
                    EmitlineTask.Instance.CloseMission_buildCreateISO();
                    break;
                default:
                    break;
            }
        }

        void Replicator()
        {
            GameUI.Instance.mNPCTalk.NormalOrSP(0);
            GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(new List<int> { 300241 });
            GameUI.Instance.mNPCTalk.PreShow();

            //lz-2016.08.03 这里注销事件写错了
            UICompoundWndControl.OnShow -= Replicator;
            //UIWarehouse.OnShow -= Replicator;
        }

        public void InitMission(int missionId)
        {
            switch (missionId)
            {
                case TrainingRoomConfig.ChangeControlModeID:
                    NewMoveTask.Instance.InitFirstMission();
                    currentMission = TrainingTaskType.ChangeControlMode;
                    break;
                case TrainingRoomConfig.BaseControlID:
                    NewMoveTask.Instance.InitBaseControl();
                    currentMission = TrainingTaskType.BaseControl;
                    break;
                case TrainingRoomConfig.MoveID:
                    NewMoveTask.Instance.InitMoveScene();
                    currentMission = TrainingTaskType.Move;
                    break;
                case TrainingRoomConfig.GetMed:
                    NewMoveTask.Instance.InitGetMedicineScene();
                    currentMission = TrainingTaskType.GetMedicine;
                    break;
                case TrainingRoomConfig.BackToAndy:
                    currentMission = TrainingTaskType.BackToAndy;
                    break;
                case TrainingRoomConfig.OpenPackage:
                    UseItemTask.Instance.InitOpenPackageScene();
                    currentMission = TrainingTaskType.OpenPack;
                    break;
                case TrainingRoomConfig.PutMed:
                    UseItemTask.Instance.InitPutMedScene();
                    currentMission = TrainingTaskType.PutMed;
                    break;
                case TrainingRoomConfig.EquipKnife:
                    UseItemTask.Instance.InitEquipKnifeScene();
                    currentMission = TrainingTaskType.EquipKnife;
                    break;
                case TrainingRoomConfig.Fighting:
                    currentMission = TrainingTaskType.Fighting;
                    break;
                case TrainingRoomConfig.GatherID:
                    HoloherbTask.Instance.InitScene();
                    currentMission = TrainingTaskType.GATHER;
                    break;
                case TrainingRoomConfig.CutID:
                    HolotreeTask.Instance.InitScene();
                    currentMission = TrainingTaskType.CUT;
                    break;
                case TrainingRoomConfig.SynID:
                    UICompoundWndControl.OnShow += Replicator;
                    break;
                case TrainingRoomConfig.DigID:
                    TerrainDigTask.Instance.InitScene();
                    currentMission = TrainingTaskType.DIG;
                    break;
                case TrainingRoomConfig.BuildMenu:
                    TerrainDigTask.Instance.InitMenuBtn();
                    currentMission = TrainingTaskType.BuildMenu;
                    break;
                case TrainingRoomConfig.BuildInID:
                    EmitlineTask.Instance.InitInBuildScene();
                    currentMission = TrainingTaskType.BUILDIn;
                    break;
                case TrainingRoomConfig.BuildPoint:
                    EmitlineTask.Instance.InitScene();
                    currentMission = TrainingTaskType.BuildPoint;
                    break;
                case TrainingRoomConfig.CreateIso:
                    currentMission = TrainingTaskType.CreateIso;
                    break;
                case TrainingRoomConfig.Replicator:
                    currentMission = TrainingTaskType.Replicator;
                    break;
                case TrainingRoomConfig.BuildSaveIso:
                    GameUI.Instance.mBuildBlock.onSaveIsoClick += CompleteMission;
                    currentMission = TrainingTaskType.BuildSaveIso;
                    break;
                case TrainingRoomConfig.BuildExportIso:
                    EmitlineTask.Instance.CreateExportIsoCube();
                    BSIsoBrush.onBrushDo += CompleteMission;
                    currentMission = TrainingTaskType.BuildExpotIso;
                    break;
                case TrainingRoomConfig.MissionPlane:
                    currentMission = TrainingTaskType.MissionPlane;
                    break;
                default:
                    break;
            }

            if (null != GameUI.Instance)
            {
                //lz-2016.11.07 UI上的Tutorial提示统一管理检测
                GameUI.Instance.CheckMissionIDShowTutorial(missionId);
            }
        }

        public void CompleteMission()
        {
            if (currentMission == TrainingTaskType.ChangeControlMode)
            {
                if (PeGameMgr.IsMulti)
                    mm.RequestCompleteMission(TrainingRoomConfig.ChangeControlModeID);
                else
                    mm.CompleteMission(TrainingRoomConfig.ChangeControlModeID);
            }
            else if (currentMission == TrainingTaskType.BaseControl)
            {
                if (PeGameMgr.IsMultiStory)
                    mm.RequestCompleteMission(TrainingRoomConfig.BaseControlID);
                else
                    mm.CompleteMission(TrainingRoomConfig.BaseControlID);
            }
            else if (currentMission == TrainingTaskType.Move)
            {
                if (PeGameMgr.IsMultiStory)
                    mm.RequestCompleteMission(TrainingRoomConfig.MoveID);
                else
                    mm.CompleteMission(TrainingRoomConfig.MoveID);
            }
            else if (currentMission == TrainingTaskType.BackToAndy)
            {
                if (PeGameMgr.IsMulti)
                    mm.RequestCompleteMission(TrainingRoomConfig.BackToAndy);
                else
                    mm.CompleteMission(TrainingRoomConfig.BackToAndy);
            }
            else if (currentMission == TrainingTaskType.OpenPack)
            {
                if (PeGameMgr.IsMulti)
                    mm.RequestCompleteMission(TrainingRoomConfig.OpenPackage);
                else
                    mm.CompleteMission(TrainingRoomConfig.OpenPackage);
            }
            else if (currentMission == TrainingTaskType.PutMed)
            {
                if (PeGameMgr.IsMulti)
                    mm.RequestCompleteMission(TrainingRoomConfig.PutMed);
                else
                    mm.CompleteMission(TrainingRoomConfig.PutMed);
            }
            else if (currentMission == TrainingTaskType.EquipKnife)
            {
                if (PeGameMgr.IsMulti)
                    mm.RequestCompleteMission(TrainingRoomConfig.EquipKnife);
                else
                    mm.CompleteMission(TrainingRoomConfig.EquipKnife);
            }
            else if (currentMission == TrainingTaskType.GATHER)
            {
                if (PeGameMgr.IsMulti)
                    mm.RequestCompleteMission(TrainingRoomConfig.GatherID);
                else
                    mm.CompleteMission(TrainingRoomConfig.GatherID);
            }
            else if (currentMission == TrainingTaskType.CUT)
            {
                if (PeGameMgr.IsMulti)
                    mm.RequestCompleteMission(TrainingRoomConfig.CutID);
                else
                    mm.CompleteMission(TrainingRoomConfig.CutID);
            }
            else if (currentMission == TrainingTaskType.DIG)
            {
                if (PeGameMgr.IsMulti)
                    mm.RequestCompleteMission(TrainingRoomConfig.DigID);
                else
                    mm.CompleteMission(TrainingRoomConfig.DigID);
            }
            else if (currentMission == TrainingTaskType.BUILDIn)
            {
                if (PeGameMgr.IsMulti)
                    mm.RequestCompleteMission(TrainingRoomConfig.BuildInID);
                else
                    mm.CompleteMission(TrainingRoomConfig.BuildInID);
            }
            else if (currentMission == TrainingTaskType.BuildPoint)
            {
                if (PeGameMgr.IsMulti)
                    mm.RequestCompleteMission(TrainingRoomConfig.BuildPoint);
                else
                    mm.CompleteMission(TrainingRoomConfig.BuildPoint);
            }
            else if (currentMission == TrainingTaskType.BuildMenu)
            {
                if (PeGameMgr.IsMultiStory)
                    mm.RequestCompleteMission(TrainingRoomConfig.BuildMenu);
                else
                    mm.CompleteMission(TrainingRoomConfig.BuildMenu);
            }
            else if (currentMission == TrainingTaskType.BuildSaveIso)
            {
                GameUI.Instance.mBuildBlock.onSaveIsoClick -= CompleteMission;
                if (PeGameMgr.IsMulti)
                    mm.RequestCompleteMission(TrainingRoomConfig.BuildSaveIso);
                else
                    mm.CompleteMission(TrainingRoomConfig.BuildSaveIso);
            }
            else if (currentMission == TrainingTaskType.BuildExpotIso)
            {
                BSIsoBrush.onBrushDo -= CompleteMission;
                if (PeGameMgr.IsMulti)
                    mm.RequestCompleteMission(TrainingRoomConfig.BuildExportIso);
                else
                    mm.CompleteMission(TrainingRoomConfig.BuildExportIso);
            }
            //else if (currentMission == TrainingTaskType.CreateIso)
            //{
            //    //VCEditor.OnMakeCreation -= new VCEditor.DNoParam(CompleteMission);
            //    if (PeGameMgr.IsMultiStory)
            //        mm.RequestCompleteMission(TrainingRoomConfig.CreateIso);
            //    else
            //        mm.CompleteMission(TrainingRoomConfig.CreateIso);
            //}
            else if (currentMission == TrainingTaskType.Replicator)
            {
                if (PeGameMgr.IsMulti)
                    mm.RequestCompleteMission(TrainingRoomConfig.Replicator);
                else
                    mm.CompleteMission(TrainingRoomConfig.Replicator);
            }
        }

        IEnumerator MissionStart()
        {
            while (true)
            {
                if (Pathea.PeCreature.Instance.mainPlayer)
                {
                    mm = MissionManager.Instance;
                    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(739, 1);
                    GameUI.Instance.mNPCTalk.PreShow();
                    //mm.SetGetTakeMission(739, EntityMgr.Instance.Get(MissionManager.GetMissionCommonData(718).m_iNpc),
                    //                                          MissionManager.TakeMissionType.TakeMissionType_Get, false);
                    //if (PeGameMgr.IsMultiStory)
                    //    MissionManager.Instance.RequestCompleteMission(739);
                    //else
                    //    mm.CompleteMission(739);
                    break;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}