using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using ItemAsset;
using ItemAsset.PackageHelper;

namespace TrainingScene
{
    public class NewMoveTask : MonoBehaviour
    {
        static NewMoveTask mInstance = null;
        public static NewMoveTask Instance { get { return mInstance; } }

        public NewMoveTaskAppearance appearance1;
        public NewMoveTaskAppearance1 appearance2;

        HoloCameraControl hcc;

        Vector3 mTarPos;
        //Vector3 mBackPos;

        PlayerPackageCmpt ppc;
        ItemPackage mItemPac;

        int num = 0;

        bool isMoveComplete = false;//移动任务是否完成
        //bool isBackComplete = false;//返回Andy身边
        bool isGetMedComplete = false;//取药是否完成
        //bool isFirstComplete = false;

        void Awake()
        {
            mInstance = this;
            mTarPos = new Vector3(7f, 1.5f, 12f);
            //mBackPos = new Vector3(32f, 4f, 12f);
        }

        void Start()
        {
            hcc = HoloCameraControl.Instance;
            ppc = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
            mItemPac = ppc.package._playerPak;
            //ppc.getItemEventor.Subscribe(OnItemAdd);
        }

        //lz-2016.08.03 销毁的时候取消注册的事件
        void OnDestroy()
        {
            if (null != UIWarehouse.OnShow)
            {
                UIWarehouse.OnShow -= OpenWareHouseTalk;
            }
            PeCamera.onControlModeChange -= OnControlModeChange;
            //if (null != PeCamera.onControlModeChange)
            //{
            //    PeCamera.onControlModeChange -= OnControlModeChange;
            //}
        }

        void OnItemAdd(object sender, PlayerPackageCmpt.GetItemEventArg evt)
        {
            if (evt.protoId == 916)//草药
            {
                ++num;
                ++num;
                if (num >= 2)
                {
                    num = 0;
                    DestroyGetMedicineScene();
                    TrainingTaskManager.Instance.CompleteMission();
                    ppc.getItemEventor.Unsubscribe(OnItemAdd);
                }
            }
        }

        bool HasItemObj()
        {
            if (mItemPac.FindItemByProtoId(916) != null)
            {
                return true;
            }

            return false;
        }

        void Update()
        {
            //完成移动任务的触发
            if (null != GameUI.Instance.mMainPlayer && !isMoveComplete)
            {
                if (Vector3.Distance(GameUI.Instance.mMainPlayer.position, mTarPos) <= 3f)
                {
                    if (MissionManager.Instance.HasMission(TrainingRoomConfig.MoveID))
                    {
                        isMoveComplete = true;
                        TrainingTaskManager.Instance.CompleteMission();
                        DestroyMoveScene();
                    }
                }
            }

            //完成取药任务的触发
            if (HasItemObj() && !isGetMedComplete)
            {
                isGetMedComplete = true;
                DestroyGetMedicineScene();
                TrainingTaskManager.Instance.CompleteMission();
            }

            ////完成返回任务的触发
            if (TrainingTaskManager.Instance.currentMission == TrainingTaskType.BaseControl)
            {
                UIMissionMgr.MissionView missview = UIMissionMgr.Instance.GetMissionView(TrainingRoomConfig.BaseControlID);
                if (missview != null)
                {
                    UIMissionMgr.TargetShow tarshow;
                    if (player.motionMgr.IsActionRunning(PEActionType.Move))
                    {
                        tarshow = missview.mTargetList.Find(ite => UIMissionMgr.MissionView.MatchID(ite, 4116));
                        if (tarshow != null)
                            tarshow.mComplete = true;
                    }
                    if (player.motionMgr.IsActionRunning(PEActionType.Jump))
                    {
                        tarshow = missview.mTargetList.Find(ite => UIMissionMgr.MissionView.MatchID(ite, 4117));
                        if (tarshow != null)
                            tarshow.mComplete = true;
                    }
                    if (player.motionMgr.IsActionRunning(PEActionType.Sprint))
                    {
                        tarshow = missview.mTargetList.Find(ite => UIMissionMgr.MissionView.MatchID(ite, 4118));
                        if (tarshow != null)
                            tarshow.mComplete = true;
                    }
                    if (playerCmpt.AutoRun)
                    {
                        tarshow = missview.mTargetList.Find(ite => UIMissionMgr.MissionView.MatchID(ite, 4119));
                        if (tarshow != null)
                            tarshow.mComplete = true;
                    }
                    bool complete = true;
                    foreach (var item in missview.mTargetList)
                    {
                        if (item.mComplete == false)
                        {
                            complete = false;
                            break;
                        }
                    }
                    if (complete)
                        TrainingTaskManager.Instance.CompleteMission();
                }
            }
        }

