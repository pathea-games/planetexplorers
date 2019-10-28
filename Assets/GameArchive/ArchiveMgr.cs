using System.IO;
using UnityEngine;
using System.Linq;


namespace Pathea
{
    public class ArchiveMgr : MonoLikeSingleton<ArchiveMgr>
    {
        public const string RecordNameWorld = "world";

        const string UserSaveFileNamePrefix = "save";
        const string AutoSaveFileNamePrefix = "auto";
        const string ArchiveDirName = "GameSave";

        public enum ESave
        {
            Min = 0,
            MinAuto = Min,
            Auto1 = MinAuto,
            Auto2,
            Auto3,
            MaxAuto,
            MinUser = MaxAuto,
            User1 = MinUser,
            User2,
            User3,
            User4,
            User5,
            User6,
            User7,
            User8,
            User9,
            User10,
            User11,
            User12,
            User13,
            User14,
            User15,
            User16,
            User17,
            User18,
            User19,
            User20,
            MaxUser,
            Max = MaxUser,
            New = Max,
        }

        public class ArchiveEvent : PeEvent.EventArg
        {
            public ESave eSave;
        }

        PeEvent.Event<ArchiveEvent> mEventor;

        public PeEvent.Event<ArchiveEvent> eventor
        {
            get
            {
                if (mEventor == null)
                {
                    mEventor = new PeEvent.Event<ArchiveEvent>(this);
                }

                return mEventor;
            }
        }

        ArchiveObj.List mArchiveObjList = new ArchiveObj.List(30);
        string mYirdName;
        string yirdName
        {
            get
            {
                return mYirdName;
            }
        }

        Archive mCurArchive = null;
        ESave mCurSave = ESave.Max;

        SwapSpace mSwapSpace = new SwapSpace();

        Archive.Header[] mArchiveHeaders = new Archive.Header[ESave.Max - ESave.Min];

        string CreateCurArchive(ESave eSave)
        {
            string dir = GetArchiveDir(eSave);

            mCurSave = eSave;

            mCurArchive = new Archive(dir);

            return dir;
        }

        void RemoveCurArchive()
        {
            mCurSave = ESave.Max;
            mCurArchive = null;
        }

        static string GetArchiveRootDir()
        {
            return Path.Combine(GameConfig.GetPeUserDataPath(), ArchiveDirName);
        }

        static string GetArchiveDir(string dirName)
        {
            return Path.Combine(GetArchiveRootDir(), dirName);
        }

        static string GetArchiveDir(ESave eSave)
        {
            string fileName;

            if (eSave < ESave.MaxAuto)
            {
                fileName = AutoSaveFileNamePrefix + (int)eSave;
            }
            else
            {
                fileName = UserSaveFileNamePrefix + ((int)eSave - (int)ESave.MinUser);
            }

            return GetArchiveDir(fileName);
        }

        //void CleanArchiveDir(string dir)
        //{
        //    try
        //    {
        //        DirectoryInfo dirInfo = new DirectoryInfo(dir);

        //        if (dirInfo.Exists)
        //        {
        //            FileInfo[] fileInfos = dirInfo.GetFiles("*", SearchOption.AllDirectories);

        //            foreach (FileInfo fileInfo in fileInfos)
        //            {
        //                string relativePath = fileInfo.FullName.Remove(0, dirInfo.FullName.Length + 1);

        //                if (!mSwapSpace.Exists(relativePath))
        //                {
        //                    fileInfo.Delete();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            dirInfo.Create();
        //        }

        //        //if (Directory.Exists(dir))
        //        //{
        //        //    Directory.Delete(dir, true);
        //        //}

        //        //Directory.CreateDirectory(dir);
        //    }
        //    catch (System.Exception ex)
        //    {
		//        GameLog.HandleIOException(ex);
        //    }
        //}

        Archive.Header GetHeader(ESave eSave)
        {
            return mArchiveHeaders[eSave - ESave.Min];
        }

        void SetHeader(ESave eSave, Archive.Header header)
        {
            mArchiveHeaders[eSave - ESave.Min] = header;
        }

