using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea
{
    public class FastTravel : MonoBehaviour
    {
        void Start()
		{
			MessageBox_N.CancelMask(MsgInfoType.DungeonGeneratingMask);
			UILoadScenceEffect.Instance.EndScence(SetToTravelPos);
	    }
		public static bool bTraveling = false;
		public Vector3 travelPos = Vector3.zero;
        public static void TravelTo(Vector3 pos)
        {
			if(pos.y>-100&&RandomDungenMgr.Instance!=null&&RandomDungenMgrData.InDungeon){
				RandomDungenMgr.Instance.TransFromDungeonMultiMode(pos);
				RandomDungenMgr.Instance.DestroyDungeon();
			}
			FastTravel ft =new GameObject("FastTravel", typeof(FastTravel)).GetComponent<FastTravel>();
			ft.travelPos = pos;
            FastTravelMgr.Instance.DispatchFastTravel(pos);

        }

		bool bFirstRun = true;
		public void StartLoadScene(){
			for (int i = 0; i < 100; i++)
			{
				PeLauncher.Instance.Add(new GameLoader.Dummy());
			}
			bFirstRun = true;
			PeLauncher.Instance.endLaunch = delegate()
			{
				if (PeGameMgr.IsMulti && !NetworkInterface.IsClient)
				return true;

				if(bFirstRun){
					bFirstRun = false;
					VFVoxelTerrain.TerrainVoxelComplete=false;
					return false;
				}
				if (!VFVoxelTerrain.TerrainVoxelComplete)
				{
					return false;
				}
				
				PeEntity mainPlayer = MainPlayer.Instance.entity;
				if (null == mainPlayer)
				{
					return false;
				}
				
				MotionMgrCmpt motion = mainPlayer.GetCmpt<MotionMgrCmpt>();
				if (motion == null)
				{
					return false;
				}
				Vector3 safePos;
				if(PeGameMgr.IsMulti)
				{
					if(PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.MainLand)
					{
						if(PETools.PE.FindHumanSafePos(mainPlayer.position, out safePos, 10))
						{
							mainPlayer.position = safePos;
							motion.FreezePhySteateForSystem(false);
						}
						else
						{
							mainPlayer.position += 10 * Vector3.up;
							motion.FreezePhySteateForSystem(true);
							return false;
						}
					}
				}
				else
				{
					if(PETools.PE.FindHumanSafePos(mainPlayer.position, out safePos, 10))
					{
						mainPlayer.position = safePos;
						motion.FreezePhySteateForSystem(false);
					}
					else
					{
						mainPlayer.position += 10 * Vector3.up;
						motion.FreezePhySteateForSystem(true);
						return false;
					}
				}
				
				Object.Destroy(gameObject);
                Resources.UnloadUnusedAssets();
				System.GC.Collect();
				return true;
			};
			
			PeLauncher.Instance.StartLoad();
		}
		public void SetToTravelPos(){
			if(travelPos==Vector3.zero)
				return;
			PlayerNetwork pn = PlayerNetwork.mainPlayer;
			pn.transform.position = travelPos;
			if(null !=pn._move)
				pn._move.NetMoveTo(travelPos, Vector3.zero, true);
			if(bTraveling)
				return;
			bTraveling = true;
			PeEntity mainPlayer = PeCreature.Instance.mainPlayer;
			if(null == mainPlayer)
			{
				Debug.LogError("no main player");
				return;
			}
			
			PeTrans tran = mainPlayer.peTrans;
			
			if (null == tran)
			{
				return;
			}
			tran.position = travelPos;
			StartLoadScene();
		}
    }

    public class FastTravelMgr : Pathea.MonoLikeSingleton<FastTravelMgr>
    {
        public interface IEnable
        {
            bool Enable();
        }

        public event System.Action<Vector3> OnFastTravel;

        public  void DispatchFastTravel(Vector3 pos)
        {
            if (OnFastTravel != null)
                OnFastTravel(pos);
        }

        List<IEnable> mList = new List<IEnable>(1);

        public void TravelTo(string yirdName, Vector3 pos)
        {
			PeGameMgr.targetYird = yirdName;

            TravelTo(pos);
        }

        public void TravelTo(int worldIndex, Vector3 pos)
        {
            FastTravel.bTraveling = true;
            YirdData yd = CustomGameData.Mgr.Instance.curGameData.GetYirdData(worldIndex);
            if (yd == null)
                return;

            CustomGameData.Mgr.Instance.curGameData.WorldIndex = worldIndex;
			PeGameMgr.targetYird = yd.name;

            TravelTo(pos);

        }

        public void TravelTo(Vector3 pos)
        {
            if (!TravelEnable())
            {
                Debug.Log("<color=aqua>fast travel enable = false</color>");
                return;
            }

            PeEntity mainPlayer = PeCreature.Instance.mainPlayer;
            if (null == mainPlayer)
            {
                Debug.LogError("no main player");
                return;
            }

            PeTrans tran = mainPlayer.peTrans;

            if (null == tran)
            {
                return;
            }

            tran.fastTravelPos = pos;
			if(PeGameMgr.yirdName==AdventureScene.Dungen.ToString()){
				RandomDungenMgr.Instance.TransFromDungeon(pos);
				PeGameMgr.targetYird = AdventureScene.MainAdventure.ToString();
			}

            DispatchFastTravel(pos);
            TravelWithLoadScene();
        }
        
        void TravelWithLoadScene()
        {
            Pathea.PeGameMgr.loadArchive = ArchiveMgr.ESave.Auto1;
            PeFlowMgr.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
        }

        public void Add(IEnable e)
        {
            mList.Add(e);
        }

        public bool Remove(IEnable e)
        {
            return mList.Remove(e);
        }

        public bool TravelEnable()
        {
            foreach (IEnable e in mList)
            {
                if (!e.Enable())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