        void OnBoxOpClose(bool OpOrClose)
        {
            if (null != TrainingTaskManager.Instance.mWareHouse)
                TrainingTaskManager.Instance.mWareHouse.SetActive(OpOrClose);
        }

        PeEntity player;
        MainPlayerCmpt playerCmpt;
        public void InitBaseControl()
        {
            player = PeCreature.Instance.mainPlayer;
            playerCmpt = player.GetComponent<MainPlayerCmpt>();
        }

        public void InitFirstMission()
        {
            PeCamera.onControlModeChange += OnControlModeChange;
            completeCount = 0;
        }

        int completeCount;
        void OnControlModeChange(PeCamera.ControlMode mode)
        {
            UIMissionMgr.MissionView missview = UIMissionMgr.Instance.GetMissionView(TrainingRoomConfig.ChangeControlModeID);
            if (missview == null)
            {
                MissionManager.Instance.CompleteMission(TrainingRoomConfig.ChangeControlModeID);
                PeCamera.onControlModeChange -= OnControlModeChange;
            }
            UIMissionMgr.TargetShow tarshow;
            switch (mode)
            {
                case PeCamera.ControlMode.MMOControl:
                    tarshow = missview.mTargetList.Find(ite => UIMissionMgr.MissionView.MatchID(ite, 4114));
                    if (tarshow != null)
                        tarshow.mComplete = true;
                    completeCount++;

                    if (MissionRepository.HaveTalkOP(722))
                    {
                        GameUI.Instance.mNPCTalk.NormalOrSP(0);
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(722, 1);
                        GameUI.Instance.mNPCTalk.PreShow();
                    }
                    else if (MissionManager.Instance.IsGetTakeMission(722))
                        MissionManager.Instance.SetGetTakeMission(722, null, MissionManager.TakeMissionType.TakeMissionType_Get);

                    break;
                case PeCamera.ControlMode.FirstPerson:
                    tarshow = missview.mTargetList.Find(ite => UIMissionMgr.MissionView.MatchID(ite, 4115));
                    if (tarshow != null)
                        tarshow.mComplete = true;
                    completeCount++;

                    if (MissionRepository.HaveTalkOP(723))
                    {
                        GameUI.Instance.mNPCTalk.NormalOrSP(0);
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(723, 1);
                        GameUI.Instance.mNPCTalk.PreShow();
                    }
                    else if (MissionManager.Instance.IsGetTakeMission(723))
                        MissionManager.Instance.SetGetTakeMission(723, null, MissionManager.TakeMissionType.TakeMissionType_Get);
                    break;
                default:
                    break;
            }
            if (completeCount > 1)
            {
                TrainingTaskManager.Instance.CompleteMission();
                PeCamera.onControlModeChange -= OnControlModeChange;
            }
        }

        public void InitMoveScene()
        {
            //CreatTerrian();
        }

        public void DestroyMoveScene()
        {
            Debug.Log("关闭NewMoveMove场景");
            DestroyTerrian();
        }

        public void InitGetMedicineScene()
        {
            if (TrainingTaskManager.Instance.mWareHouse != null)
                TrainingTaskManager.Instance.mWareHouse.SetActive(true);
            UIWarehouse.OnShow += OpenWareHouseTalk;
        }

        void OpenWareHouseTalk() 
        {
            GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(new List<int> { 300219 });
            GameUI.Instance.mNPCTalk.PreShow();

            UIWarehouse.OnShow -= OpenWareHouseTalk;
        }

        public void DestroyGetMedicineScene()
        {
            if (TrainingTaskManager.Instance.mWareHouse != null)
                TrainingTaskManager.Instance.mWareHouse.SetActive(false);
        }

        void CloseMission()
        {
            Debug.Log("开始NewMove任务");
        }


        void CreatTerrian()
        {
            BSVoxel voxel = new BSVoxel();
            voxel.blockType = BSVoxel.MakeBlockType(1, 0);
            if (BSBlockMatMap.s_ItemToMat.ContainsKey(281))
                voxel.materialType = (byte)BSBlockMatMap.s_ItemToMat[281];
            else
                voxel.materialType = 0;

            for (int i = 9; i <= 14; i++)
            {
                Vector3 pos1 = new Vector3(10f, 2f, i);
                pos1 = new Vector3(pos1.x * BuildingMan.Blocks.ScaleInverted,
                    pos1.y * BuildingMan.Blocks.ScaleInverted,
                    pos1.z * BuildingMan.Blocks.ScaleInverted);
                BuildingMan.Blocks.SafeWrite(voxel, (int)pos1.x, (int)pos1.y, (int)pos1.z);

                Vector3 pos2 = new Vector3(10f, 2f, i + 0.5f);
                pos2 = new Vector3(pos2.x * BuildingMan.Blocks.ScaleInverted,
                    pos2.y * BuildingMan.Blocks.ScaleInverted,
                    pos2.z * BuildingMan.Blocks.ScaleInverted);
                BuildingMan.Blocks.SafeWrite(voxel, (int)pos2.x, (int)pos2.y, (int)pos2.z);

                Vector3 pos3 = new Vector3(10f, 1.5f, i);
                pos3 = new Vector3(pos3.x * BuildingMan.Blocks.ScaleInverted,
                    pos3.y * BuildingMan.Blocks.ScaleInverted,
                    pos3.z * BuildingMan.Blocks.ScaleInverted);
                BuildingMan.Blocks.SafeWrite(voxel, (int)pos3.x, (int)pos3.y, (int)pos3.z);

                Vector3 pos4 = new Vector3(10f, 1.5f, i + 0.5f);
                pos4 = new Vector3(pos4.x * BuildingMan.Blocks.ScaleInverted,
                    pos4.y * BuildingMan.Blocks.ScaleInverted,
                    pos4.z * BuildingMan.Blocks.ScaleInverted);
                BuildingMan.Blocks.SafeWrite(voxel, (int)pos4.x, (int)pos4.y, (int)pos4.z);
            }
        }

