#define IncVer4Adv
using UnityEngine;
using System.Collections;
using System.IO;
using System;

namespace Pathea
{
    public class PeGameSummary
    {
        const int VERSION_0000 = 0;
		const int VERSION_0001 = VERSION_0000 + 1;
		const int VERSION_0002 = VERSION_0001 + 1;
		const int VERSION_0003 = VERSION_0002 + 1;	// for adv task integration, save data not compatiable with old data
		const int VERSION_0004 = VERSION_0003 + 1;	// for adv task integration, save data not compatiable with old data
#if !IncVer4Adv
		const int CURRENT_VERSION = VERSION_0002;
#else
		const int CURRENT_VERSION = VERSION_0004;
#endif
        PeGameSummary() { }

        public Vector3 playerPos
        {
            get;
            private set;
        }

        public Texture2D screenshot
        {
            get;
            private set;
        }

        public double gameTime
        {
            get;
            private set;
        }

        public int playTime
        {
            get;
            private set;
        }

        public string seed
        {
            get;
            private set;
        }

        public DateTime saveTime
        {
            get;
            private set;
        }

        public Pathea.PeGameMgr.ESceneMode sceneMode
        {
            get;
            private set;
        }

        public string playerName
        {
            get;
            private set;
        }

		public Pathea.PeGameMgr.EGameLevel gameLevel
		{
			get;
			private set;
		}

        #region import export
		void Export(BinaryWriter w)
        {
		    w.Write((int)CURRENT_VERSION);

            byte[] screenBuff = null;
            if (screenshot != null)
            {
                screenBuff = screenshot.EncodeToPNG();
            }
            PETools.Serialize.WriteBytes(screenBuff, w);

            w.Write((double)gameTime);
            w.Write((int)playTime);
            w.Write(seed);
            w.Write((long)saveTime.Ticks);
            w.Write((int)sceneMode);
            w.Write(playerName);
			PETools.Serialize.WriteVector3(w, playerPos);
			w.Write((int)gameLevel);
        }

        bool Import(byte[] buffer)
        {
			try{
	            using (MemoryStream ms = new MemoryStream(buffer, false))
	            {
	                using (BinaryReader r = new BinaryReader(ms))
	                {
	                    int version = r.ReadInt32();
	                    if (version > CURRENT_VERSION)
	                    {
							Debug.LogError("[GameSummary]error version:" + version);
							return false;
	                    }

	                    byte[] buff = PETools.Serialize.ReadBytes(r);
	                    if (null != buff && buff.Length > 0)
	                    {
	                        screenshot = new Texture2D(2, 2, TextureFormat.RGB24, false);
	                        screenshot.LoadImage(buff);
	                        screenshot.Apply();
	                    }

	                    gameTime = r.ReadDouble();
	                    playTime = r.ReadInt32();
	                    seed = r.ReadString();
	                    saveTime = new DateTime(r.ReadInt64());
	                    sceneMode = (Pathea.PeGameMgr.ESceneMode)r.ReadInt32();						
#if DemoVersion
						if(sceneMode == PeGameMgr.ESceneMode.Adventure || sceneMode == PeGameMgr.ESceneMode.Custom)
						{
							Debug.LogError("[GameSummary]Demo can't load adv and custom save");
							return false;
						}
#endif
#if IncVer4Adv
						if(sceneMode == PeGameMgr.ESceneMode.Adventure)
						{
							if(version < CURRENT_VERSION){
								Debug.LogError("[GameSummary]Deprecated version of adv save");
								return false;
							}
						}
#endif
	                    playerName = r.ReadString();

	                    playerPos = (version >= VERSION_0001) ? PETools.Serialize.ReadVector3(r) : Vector3.zero;
						gameLevel = (version >= VERSION_0002) ? (Pathea.PeGameMgr.EGameLevel)r.ReadInt32() : PeGameMgr.EGameLevel.Normal;
						return true;
	                }
	            }
			} catch {
				Debug.LogError("[GameSummary]Corrupt save data");
			}
			return false;
        }
        #endregion

        public class Mgr : ArchivableSingleton<Mgr>
        {
            const string ArchiveKey = "ArchiveKeyGameSummary";

            protected override string GetArchiveKey()
            {
                return ArchiveKey;
            }

            protected override bool GetYird()
            {
                return false;
            }
            protected override void WriteData(BinaryWriter bw)
            {
                Gather().Export(bw);
            }

            protected override void SetData(byte[] data)
            {
                //throw new NotImplementedException();
            }

