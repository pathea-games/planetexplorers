using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TownEditor : MonoBehaviour 
{
	static TownEditor mInstance;
	public static TownEditor Instance{ get { return mInstance; } }
	
	public Block45CurMan mPerfab;
	public float ActiveBuildingDistance = 1000f;
	
	bool mIsActive = false;
	
	class StoryTownData
	{
		public int 		mId;
		public int 		mBuildingId;
		public Vector3 	mPosition;
		public int		mRot;
	}
	List<StoryTownData>		mTownDataList = new List<StoryTownData>();
	List<StoryTownData>		mUnactivedList = new List<StoryTownData>();
	List<int>				mActivelist = new List<int>();
	
	List<int>				mNpcList = new List<int>();
	
	List<EditBuilding> mEditBuildingList = new List<EditBuilding>();
	EditBuilding mCurrentOpBuilding;
	
	bool mDragMode = false;
	
	Vector3 mMousePos = Vector3.zero;
	
	const int Version = 1;
	
	void Awake()
	{
		mInstance = this;
        //if(GameConfig.GameModeNoMask == GameConfig.EGameMode.Singleplayer_Story)
        //    LoadData();
	}
	
	public void SetOpBuildingPosition(Vector3 pos)
	{
		if(mCurrentOpBuilding)
		{
			if(mDragMode)
				mCurrentOpBuilding.transform.position = pos + 0.5f * Vector3.down;
			else
				mMousePos = Input.mousePosition;
		}
	}
	
	public void OnCreateBuilding(string fileName)
	{
		if(mPerfab == null) mPerfab = Block45CurMan.self; 
		BlockBuilding building = BlockBuilding.GetBuilding(fileName);
		
		GameObject editObj = new GameObject();
		editObj.name = "EditBuilding";
		editObj.transform.position = Vector3.zero;
		editObj.transform.rotation = Quaternion.identity;
		editObj.transform.localScale = Vector3.one;
		
		EditBuilding editBuilding = editObj.AddComponent<EditBuilding>();
		editBuilding.Init(building, mPerfab);
		
		mEditBuildingList.Add(editBuilding);
		
		OnBuildingSelected(editBuilding);
		mDragMode = true;
	}
	
	public void OnBuildingSelected(EditBuilding building)
	{
		if(mCurrentOpBuilding)
		{
			if(building == mCurrentOpBuilding)
			{
				mCurrentOpBuilding.mSelected = false;
				mCurrentOpBuilding = null;
				TownEditGui_N.Instance.SetOpBuild(null);
			}
			else
			{
				mCurrentOpBuilding.mSelected = false;
				mCurrentOpBuilding = building;
				mCurrentOpBuilding.mSelected = true;
				TownEditGui_N.Instance.SetOpBuild(mCurrentOpBuilding);
			}
		}
		else
		{
			mCurrentOpBuilding = building;
			mCurrentOpBuilding.mSelected = true;
			TownEditGui_N.Instance.SetOpBuild(mCurrentOpBuilding);
		}
		mDragMode = false;
	}
	
	public void OnBuildingDrag(EditBuilding building)
	{
		if(building == mCurrentOpBuilding)
		{
			if(!mDragMode)
			{
				if(Vector3.Distance(Input.mousePosition, mMousePos) > 0)
					mDragMode = true;
			}
			else
			{
				mMousePos = Input.mousePosition;
			}
		}
	}
	
	public void PutBuildingDown()
	{
		if(mDragMode && null != mCurrentOpBuilding)
		{
			mDragMode = false;
			mCurrentOpBuilding.mSelected = false;
			mCurrentOpBuilding = null;
			TownEditGui_N.Instance.SetOpBuild(null);
		}
	}
	
	public void TurnBuilding()
	{
		if(mCurrentOpBuilding)
		{
			mCurrentOpBuilding.transform.rotation *= Quaternion.Euler(90 * Vector3.up);
		}
	}
	
	public void CancelSelect()
	{
		if(mCurrentOpBuilding)
		{
			mDragMode = false;
			mCurrentOpBuilding.mSelected = false;
			mCurrentOpBuilding = null;
		}
	}
	
	public void DeletBuilding()
	{
		if(null != mCurrentOpBuilding && mCurrentOpBuilding.DeletEnable)
		{
			mEditBuildingList.Remove(mCurrentOpBuilding);
			Destroy(mCurrentOpBuilding.gameObject);
			mCurrentOpBuilding = null;
		}
	}
	
	public void ActiveEditor()
	{
		if(!mIsActive)
		{
			TownEditGui_N.Instance.Show();
			TownEditGui_N.Instance.ResetFileList();
			
			//GameGui_N.Instance.gameObject.SetActive(false);
			//BuildBlockManager.self.QuitBuildMode();
			
			//DragItem.Mgr.Instance.Clear();
			
			if(null != Block45Man.self.DataSource)
				Block45Man.self.DataSource.Clear();
			
			InitEdit();
			
			BGEffect.Instance.gameObject.SetActive(false);

			Block45Man.self.onPlayerPosReady(Camera.main.transform);
		}
	}
	
	public void Save()
	{
		if(mIsActive)
		{
			string _strFilePath = GameConfig.GetUserDataPath() + "/PlanetExplorers/Building/StoryBuildings.txt";
//			string _strFilePath = Application.dataPath + "/Resources/StoryBuildings.txt";
			
	        using(FileStream fileStream = new FileStream(_strFilePath, FileMode.Create, FileAccess.Write))
	        {
	            BinaryWriter _out = new BinaryWriter(fileStream);
	            if (_out == null)
	            {
	                Debug.LogError("On WriteRecord FileStream is null!");
	                return;
	            }
				_out.Write(Version);
				_out.Write(mEditBuildingList.Count);
				
				foreach(EditBuilding building in mEditBuildingList)
				{
					_out.Write(building.mBlockBuilding.mId);
					_out.Write(building.transform.position.x);
					_out.Write(building.transform.position.y + Block45Constants._scale);
					_out.Write(building.transform.position.z);
					_out.Write(Mathf.RoundToInt(building.transform.eulerAngles.y/90f));
				}
				
	            _out.Close();
	            fileStream.Close();
	        }
		}
	}
	
	public void InitEdit()
	{
		mIsActive = true;
		if(mPerfab == null) mPerfab = Block45CurMan.self; 
		for(int i = 0; i < mTownDataList.Count; i++)
		{
			BlockBuilding building = BlockBuilding.GetBuilding(mTownDataList[i].mBuildingId);

			GameObject editObj = new GameObject();
			editObj.name = "EditBuilding";
			editObj.transform.position = mTownDataList[i].mPosition;
			editObj.transform.rotation = Quaternion.Euler(0, mTownDataList[i].mRot * 90, 0);
			editObj.transform.localScale = Vector3.one;
			
			EditBuilding editBuilding = editObj.AddComponent<EditBuilding>();
			editBuilding.Init(building, mPerfab);
			mEditBuildingList.Add(editBuilding);
		}
	}
	
	public void Export(BinaryWriter bw)
	{
        bw.Write(Version);
		bw.Write(mActivelist.Count);
		for (int i = 0; i < mActivelist.Count; i++) {
			bw.Write ((int)mActivelist [i]);
		}
		
		bw.Write(mNpcList.Count);
		for(int i = 0; i < mNpcList.Count; i++)
		{
            NpcMissionData data = NpcMissionDataRepository.GetMissionData(mNpcList[i]);
            if (data == null)
            {
                bw.Write(-1);
            }
            else
            {
                bw.Write(mNpcList[i]);
                bw.Write(data.m_Rnpc_ID);
                bw.Write(data.m_QCID);
                bw.Write(data.m_CurMissionGroup);
                bw.Write(data.m_CurGroupTimes);
                bw.Write(data.mCurComMisNum);
                bw.Write(data.mCompletedMissionCount);
                bw.Write(data.m_RandomMission);
                bw.Write(data.m_RecruitMissionNum);

                bw.Write(data.m_MissionList.Count);
                for (int m = 0; m < data.m_MissionList.Count; m++)
                    bw.Write(data.m_MissionList[m]);

                bw.Write(data.m_MissionListReply.Count);
                for (int m = 0; m < data.m_MissionListReply.Count; m++)
                    bw.Write(data.m_MissionListReply[m]);
            }
		}
	}
	
	void Import(byte[] buffer)
	{
        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader _in = new BinaryReader(ms);
		/*int readVersion = */_in.ReadInt32();
		int count = _in.ReadInt32();
		for(int i = 0; i < count; i++)
			mActivelist.Add(_in.ReadInt32());
		
        count = _in.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            NpcMissionData data = new NpcMissionData();
            int npcid = _in.ReadInt32();

            if (npcid == -1)
                continue;

            data.m_Rnpc_ID = _in.ReadInt32();
            data.m_QCID = _in.ReadInt32();
            data.m_CurMissionGroup = _in.ReadInt32();
            data.m_CurGroupTimes = _in.ReadInt32();
            data.mCurComMisNum = _in.ReadByte();
            data.mCompletedMissionCount = _in.ReadInt32();
            data.m_RandomMission = _in.ReadInt32();
            data.m_RecruitMissionNum = _in.ReadInt32();

            int num = _in.ReadInt32();
            for (int j = 0; j < num; j++)
                data.m_MissionList.Add(_in.ReadInt32());

            num = _in.ReadInt32();
            for (int j = 0; j < num; j++)
                data.m_MissionListReply.Add(_in.ReadInt32());

            mNpcList.Add(npcid);
            NpcMissionDataRepository.AddMissionData(npcid, data);
        }
		
        _in.Close();
        ms.Close();
	}
	
	public void LoadData()
	{
//		
		string _strFilePath = GameConfig.GetUserDataPath() + "/PlanetExplorers/Building/StoryBuildings.txt";
		
		if(File.Exists(_strFilePath))
		{
			using (FileStream _fileStream = new FileStream(_strFilePath, FileMode.Open, FileAccess.Read))
			{
				BinaryReader _in = new BinaryReader(_fileStream);
				int readVersion = _in.ReadInt32();
				switch(readVersion)
				{
				case 1:
					int count = _in.ReadInt32();
					for(int i = 0; i < count; i++)
					{
						StoryTownData sTD = new StoryTownData();
						sTD.mId = i;
						sTD.mBuildingId = _in.ReadInt32();
						sTD.mPosition = new Vector3(_in.ReadSingle(),_in.ReadSingle(),_in.ReadSingle());;
						sTD.mRot = _in.ReadInt32();
						mTownDataList.Add(sTD);
					}
					break;
				}
				_in.Close();
			}
		}
		else
		{
			_strFilePath = "StoryBuildings";
			UnityEngine.Object file = Resources.Load(_strFilePath);
			if(file)
			{
				TextAsset textFile = file as TextAsset;
				MemoryStream ms = new MemoryStream(textFile.bytes);
				BinaryReader _in = new BinaryReader(ms);
				int readVersion = _in.ReadInt32();
				switch(readVersion)
				{
				case 1:
					int count = _in.ReadInt32();
					for(int i = 0; i < count; i++)
					{
						StoryTownData sTD = new StoryTownData();
						sTD.mId = i;
						sTD.mBuildingId = _in.ReadInt32();
						sTD.mPosition = new Vector3(_in.ReadSingle(),_in.ReadSingle(),_in.ReadSingle());;
						sTD.mRot = _in.ReadInt32();
						mTownDataList.Add(sTD);
					}
					break;
				}
				_in.Close();
				//			Resources.UnloadAsset(file);
			}
		}

		
        //if (!GameConfig.IsMultiMode && Record.m_RecordBuffer.ContainsKey((int)Record.RecordIndex.RECORD_STORYTOWN))
        //    Import(Record.m_RecordBuffer[(int)Record.RecordIndex.RECORD_STORYTOWN]);
		
		for(int i = 0; i < mTownDataList.Count; i++)
			if(!mActivelist.Contains(mTownDataList[i].mId))
				mUnactivedList.Add(mTownDataList[i]);
	}

    public Transform viewTrans;

	void Update()
	{
        if (null == viewTrans)
        {
            return;
        }

		foreach(StoryTownData sTD in mUnactivedList)
		{
			if(Vector3.Distance(viewTrans.position, sTD.mPosition) < ActiveBuildingDistance)
			{
				CreateBuilding(sTD);
				mActivelist.Add(sTD.mId);
				mUnactivedList.Remove(sTD);
				break;
			}
		}
	}
	
	void CreateBuilding(StoryTownData sTD)
	{
//		BlockBuilding bb = BlockBuilding.GetBuilding(sTD.mBuildingId);
//		
//        List<Vector3> npcPositionList;
//        List<CreatItemInfo> itemInfoList;
//        Dictionary<int, BuildingNpc> npcIdNum;
//		Vector3 size;
////        Dictionary<IntVector3, B45Block> retBuild = BuildBlockManager.self.BuildBuilding(sTD.mPosition, sTD.mBuildingId, sTD.mRot, out size
////			, out npcPositionList, out itemInfoList, out npcIdNum);
//		Dictionary<IntVector3, B45Block> retBuild = new Dictionary<IntVector3, B45Block>
//		
//		foreach(IntVector3 index in retBuild.Keys)
//			Block45Man.self.DataSource.SafeWrite(retBuild[index], index.x, index.y, index.z, 0);
//		
//		foreach(CreatItemInfo cii in itemInfoList)
//			DragArticleAgent.PutItemByProroId(cii.mItemId, cii.mPos, cii.mRotation);
//		
//		
//		List<int> missionIdList = new List<int>();
//		
//		foreach(int missionId in npcIdNum.Keys)
//				missionIdList.Add(missionId);
//		
//		int npcPosIndex = 0;
//		for(int i = 0; i < missionIdList.Count; i++)
//		{
//			if(i < npcPositionList.Count)
//			{
//				if (!NpcMissionDataRepository.m_AdRandMisNpcData.ContainsKey(missionIdList[i]))
//		        {
//		            LogManager.Error("not exist! id: ", missionIdList[i], " Pos: ", npcPositionList[npcPosIndex]);
//		            continue;
//		        }
//		        Vector3 npcPos = npcPositionList[npcPosIndex] + 0.2f * Vector3.up;
//				
//	            AdNpcData adNpcData = NpcMissionDataRepository.m_AdRandMisNpcData[missionIdList[i]];
//	            int RNpcId = adNpcData.mRnpc_ID;
//	            int Qid = adNpcData.mQC_ID;
//	
//                //NpcRandom nr = NpcManager.Instance.CreateRandomNpc(RNpcId, npcPos);
//                //StroyManager.Instance.NpcTakeMission(RNpcId, Qid,npcPos, nr, adNpcData.m_CSRecruitMissionList);
//				
//                //mNpcList.Add(nr.mNpcId);
//			}
//		}
	}
}