        void DestroyTerrian()
        {
            BSVoxel voxel = new BSVoxel();
            voxel.blockType = (byte)0;
            voxel.materialType = (byte)0;

            for (int i = 9; i <= 14; i++)
            {
                Vector3 pos1 = new Vector3(10f, 2f, i);
                pos1 = new Vector3(pos1.x * BuildingMan.Blocks.ScaleInverted,
                    pos1.y * BuildingMan.Blocks.ScaleInverted,
                    pos1.z * BuildingMan.Blocks.ScaleInverted);
                BuildingMan.Blocks.SafeWrite(voxel, (int)pos1.x, (int)pos1.y, (int)pos1.z);

                Vector3 pos2 = new Vector3(10f, 2f, i + 0.5f);
                pos2 = new Vector3(pos2.x * BuildingMan.Blocks.ScaleInverted,
                    pos2.y * BuildingMan.Blocks.ScaleInverted,
                    pos2.z * BuildingMan.Blocks.ScaleInverted);
                BuildingMan.Blocks.SafeWrite(voxel, (int)pos2.x, (int)pos2.y, (int)pos2.z);

                Vector3 pos3 = new Vector3(10f, 1.5f, i);
                pos3 = new Vector3(pos3.x * BuildingMan.Blocks.ScaleInverted,
                    pos3.y * BuildingMan.Blocks.ScaleInverted,
                    pos3.z * BuildingMan.Blocks.ScaleInverted);
                BuildingMan.Blocks.SafeWrite(voxel, (int)pos3.x, (int)pos3.y, (int)pos3.z);

                Vector3 pos4 = new Vector3(10f, 1.5f, i + 0.5f);
                pos4 = new Vector3(pos4.x * BuildingMan.Blocks.ScaleInverted,
                    pos4.y * BuildingMan.Blocks.ScaleInverted,
                    pos4.z * BuildingMan.Blocks.ScaleInverted);
                BuildingMan.Blocks.SafeWrite(voxel, (int)pos4.x, (int)pos4.y, (int)pos4.z);
            }
        }


        IEnumerator FindTerrian()
        {
            yield return new WaitForSeconds(1f);
            Transform terrain1 = GameObject.Find("b45Chnk_8_0_12_0").transform;
            Transform terrain2 = GameObject.Find("b45Chnk_8_0_8_0").transform;
            if (terrain1 != null)
            {
                appearance1.orgterrain = terrain1;
                //terrain1.localScale = Vector3.zero;
                terrain1.gameObject.SetActive(false);
            }
            appearance1.gameObject.SetActive(true);
            appearance1.orgterrain.gameObject.SetActive(true);
            appearance1.orgterrain.GetComponent<MeshCollider>().GetComponent<Collider>().enabled = true;
            hcc.renderObjs2.Add(appearance1.orgterrain.GetComponent<MeshRenderer>());
            appearance1.produce = true;


            if (terrain2 != null)
            {
                appearance2.orgterrain = terrain2;
                //terrain2.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                terrain2.gameObject.SetActive(false);
            }
            appearance2.gameObject.SetActive(true);
            appearance2.orgterrain.gameObject.SetActive(true);
            appearance2.orgterrain.GetComponent<MeshCollider>().GetComponent<Collider>().enabled = true;
            hcc.renderObjs2.Add(appearance2.orgterrain.GetComponent<MeshRenderer>());
            appearance2.produce = true;
        }


        public void ChangeRenderTarget(MeshRenderer newTarget, bool org = true)
        {
            if (org)
            {
                hcc.renderObjs2.Clear();
                hcc.renderObjs2.Add(newTarget);
            }
            else
            {
                hcc.renderObjs3.Clear();
                hcc.renderObjs3.Add(newTarget);
            }
        }
    }
}
