using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

namespace TrainingScene
{
    public class TerrainDigTask : MonoBehaviour
    {
        private static TerrainDigTask s_instance;
        public static TerrainDigTask Instance { get { return s_instance; } }
        public TerrainDigAppearance appearance;
        HoloCameraControl hcc;
        //PeEntity mplayer;
        //int num = 0;

        Transform terrain;

        void Awake()
        {
            s_instance = this;
            //CreatTerrian();
        }
        void Start()
        {
            hcc = HoloCameraControl.Instance;
            //mplayer = PeCreature.Instance.mainPlayer;
        }

        void OnDestroy()
        {
            if (PeCreature.Instance != null && PeCreature.Instance.mainPlayer != null)
                PeCreature.Instance.mainPlayer.equipmentCmpt.changeEventor.Unsubscribe(OnEquip);
            if (GameUI.Instance != null && GameUI.Instance.mBuildBlock != null)
                GameUI.Instance.mBuildBlock.mMenuCtrl.onMenuBtn -= OnBuildMenuBtn;
        }

        IEnumerator FindDigTerrain()
        {
            yield return new WaitForSeconds(2f);
            terrain = GameObject.Find("Chnk_0_0_0_0").transform;
            if (terrain != null)
                Debug.Log("找到了");
        }

        public void InitScene()
        {
            CreatTerrian();
            PeCreature.Instance.mainPlayer.equipmentCmpt.changeEventor.Subscribe(OnEquip);
        }

        public void InitMenuBtn()
        {
            GameUI.Instance.mBuildBlock.mMenuCtrl.onMenuBtn += OnBuildMenuBtn;
        }

        void OnBuildMenuBtn()
        {
            TrainingTaskManager.Instance.CompleteMission();
            GameUI.Instance.mBuildBlock.mMenuCtrl.onMenuBtn -= OnBuildMenuBtn;
        }

        void OnEquip(object sender, EquipmentCmpt.EventArg arg) 
        {
            if (!arg.isAdd)
                return;
            if (!PETools.PEUtil.IsChildItemType(arg.itemObj.protoData.editorTypeId, 5))
                return;

            if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
            {
                GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(new List<int> { 4078 });
                GameUI.Instance.mNPCTalk.PreShow();
            }
            else
                GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(new List<int> { 4078 }, null, false);

            PeCreature.Instance.mainPlayer.equipmentCmpt.changeEventor.Unsubscribe(OnEquip);
        }

        public void DestroyScene()
        {
            DestroyTerrian();
        }
        void CloseMission()
        {
            DestroyTerrian();
        }
        public void ChangeRenderTarget(MeshRenderer newTarget)
        {
            hcc.renderObjs2.Clear();
            hcc.renderObjs2.Add(newTarget);
        }

        void CreatTerrian()
        {
            VFVoxel vol1 = new VFVoxel((byte)255, 1);
            VFVoxel vol2 = new VFVoxel((byte)210, 1);
            VFVoxel vol3 = new VFVoxel((byte)190, 1);
            VFVoxel vol4 = new VFVoxel((byte)170, 1);
            VFVoxel vol5 = new VFVoxel((byte)150, 1);
            VFVoxel vol6 = new VFVoxel((byte)130, 1);
            VFVoxel vol7 = new VFVoxel((byte)110, 1);

            for (int i = -2; i <= 15; i++)
            {
                for (int j = 2; j <= 21; j++)
                {
                    VFVoxelTerrain.self.Voxels.SafeWrite(i, 0, j, vol1);
                }
            }

            for (int i = -1; i <= 14; i++)
            {
                for (int j = 3; j <= 20; j++)
                {
                    VFVoxelTerrain.self.Voxels.SafeWrite(i, 1, j, vol2);
                }
            }

            for (int i = 4; i <= 8; i++)
            {
                for (int j = 8; j <= 15; j++)
                {
                    VFVoxelTerrain.self.Voxels.SafeWrite(i, 2, j, vol3);
                }
            }

            for (int i = 10; i <= 12; i++)
            {
                VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, i, vol7);
            }

            VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, 13, vol4);
            VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, 14, vol4);
            VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, 9, vol5);

            for (int i = 5; i <= 6; i++)
            {
                for (int j = 10; j <= 12; j++)
                {
                    VFVoxelTerrain.self.Voxels.SafeWrite(i, 3, j, vol3);
                }
            }

            VFVoxelTerrain.self.Voxels.SafeWrite(5, 3, 13, vol5);
            VFVoxelTerrain.self.Voxels.SafeWrite(6, 3, 13, vol5);

            VFVoxelTerrain.self.Voxels.SafeWrite(6, 3, 9, vol4);

            VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 11, vol4);
            VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 10, vol5);
            VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 9, vol6);
            VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 12, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 13, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 14, vol7);

            VFVoxelTerrain.self.Voxels.SafeWrite(2, 2, 11, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(2, 2, 10, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(2, 2, 9, vol7);

            VFVoxelTerrain.self.Voxels.SafeWrite(5, 2, 7, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(6, 2, 7, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(7, 2, 7, vol7);

            VFVoxelTerrain.self.Voxels.SafeWrite(6, 2, 6, vol7);

            VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 11, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 12, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 14, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 13, vol7);

            VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 11, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 12, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 10, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 13, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 9, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 14, vol7);
        }

        IEnumerator FindTerrian()
        {
            yield return new WaitForSeconds(1f);
            Transform terrain = GameObject.Find("Chnk_0_0_0_0").transform;
            if (terrain != null)
            {
                appearance.orgterrain = terrain;
                terrain.localScale = Vector3.zero;
                terrain.gameObject.SetActive(false);
            }
            appearance.gameObject.SetActive(true);
            appearance.orgterrain.gameObject.SetActive(true);
            appearance.orgterrain.GetComponent<MeshCollider>().GetComponent<Collider>().enabled = true;
            hcc.renderObjs2.Add(appearance.orgterrain.GetComponent<MeshRenderer>());
            appearance.produce = true;
        }

        void DestroyTerrian()
        {
            VFVoxel vol1 = new VFVoxel((byte)0, 0);
            VFVoxel vol2 = new VFVoxel((byte)0, 0);
            VFVoxel vol3 = new VFVoxel((byte)0, 0);
            VFVoxel vol4 = new VFVoxel((byte)0, 0);
            VFVoxel vol5 = new VFVoxel((byte)0, 0);
            VFVoxel vol6 = new VFVoxel((byte)0, 0);
            VFVoxel vol7 = new VFVoxel((byte)0, 0);

            for (int i = -2; i <= 15; i++)
            {
                for (int j = 2; j <= 21; j++)
                {
                    VFVoxelTerrain.self.Voxels.SafeWrite(i, 0, j, vol1);
                }
            }

            for (int i = -1; i <= 14; i++)
            {
                for (int j = 3; j <= 20; j++)
                {
                    VFVoxelTerrain.self.Voxels.SafeWrite(i, 1, j, vol2);
                }
            }

            for (int i = 4; i <= 8; i++)
            {
                for (int j = 8; j <= 15; j++)
                {
                    VFVoxelTerrain.self.Voxels.SafeWrite(i, 2, j, vol3);
                }
            }

            for (int i = 10; i <= 12; i++)
            {
                VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, i, vol7);
            }

            VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, 13, vol4);
            VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, 14, vol4);
            VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, 9, vol5);

            for (int i = 5; i <= 6; i++)
            {
                for (int j = 10; j <= 12; j++)
                {
                    VFVoxelTerrain.self.Voxels.SafeWrite(i, 3, j, vol3);
                }
            }

            VFVoxelTerrain.self.Voxels.SafeWrite(5, 3, 13, vol5);
            VFVoxelTerrain.self.Voxels.SafeWrite(6, 3, 13, vol5);

            VFVoxelTerrain.self.Voxels.SafeWrite(6, 3, 9, vol4);

            VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 11, vol4);
            VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 10, vol5);
            VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 9, vol6);
            VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 12, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 13, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 14, vol7);

            VFVoxelTerrain.self.Voxels.SafeWrite(2, 2, 11, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(2, 2, 10, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(2, 2, 9, vol7);

            VFVoxelTerrain.self.Voxels.SafeWrite(5, 2, 7, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(6, 2, 7, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(7, 2, 7, vol7);

            VFVoxelTerrain.self.Voxels.SafeWrite(6, 2, 6, vol7);

            VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 11, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 12, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 14, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 13, vol7);

            VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 11, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 12, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 10, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 13, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 9, vol7);
            VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 14, vol7);
        }
    }
}
