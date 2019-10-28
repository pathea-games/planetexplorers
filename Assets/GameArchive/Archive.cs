using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pathea
{
    public class Archive
    {
        public const string FileExtention = "arc";

        public class Header
        {
            public const int Version_0 = 0;
            public const int Version_1 = Version_0 + 1;
			public const int Version_2 = Version_1 + 1;
			public const int Version_3 = Version_2 + 1;
			public const int Version_4 = Version_3 + 1;
            public const int Version_5 = Version_4 + 1;

            const int CurrentVersion = Version_5;

            const int VerLength = sizeof(int);
            const int GuidLength = 16;
            const int VerGuidLength = VerLength + GuidLength;

            static Guid GenerateUid()
            {
                return Guid.NewGuid();
            }

            public int version
            {
                get
                {
                    return mVersion;
                }
            }

            int mVersion = CurrentVersion;
            Guid mGuid;

            public void NewGuid()
            {
                mGuid = GenerateUid();
            }

            public void Write(BinaryWriter w)
            {
                try
                {
                    w.Seek(0, SeekOrigin.Begin);

                    w.Write((int)CurrentVersion);

                    w.Write(mGuid.ToByteArray());
                }
                catch (Exception ex)
                {
					GameLog.HandleIOException(ex);
                }
            }

            public void BeginWriteCheckSum(BinaryWriter w)
            {
                try
                {
                    w.Seek(VerGuidLength, SeekOrigin.Begin);

                    //place holder
                    w.Write((long)0L);
                }
                catch (Exception ex)
                {
					GameLog.HandleIOException(ex);
                }
            }

            public void EndWriteCheckSum(BinaryWriter w)
            {
                try
                {
                    w.Seek(VerGuidLength, SeekOrigin.Begin);

                    w.Write((long)w.BaseStream.Length);
                }
                catch (Exception ex)
                {
					GameLog.HandleIOException(ex);
                }
            }

            public bool Read(BinaryReader r)
            {
                try
                {
                    mVersion = r.ReadInt32();
                    if (mVersion != CurrentVersion)
                    {
						//Debug.LogWarning("Error version:" + mVersion + "; need version:" + CurrentVersion);
                        return false;
                    }

                    byte[] b = r.ReadBytes(GuidLength);
                    mGuid = new Guid(b); 
                    
                    long length = r.ReadInt64();
                    if (length != r.BaseStream.Length)
                    {
                        Debug.LogError("Error check sum");
                        return false;
                    }
                    
                    return true;
                }
                catch (Exception ex)
                {
					GameLog.HandleIOException(ex);
                    return false;
                }
            }

            public bool IsMatch(Header other)
            {
                if(other == null)
                {
                    return false;
                }

				return 0 == mGuid.CompareTo(other.mGuid);
            }
        }

        class ArchiveIndexFile
        {
            const string IndexFileName = "index";

            const int VERSION_0000 = 0;
            const int CURRENT_VERSION = VERSION_0000;

            Dictionary<string, ArchiveIndex> mDicIndex = new Dictionary<string, ArchiveIndex>(10);

            Dictionary<string, ArchiveIndex> mYirdDicIndex = new Dictionary<string, ArchiveIndex>(10);

            string mYirdName;

            Archive.Header mHeader;

            public Archive.Header header
            {
                get { return mHeader; }
                set { mHeader = value; }
            }

            public ArchiveIndexFile() { }

            public string yirdName
            {
                get
                {
                    return mYirdName;
                }

                set
                {
                    mYirdName = value;
                }
            }

            static string GetIndexFilePath(string dir)
            {
                return GetFilePath(dir, IndexFileName);
            }

            public string GetYirdDir(string dir)
            {
                return Path.Combine(dir, yirdName);
            }

            public void Add(string key, ArchiveIndex index)
            {
                if (index.yird)
                {
                    mYirdDicIndex.Add(key, index);
                }
                else
                {
                    mDicIndex.Add(key, index);
                }
            }

            public ArchiveIndex GetArchiveIndex(string key)
            {
                if (mDicIndex.ContainsKey(key))
                {
                    return mDicIndex[key];
                }
                else if (mYirdDicIndex.ContainsKey(key))
                {
                    return mYirdDicIndex[key];
                }

                return null;
            }

            public bool Write(string dir)
            {
                if (!Write(mHeader, GetIndexFilePath(dir), mDicIndex))
                {
                    return false;
                }

                if (!Write(mHeader, GetIndexFilePath(GetYirdDir(dir)), mYirdDicIndex))
                {
                    return false;
                }

                return true;
            }

            static bool Write(Archive.Header header, string path, Dictionary<string, ArchiveIndex> dic)
            {
                try
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                    {
                        using (BinaryWriter bw = new BinaryWriter(fs))
                        {
                            header.Write(bw);
                            header.BeginWriteCheckSum(bw);

                            bw.Write((int)dic.Count);

                            foreach (KeyValuePair<string, ArchiveIndex> kvp in dic)
                            {
                                bw.Write(kvp.Key);

                                kvp.Value.Write(bw);
                            }

                            header.EndWriteCheckSum(bw);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex);
                    return false;
                }

                return true;
            }

            bool Load(string dir)
            {
                string path = GetIndexFilePath(dir);

                header = Load(path, mDicIndex);

                return header != null;
            }

            public bool LoadYird(string dir, string yirdName)
            {
                if (string.IsNullOrEmpty(yirdName))
                {
                    return false;
                }

                mYirdName = yirdName;

                string path = GetIndexFilePath(GetYirdDir(dir));

                Archive.Header yirdHeader = Load(path, mYirdDicIndex, header);
                if (yirdHeader == null)
                {
                    return false;
                }

                return true;
            }

            static Archive.Header Load(string path, Dictionary<string, ArchiveIndex> dic, Header curHeader = null)
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                try
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            Archive.Header header = new Header();
                            if (!header.Read(br))
                            {
                                return null;
                            }

                            if (curHeader != null && !curHeader.IsMatch(header))
                            {
                                return null;
                            }

                            dic.Clear();

                            int count = br.ReadInt32();

                            for (int i = 0; i < count; i++)
                            {
                                string key = br.ReadString();
                                ArchiveIndex index = new ArchiveIndex();
                                index.Read(br);

                                dic.Add(key, index);
                            }

                            return header;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex);
                    return null;
                }
            }

            public static ArchiveIndexFile LoadFromFile(string dir, string yirdName = null)
            {
                ArchiveIndexFile archiveIndexFile = new ArchiveIndexFile();
                if (!archiveIndexFile.Load(dir))
                {
                    return null;
                }

                archiveIndexFile.LoadYird(dir, yirdName);

                return archiveIndexFile;
            }
        }

        ArchiveIndexFile mIndexFile;
        string mDir;

        public Archive(string dir)
        {
            mDir = dir;
        }

        public byte[] GetData(string key)
        {
            PeRecordReader r = GetReader(key);
            if (null == r)
            {
                return null;
            }

            return r.ReadBytesDirect();
        }

        public bool LoadYird(string yirdName)
        {
            if (mIndexFile == null)
            {
                return false;
            }

            return mIndexFile.LoadYird(dir, yirdName);
        }

        public Header GetHeader()
        {
            if (mIndexFile == null)
            {
                return null;
            }

            return mIndexFile.header;
        }

        public PeRecordReader GetReader(string key)
        {
            if (mIndexFile == null)
            {
                return null;
            }

            ArchiveIndex index = mIndexFile.GetArchiveIndex(key);

            if (null == index)
            {
                return null;
            }

            string fileDir;
            if (index.yird)
            {
                if (string.IsNullOrEmpty(mIndexFile.yirdName))
                {
                    Debug.LogError("yird name is empty or null");
                    return null;
                }

                fileDir = mIndexFile.GetYirdDir(dir);
            }
            else
            {
                fileDir = dir;
            }

            return new PeRecordReader(index, fileDir, mIndexFile.header);
        }

        public string dir
        {
            get
            {
                return mDir;
            }
        }

        //string GetYirdDir(string yirdName)
        //{
        //    return Path.Combine(dir, yirdName);
        //}

        public static string GetFilePath(string dir, string name)
        {
            return Path.Combine(dir, name + "." + FileExtention);
        }

        public delegate bool SaveRecordObj(ArchiveObj obj);

        public bool WriteToFile(ArchiveObj.List serializeObjList, string yirdName, Archive.Header header, SaveRecordObj saveRecordObj)
        {
            try
            {
                if(!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                mIndexFile = new ArchiveIndexFile();
                mIndexFile.yirdName = yirdName;
                mIndexFile.header = header;
                
				bool bOldIdxFileLoaded = false;
				ArchiveIndexFile oldIndexFile = null;
                Profiler.BeginSample("Archive");
                serializeObjList.Foreach((serializeObj) =>
                {
                    bool bWrite = true;
                    if (saveRecordObj != null)
                    {
                        bWrite = saveRecordObj(serializeObj);
                    }

                    if (bWrite)
                    {
                        WriteRecord(mIndexFile, serializeObj);
                    }
                    else
                    {
						if(!bOldIdxFileLoaded){
							oldIndexFile = ArchiveIndexFile.LoadFromFile(dir, yirdName);
							bOldIdxFileLoaded = true;
						}
                        CopyIndex(mIndexFile, oldIndexFile, serializeObj);
                    }
                });
                Profiler.EndSample();

                mIndexFile.Write(dir);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
                return false;
            }

            return true;
        }

        static void CopyIndex(ArchiveIndexFile indexFile, ArchiveIndexFile oldIndexFile, ArchiveObj serializeObj)
        {
            if (oldIndexFile == null)
            {
                return;
            }

            serializeObj.Foreach((obj) =>
            {
                Profiler.BeginSample("only copy index:" + obj.key);

                ArchiveIndex index = oldIndexFile.GetArchiveIndex(obj.key);
                if (index != null)
                {
                    indexFile.Add(obj.key, index);
                }

                Profiler.EndSample();
            });
        }

        void WriteRecord(ArchiveIndexFile indexFile, ArchiveObj serializeObj)
        {
            if (serializeObj.hasNonYird)
            {
                string filePath = GetFilePath(dir, serializeObj.recordName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter writer = new BinaryWriter(fileStream))
                    {
                        indexFile.header.Write(writer);
                        indexFile.header.BeginWriteCheckSum(writer);

                        serializeObj.Foreach((obj) =>
                        {
                            if (!obj.yird)
                            {
								Profiler.BeginSample("save:" + obj.key);
                                long beginPos = fileStream.Position;
                                obj.serialize.Serialize(new PeRecordWriter(obj.key, writer));
                                long endPos = fileStream.Position;

                                indexFile.Add(obj.key, new ArchiveIndex(serializeObj.recordName, obj.yird, beginPos, endPos));
								Profiler.EndSample();
                            }
                        });

                        indexFile.header.EndWriteCheckSum(writer);
                    }
                }
            }

            if (serializeObj.hasYird)
            {
                string yirdDir = indexFile.GetYirdDir(dir);
                string yirdFilePath = GetFilePath(yirdDir, serializeObj.recordName);

                if (!Directory.Exists(yirdDir))
                {
                    Directory.CreateDirectory(yirdDir);
                }

                using (FileStream yirdFileStream = new FileStream(yirdFilePath, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter yirdWriter = new BinaryWriter(yirdFileStream))
                    {
                        indexFile.header.Write(yirdWriter);
                        indexFile.header.BeginWriteCheckSum(yirdWriter);

                        serializeObj.Foreach((obj) =>
                        {
                            Profiler.BeginSample("save:" + obj.key);

                            if (obj.yird)
                            {
                                long beginPos = yirdFileStream.Position;

                                obj.serialize.Serialize(new PeRecordWriter(obj.key, yirdWriter));

                                long endPos = yirdFileStream.Position;

                                indexFile.Add(obj.key, new ArchiveIndex(serializeObj.recordName, obj.yird, beginPos, endPos));
                            }

                            Profiler.EndSample();
                        });

                        indexFile.header.EndWriteCheckSum(yirdWriter);
                    }
                }
            }
        }

        public bool LoadFromFile()
        {
            try
            {
                if (!Directory.Exists(dir))
                {
                    return false;
                }

                mIndexFile = ArchiveIndexFile.LoadFromFile(dir);
				return mIndexFile != null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
                return false;
            }
        }
    }
}