            PeGameSummary Gather()
            {
                PeGameSummary summary = new PeGameSummary();

               	summary.screenshot = ArchiveMgr.Instance.autoSave ? null : PeScreenshot.GetTex();

                PeTrans tr = MainPlayer.Instance.entity.peTrans;
                if(tr != null){
                    summary.playerPos = tr.fastTravel ? tr.fastTravelPos : tr.position;
                }

                summary.gameTime = GameTime.Timer.Second;
                summary.playTime = (int)GameTime.PlayTime.Second;
                summary.saveTime = DateTime.Now;
                summary.sceneMode = PeGameMgr.sceneMode;
                summary.seed = summary.sceneMode == Pathea.PeGameMgr.ESceneMode.Story ? "NA" : RandomMapConfig.SeedString;

                PeEntity mainPlayer = PeCreature.Instance.mainPlayer;
                summary.playerName = null == mainPlayer ? "NA" : mainPlayer.ToString();

				summary.gameLevel = PeGameMgr.gameLevel;                
                return summary;
            }

            public PeGameSummary Get()
            {
                byte[] buff = ArchiveMgr.Instance.GetData(ArchiveKey);
                if (null == buff || buff.Length <= 0){
                    return null;
                }

                PeGameSummary s = new PeGameSummary();
				return s.Import(buff) ? s : null;
            }

            //use this to register archive.
            public void Init()
            {
                Debug.Log("game summary");
            }
        }
    }

    class SinglePlayerTypeArchiveMgr : ArchivableSingleton<SinglePlayerTypeArchiveMgr>
    {
        const int VERSION_0000 = 0;
        const int CURRENT_VERSION = VERSION_0000;

        SinglePlayerTypeLoader mSingleScenario;

        public SinglePlayerTypeLoader singleScenario
        {
            get { return mSingleScenario; }
            private set { mSingleScenario = value; }
        }

        protected override bool GetYird()
        {
            return false;
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                mSingleScenario = new SinglePlayerTypeLoader();

                mSingleScenario.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            mSingleScenario.Export(bw);
        }

        public override void New()
        {
            base.New();

            mSingleScenario = new SinglePlayerTypeLoader();
        }
    }

	class MultiPlayerTypeArchiveMgr : ArchivableSingleton<MultiPlayerTypeArchiveMgr>
	{
		const int VERSION_0000 = 0;
		const int CURRENT_VERSION = VERSION_0000;
		
		MultiPlayerTypeLoader mMultiScenario;
		
		public MultiPlayerTypeLoader multiScenario
		{
			get { return mMultiScenario; }
			private set { 
				mMultiScenario = value; 
			}
		}
		
		protected override bool GetYird()
		{
			return false;
		}
		
		protected override void SetData(byte[] data)
		{
// 			if (null != data)
// 			{
// 				mMultiScenario = new MultiPlayerTypeLoader();
// 				
// 				mMultiScenario.Import(data);
// 			}
		}
		
		protected override void WriteData(BinaryWriter bw)
		{
			mMultiScenario.Export(bw);
		}
		
		public override void New()
		{
			base.New();
			
			mMultiScenario = new MultiPlayerTypeLoader();
		}
	}

    public class RandomMapConfigArchiveMgr : ArchivableSingleton<RandomMapConfigArchiveMgr>
    {
        const int VERSION_0000 = 0;
        const int VERSION_0001 = 1;
        const int VERSION_0002 = 2;
		const int VERSION_0003 = 3;
		const int VERSION_0004 = 4;//2016-7-29 11:59:19
		const int VERSION_0005 = 5;//2016-10-14 20:44:22
		const int CURRENT_VERSION = VERSION_0005;

        protected override void WriteData(BinaryWriter bw)
        {
            Export(bw);
        }

        protected override void SetData(byte[] data)
        {
            Import(data);
            RandomMapConfig.Instance.SetMapParam();
        }

        public override void New()
        {
            base.New();

            RandomMapConfig.Instance.SetMapParam();
        }

