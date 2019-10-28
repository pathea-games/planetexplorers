using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

namespace TrainingScene
{
    public class EmitlineTask : MonoBehaviour
    {
        private static EmitlineTask s_instance;
        public static EmitlineTask Instance { get { return s_instance; } }
        public EmitreceiverAppear[] receivers;
        [SerializeField]
        GameObject[] recieverBase;
        [SerializeField]
        Transform lineGroup;
        [SerializeField]
        GameObject receiverGroup;
        [HideInInspector]
        public int testScore;
        [HideInInspector]
        public bool missionComplete;
        [HideInInspector]
        public bool isBuildLocked = true;
        void Awake()
        {
            s_instance = this;
        }
        void Update()
        {
            testScore = 0;
            //			if(isBuildLocked && BuildBlockManager.self.IsActive())
            //				BuildBlockManager.self.QuitBuildMode();
        }
        void LateUpdate()
        {
            if (testScore > 0 && !missionComplete)
            {
                TrainingTaskManager.Instance.CompleteMission();
                missionComplete = true;
            }
        }

        void OnDestroy()
        {
            if (MainPlayerCmpt.gMainPlayer != null)
                MainPlayerCmpt.gMainPlayer.onBuildMode -= OnInBuildMode;
        }

        public void InitInBuildScene()
        {
            MainPlayerCmpt.gMainPlayer.onBuildMode += OnInBuildMode;
        }

        void OnInBuildMode(bool inorout)
        {
            if (inorout)
            {
                MainPlayerCmpt.gMainPlayer.onBuildMode -= OnInBuildMode;
                TrainingTaskManager.Instance.CompleteMission();
            }
        }

        List<int[]> recordVoxelRemove;
        public void InitScene()
        {
            recordVoxelRemove = new List<int[]>();
            BSBlock45Data.voxelWrite += AddRecordVoxel;
            CreatTerrian();//创建地形
            lineGroup.gameObject.SetActive(true);
            receiverGroup.SetActive(true);
            foreach (GameObject i in recieverBase)
                HoloCameraControl.Instance.renderObjs1.Add(i);
            foreach (EmitreceiverAppear i in receivers)
                i.produce = true;
            EmitlineAppearance.Instance.StartFadeLine(0.3f, true);
            isBuildLocked = false;
            missionComplete = false;
        }

        void AddRecordVoxel(int[] tmp) 
        {
            recordVoxelRemove.Add(tmp);
        }

        public void DestroyScene()
        {
            foreach (EmitreceiverAppear era in receivers)
                era.destroy = true;
            EmitlineAppearance.Instance.StartFadeLine(0.3f, false);
            //BuildBlockManager.self.QuitBuildMode();
            //isBuildLocked = true;
            Invoke("CloseMission", receivers[0].fadeTime + 0.2f);
        }
        public void CloseMission_buildPoint()
        {
            HoloCameraControl.Instance.renderObjs1.Clear();
            lineGroup.gameObject.SetActive(false);
        }

        GameObject isoCube1,isoCube2;
        public void CreateExportIsoCube()
        {
            isoCube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            isoCube1.name = "IsoCube";
            isoCube1.transform.position = new Vector3(18.5f, 3, 11.5f);
            isoCube1.transform.localScale = new Vector3(4, 4, 4);
            isoCube1.GetComponent<Renderer>().material = Resources.Load<Material>("Material/BlueGizmoMat");
            Component.Destroy(isoCube1.GetComponent<Collider>());

            isoCube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            isoCube2.name = "IsoCube";
            isoCube2.transform.position = new Vector3(11.5f, 3.5f, 10.5f);
            isoCube2.transform.localScale = new Vector3(4, 4, 4);
            isoCube2.GetComponent<Renderer>().material = Resources.Load<Material>("Material/BlueGizmoMat");
            Component.Destroy(isoCube2.GetComponent<Collider>());
        }

        public void CloseMission_buildCreateISO() 
        {
            GameObject.Destroy(isoCube1);
            GameObject.Destroy(isoCube2);

            BSBlock45Data.voxelWrite -= AddRecordVoxel;
            BSVoxel vol = new BSVoxel(0, 0);
            for (int i = 0; i < recordVoxelRemove.Count; i++)
            {
                int[] tmp = recordVoxelRemove[i];
                BuildingMan.Blocks.SafeWrite(vol, tmp[0], tmp[1], tmp[2]);
            }
            Pathea.MotionMgrCmpt mmc = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.MotionMgrCmpt>();
            if (mmc != null)
                mmc.EndAction(Pathea.PEActionType.Build);

            receiverGroup.SetActive(false);
            DestroyTerrian();
        }

        void CreatTerrian()
        {
            VFVoxel vol = new VFVoxel((byte)255, 1);

            for (int i = 17; i <= 20; i++)
            {
                for (int j = 10; j <= 13; j++)
                {
                    VFVoxelTerrain.self.Voxels.SafeWrite(i, 1, j, vol);
                }
            }
            for (int i = 10; i <= 13; i++)
            {
                for (int j = 2; j <= 5; j++)
                {
                    VFVoxelTerrain.self.Voxels.SafeWrite(i, j, 9, vol);
                }
            }
        }


        void DestroyTerrian()
        {
            VFVoxel vol = new VFVoxel((byte)0, 0);

            for (int i = 17; i <= 20; i++)
            {
                for (int j = 10; j <= 13; j++)
                {
                    VFVoxelTerrain.self.Voxels.SafeWrite(i, 1, j, vol);
                }
            }
            for (int i = 10; i <= 13; i++)
            {
                for (int j = 2; j <= 5; j++)
                {
                    VFVoxelTerrain.self.Voxels.SafeWrite(i, j, 9, vol);
                }
            }
        }
    }
}