        void SaveArchive(ESave eSave)
        {
            eventor.Dispatch(new ArchiveEvent() { eSave = eSave });

            string dir = CreateCurArchive(eSave);

            Archive.Header header = GetHeader(eSave);

            if (header == null)
            {
                header = new Archive.Header();
                header.NewGuid();

                SetHeader(eSave, header);

				if(!mSwapSpace.CopyTo(dir, (fileInfo) => {
	                    using (FileStream fs = fileInfo.Open(FileMode.Open, FileAccess.Write)){
	                        using (BinaryWriter w = new BinaryWriter(fs)){
	                            header.Write(w);                            
	                        }
	                    }
	                })){
					Debug.LogError("[Save]:Failed to save game at "+eSave);
					return; // Failed to save
				}
            }

            Profiler.BeginSample("write archive:"+eSave);

            mCurArchive.WriteToFile(mArchiveObjList, yirdName, header, (recordObj) =>
            {
                if (eSave >= ESave.MinAuto && eSave < ESave.MaxAuto)
                {
                    bool saveFlag = recordObj.GetSaveFlag(eSave);

                    recordObj.ResetSaveFlag(eSave);

                    return saveFlag;
                }
                else
                {
                    return true;
                }
            });

            Profiler.EndSample();

            //Profiler.BeginSample("sync swap :");

            //mSwapSpace.Sync(mCurArchive.dir);

            //Profiler.EndSample();
        }

        public void Register(string key, ISerializable serializableObj
            , bool yird = false, string recordName = RecordNameWorld, bool saveFlagResetValue = true)
        {
            mArchiveObjList.Add(key, serializableObj, yird, recordName, saveFlagResetValue);
        }

        public bool SaveMe(string key)
        {
            ArchiveObj record = mArchiveObjList.FindByKey(key);
            if (record == null)
            {
                return false;
            }

            record.SetAllFlag(true);
            return true;
        }

        public bool LoadYird(string yirdName)
        {
            mYirdName = yirdName;

            if (mCurArchive == null)
            {
                return false;
            }

            return mCurArchive.LoadYird(yirdName);
        }

        public byte[] GetData(string key)
        {
            return mCurArchive.GetData(key);
        }

        public int GetCurArvhiveVersion()
        {
            Archive.Header header = mCurArchive.GetHeader();

            return header.version;
        }

        public PeRecordReader GetReader(string key)
        {
            return mCurArchive.GetReader(key);
        }

        public void QuitSave()
        {
            Debug.Log("<color=aqua> game quit, save archive to auto1. </color>");
            Save(ESave.Auto1);
        }

        public void Save(ESave eSave)
        {
#if !UNITY_EDITOR
			if (GameLog.IsFatalError) {
				return;
			}
#endif

			if (Pathea.PeGameMgr.playerType == Pathea.PeGameMgr.EPlayerType.Tutorial)//||(Pathea.PeGameMgr.yirdName ==Pathea.AdventureScene.Dungen.ToString()&&Pathea.PeGameMgr.targetYird!=Pathea.AdventureScene.MainAdventure.ToString()))
            {
                return;
            }

			autoSave = eSave < ESave.MaxAuto;
            SaveArchive(eSave);
			autoSave = false;
        }

        public void LoadAndCleanSwap(ESave eSave)
        {
            mSwapSpace.Init();
            //mSwapSpace.Init(Path.Combine(GetArchiveRootDir(), "swap"));

            string dir = Load(eSave);

            if (!string.IsNullOrEmpty(dir))
            {
                Archive.Header header = mCurArchive.GetHeader();
                mSwapSpace.CopyFrom(dir, (fileInfo) =>
                {
					try{
	                    using (FileStream fs = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
	                    {
	                        using (BinaryReader r = new BinaryReader(fs))
	                        {
	                            Archive.Header other = new Archive.Header();
	                            if (other.Read(r))
	                            {
	                                if (header.IsMatch(other))
	                                {
	                                    return true;
	                                }
	                            }
	                        }
	                    }
					}catch(System.Exception e){
						Debug.LogWarning("[LoadAndCleanSwap]"+e);
					}
                    return false;
                });
            }
        }

        public string Load(ESave eSave)
        {
            string dir = null;

            if (eSave != ESave.New)
            {
                dir = CreateCurArchive(eSave);

                if (!mCurArchive.LoadFromFile())
                {
                    dir = null;
                }
            }

            return dir;
        }

        public void Delete(ESave eSave)
        {
            if (eSave == mCurSave)
            {
                RemoveCurArchive();
            }
			try{
            	Directory.Delete(GetArchiveDir(eSave), true);
			}catch{}
        }

        public bool autoSave
        {
            get;
            private set;
        }
    }
}