        void Export(BinaryWriter w)
        {
            w.Write((int)CURRENT_VERSION);

            w.Write((int)RandomMapConfig.RandSeed);
            w.Write(RandomMapConfig.SeedString);
            w.Write((int)RandomMapConfig.vegetationId);
            w.Write((int)RandomMapConfig.RandomMapID);
            w.Write((int)RandomMapConfig.ScenceClimate);
            w.Write((int)RandomMapConfig.mapSize);
            w.Write((int)RandomMapConfig.riverDensity);
            w.Write((int)RandomMapConfig.riverWidth);
			w.Write((bool)RandomMapConfig.useSkillTree);
			w.Write((int)RandomMapConfig.TerrainHeight);
			w.Write((int)RandomMapConfig.plainHeight);
	        w.Write((int)RandomMapConfig.flatness);
			w.Write((int)RandomMapConfig.bridgeMaxHeight);
			w.Write((bool)RandomMapConfig.mirror);
			w.Write((int)RandomMapConfig.rotation);
			w.Write((int)RandomMapConfig.pickedLineIndex);
			w.Write((int)RandomMapConfig.pickedLevelIndex);
			w.Write((int)RandomMapConfig.allyCount);
        }

        void Import(byte[] buffer)
        {
            PETools.Serialize.Import(buffer, (r) =>
            {
                int version = r.ReadInt32();
                if (version > CURRENT_VERSION)
                {
                    Debug.LogError("error version:" + version);
                }

                if (version >= VERSION_0000)
                {
                    RandomMapConfig.RandSeed = r.ReadInt32();
                    RandomMapConfig.SeedString = r.ReadString();
                    RandomMapConfig.vegetationId = (RandomMapType)r.ReadInt32();
                    RandomMapConfig.RandomMapID = (RandomMapType)r.ReadInt32();
                    RandomMapConfig.ScenceClimate = (ClimateType)r.ReadInt32();
                    RandomMapConfig.mapSize = r.ReadInt32();
                    RandomMapConfig.riverDensity = r.ReadInt32();
                    RandomMapConfig.riverWidth = r.ReadInt32();
                }

                if (version >= VERSION_0001)
                {
                    RandomMapConfig.useSkillTree = r.ReadBoolean();
                }

                if (version >= VERSION_0002)
                {
                    RandomMapConfig.TerrainHeight = r.ReadInt32();
                } 
				if(version>= VERSION_0003)
				{
					RandomMapConfig.plainHeight = r.ReadInt32();
					RandomMapConfig.flatness = r.ReadInt32();
					RandomMapConfig.bridgeMaxHeight = r.ReadInt32();
				}
				if(version>=VERSION_0004){
					RandomMapConfig.mirror = r.ReadBoolean();
					RandomMapConfig.rotation = r.ReadInt32();
					RandomMapConfig.pickedLineIndex = r.ReadInt32();
					RandomMapConfig.pickedLevelIndex = r.ReadInt32();
				}
				if(version>=VERSION_0005){
					RandomMapConfig.allyCount = r.ReadInt32();
				}
            });
        }
    }

    public class CreationDataArchiveMgr : ArchivableSingleton<CreationDataArchiveMgr>
    {
        const string ArchiveKey = "ArchiveKeyCreation";

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        void Init()
        {
            CreationMgr.Init();
        }

        protected override bool GetYird()
        {
            return false;
        }

        public override void New()
        {
            Init();

            base.New();
        }

        public override void Restore()
        {
            Init();
            base.Restore();
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                CreationMgr.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            CreationMgr.Export(bw);
        }
    }

    public class ItemAssetArchiveMgr : ArchivableSingleton<ItemAssetArchiveMgr>
    {
        const string ArchiveKey = "ArchiveKeyItemAsset";

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        protected override bool GetYird()
        {
            return false;
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
				try{
                	ItemAsset.ItemMgr.Instance.Import(data);
				} catch(Exception e){
					Debug.LogWarning(e);
				}
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            ItemAsset.ItemMgr.Instance.Export(bw);
        }
    }

    public class RMRepositoryArchiveMgr : ArchivableSingleton<RMRepositoryArchiveMgr>
    {
        const string ArchiveKey = "ArchiveKeyRMRepository";

        protected override bool GetYird()
        {
            return false;
        }

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                RMRepository.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            RMRepository.Export(bw);
        }
    }

    public class AdRMRepositoryArchiveMgr : ArchivableSingleton<AdRMRepositoryArchiveMgr>
    {
        const string ArchiveKey = "ArchiveKeyAdRMRepository";

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                AdRMRepository.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            AdRMRepository.Export(bw);
        }
    }

    public class FarmArchiveMgr : ArchivableSingleton<FarmArchiveMgr>
    {
        const string ArchiveKey = "ArchiveKeyFarm";

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                FarmManager.Instance.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            FarmManager.Instance.Export(bw);
        }
    }

    public class TownNpcArchiveMgr : ArchivableSingleton<TownNpcArchiveMgr>
    {
        const string ArchiveKey = "TownNpcArchiveMgr";

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                VATownNpcManager.Instance.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            VATownNpcManager.Instance.Export(bw);
        }
    }

    public class VABuildingArchiveMgr : ArchivableSingleton<VABuildingArchiveMgr>
    {
        const string ArchiveKey = "VABuildingArchiveMgr";

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                VABuildingManager.Instance.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            VABuildingManager.Instance.Export(bw);
        }
    }

    public class VArtifactTownArchiveMgr : ArchivableSingleton<VArtifactTownArchiveMgr>
    {
        const string ArchiveKey = "VArtifactTownArchiveMgr";

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                VArtifactTownManager.Instance.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            VArtifactTownManager.Instance.Export(bw);
        }

		protected override bool GetYird(){
			return false;
		}
    }

    public class GrassDataSLArchiveMgr : ArchivableSingleton<GrassDataSLArchiveMgr>
    {
        const string ArchiveKey = "ArchiveKeyGrassDataSL";

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        protected override void SetData(byte[] data)
        {
            GrassDataSL.Init();
            if (null != data)
            {
                GrassDataSL.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            GrassDataSL.Export(bw);
        }

        public override void New()
        {
            base.New();
            GrassDataSL.Init();
        }
    }

    public class CSDataMgrArchiveMgr : ArchivableSingleton<CSDataMgrArchiveMgr>
    {
        const string ArchiveKey = "ArchiveKeyCSDataMgr";

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                CSDataMgr.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            CSDataMgr.Export(bw);
        }
    }

    public class TownEditorArchiveMgr : ArchivableSingleton<TownEditorArchiveMgr>
    {
        const string ArchiveKey = "ArchiveKeyTownEditor";

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }
        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                //TownEditor.Instance.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            TownEditor.Instance.Export(bw);
        }
    }

    public class SPPlayerBaseArchiveMgr : ArchivableSingleton<SPPlayerBaseArchiveMgr>
    {
        const string ArchiveKey = "ArchiveKeySPPlayerBase";

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                SPPlayerBase.Single.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            SPPlayerBase.Single.Export(bw);
        }
    }

    //two key,cant use ArchivableSingleton
    public class VoxelTerrainArchiveMgr : Pathea.MonoLikeSingleton<VoxelTerrainArchiveMgr>, Pathea.ISerializable
    {
        const string VoxelArchiveFileName = "voxel";
        const string WaterArchiveFileName = "water";
        const string BlockArchiveFileName = "block45";

        public const string Bloc45kArchiveKey = "ArchiveKeyBlock45";

        protected override void OnInit()
        {
            base.OnInit();

            ArchiveMgr.Instance.Register(VFVoxelTerrain.ArchiveKey, this, true, VoxelArchiveFileName, false);
            ArchiveMgr.Instance.Register(VFVoxelWater.ArchiveKey, this, true, WaterArchiveFileName, false);
            ArchiveMgr.Instance.Register(Bloc45kArchiveKey, this, true, BlockArchiveFileName);
        }

        public void Restore()
        {
			PeRecordReader r = ArchiveMgr.Instance.GetReader(VFVoxelTerrain.ArchiveKey);
            VFVoxelTerrain.self.Import(r);

			r = ArchiveMgr.Instance.GetReader(VFVoxelWater.ArchiveKey);
            VFVoxelWater.self.Import(r);

            r = ArchiveMgr.Instance.GetReader(Bloc45kArchiveKey);
            Block45Man.self.Import(r);
        }

        public void New()
        {
            VFVoxelTerrain.self.Import(null);
            VFVoxelWater.self.Import(null);
            Block45Man.self.Import(null);
        }

        void ISerializable.Serialize(PeRecordWriter w)
        {
			if (w.key == VFVoxelTerrain.ArchiveKey)
            {
                //terrain
                VFVoxelTerrain.self.SaveLoad.Export(w);
            }
			else if (w.key == VFVoxelWater.ArchiveKey)
            {
                //water
                VFVoxelWater.self.SaveLoad.Export(w);
            }
            else if (w.key == Bloc45kArchiveKey)
            {
                Block45Man.self.Export(w);
            }
        }
    }

    public class LSubTerrSLArchiveMgr : ArchivableSingleton<LSubTerrSLArchiveMgr>
    {
        const string ArchiveKey = "LSubTerrSLArchiveMgr";

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                LSubTerrSL.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            LSubTerrSL.Export(bw);
        }
    }

    public class RSubTerrSLArchiveMgr : Pathea.ArchivableSingleton<RSubTerrSLArchiveMgr>
    {
        const string ArchiveKey = "RSubTerrSLArchiveMgr";

        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                RSubTerrSL.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            RSubTerrSL.Export(bw);
        }
    }

    public class UiHelpArchiveMgr : Pathea.MonoLikeSingleton<UiHelpArchiveMgr>, Pathea.ISerializable
    {
        const string TutorialDataArchiveKey = "TutorialData";
        const string MetalScanDataArchiveKey = "MetalScanData";
        //lz-2016.07.13 剧情邮件消息Key
        const string MessageDataArchveKey = "MessageData";
        //lz-2016.07.20 怪物图鉴存储Key
        const string MonsterHandbookDataArchveKey = "MonsterHandbookData";

        protected override void OnInit()
        {
            base.OnInit();
            ArchiveMgr.Instance.Register(TutorialDataArchiveKey, this);
            ArchiveMgr.Instance.Register(MetalScanDataArchiveKey, this);
            ArchiveMgr.Instance.Register(MessageDataArchveKey, this);
            ArchiveMgr.Instance.Register(MonsterHandbookDataArchveKey, this);
        }

        public void Restore()
        {
            byte[] data = ArchiveMgr.Instance.GetData(TutorialDataArchiveKey);
            
            if (null != data)
            {
                TutorialData.Deserialize(data);
            }

            data = ArchiveMgr.Instance.GetData(MetalScanDataArchiveKey);

            if (null != data)
            {
                MetalScanData.Deserialize(data);
            }

            data = ArchiveMgr.Instance.GetData(MessageDataArchveKey);

            if (null != data)
            {
                MessageData.Deserialize(data);
            }

            data = ArchiveMgr.Instance.GetData(MonsterHandbookDataArchveKey);

            if (null != data)
            {
                MonsterHandbookData.Deserialize(data);
            }
        }

        public void New()
        {
            MetalScanData.Clear();
            //lz-2016.07.25 添加清空方法，在新游戏的时候清空保存的用户数据
            MessageData.Clear();
            TutorialData.Clear();
            MonsterHandbookData.Clear();
        }

        void ISerializable.Serialize(PeRecordWriter w)
        {
            if (w.key == TutorialDataArchiveKey)
            {
                w.binaryWriter.Write(TutorialData.Serialize());
            }
            else if (w.key == MetalScanDataArchiveKey)
            {
                w.binaryWriter.Write(MetalScanData.Serialize());
            }
            else if (w.key == MessageDataArchveKey)
            {
                w.binaryWriter.Write(MessageData.Serialize());
            }
            else if (w.key == MonsterHandbookDataArchveKey)
            {
                w.binaryWriter.Write(MonsterHandbookData.Serialize());
            }
        }
    }

    public class PlayerSpawnPosProvider : Pathea.PeSingleton<PlayerSpawnPosProvider>
    {
        Vector3 mPos;
        public Vector3 GetPos()
        {
            return mPos;
        }

        public void SetPos(Vector3 pos)
        {
            Debug.Log("<color=yellow>set player spawn pos:"+ pos +"</color>");
            mPos = pos;
        }
    }

    //lz-2016.08.21 游戏引导存档类
    public class InGamAidArchiveMgr : Pathea.MonoLikeSingleton<InGamAidArchiveMgr>, Pathea.ISerializable
    {
        const string InGameAidArchiveKey = "InGameAidArchiveKey";

        protected override void OnInit()
        {
            base.OnInit();
            ArchiveMgr.Instance.Register(InGameAidArchiveKey, this);
        }

        public void Restore()
        {
            byte[] data = ArchiveMgr.Instance.GetData(InGameAidArchiveKey);

            if (null != data)
            {
                InGameAidData.Deserialize(data);
            }
        }

        public void New()
        {
            InGameAidData.ShowInGameAidCtrl = true;
            InGameAidData.Clear();
        }

        void ISerializable.Serialize(PeRecordWriter w)
        {
            if (w.key == InGameAidArchiveKey)
            {
                w.binaryWriter.Write(InGameAidData.Serialize());
            }
        }
    }

    public class MountsArchiveMgr :   Pathea.ArchivableSingleton<MountsArchiveMgr>
    {
        const string ArchiveKey = "MountsArchiveMgr";

        public override void New()
        {
            base.New();
            RelationshipDataMgr.Clear();
        }
        protected override string GetArchiveKey()
        {
            return ArchiveKey;
        }

        protected override void SetData(byte[] data)
        {
            if (null != data)
            {
                RelationshipDataMgr.Import(data);
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            RelationshipDataMgr.Export(bw);
        }
    }
}