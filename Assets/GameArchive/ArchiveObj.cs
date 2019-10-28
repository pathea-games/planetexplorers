using System.Collections.Generic;
using UnityEngine;

namespace Pathea
{
    public interface ISerializable
    {
        void Serialize(PeRecordWriter w);
    }

    public class ArchiveObj
    {
        public class SerializeObj
        {
            public bool yird;
            public string key;
            public ISerializable serialize;
        }

        public ArchiveObj(string recordName, bool saveNeedResetValue)
        {
            mRecordName = recordName;
            mSaveFlagResetValue = saveNeedResetValue;

            SetAllFlag(mSaveFlagResetValue);
        }

        public void SetAllFlag(bool flag)
        {
            for (int i = ArchiveMgr.ESave.Auto1 - ArchiveMgr.ESave.MinAuto
                ; i < ArchiveMgr.ESave.MaxAuto - ArchiveMgr.ESave.MinAuto
                ; i++)
            {
                mSaveFlag[i] = flag;
            }
        }

        string mRecordName;
        public string recordName
        {
            get
            {
                return mRecordName;
            }
        }

        bool mHasYird = false;

        public bool hasYird
        {
            get { return mHasYird; }
            set { mHasYird = value; }
        }

        bool mHasNonYird = false;

        public bool hasNonYird
        {
            get { return mHasNonYird; }
            set { mHasNonYird = value; }
        }

        List<SerializeObj> mItemList = new List<SerializeObj>(5);
        bool[] mSaveFlag = new bool[ArchiveMgr.ESave.MaxAuto - ArchiveMgr.ESave.MinAuto];
        bool mSaveFlagResetValue;

        void SetSaveFlagResetValue(bool value)
        {
            mSaveFlagResetValue = value || mSaveFlagResetValue;
        }

        public bool GetSaveFlag(ArchiveMgr.ESave eSave)
        {
            return mSaveFlag[eSave - ArchiveMgr.ESave.MinAuto];
        }

        public void ResetSaveFlag(ArchiveMgr.ESave eSave)
        {
            mSaveFlag[eSave - ArchiveMgr.ESave.MinAuto] = mSaveFlagResetValue;
        }

        public void Foreach(System.Action<SerializeObj> action)
        {
            mItemList.ForEach(action);
        }

        SerializeObj Find(string key)
        {
            return mItemList.Find((item) =>
            {
                if (item.key == key)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        void Add(string key, ISerializable serializableObj, bool yird)
        {
            mItemList.Add(new SerializeObj() {key = key, serialize = serializableObj, yird = yird });

            if (yird)
            {
                mHasYird = true;
            }
            else
            {
                mHasNonYird = true;
            }
        }

        public class List
        {
            string mDirName;

            List<ArchiveObj> mList;

            public List(int capacity)
            {
                mList = new List<ArchiveObj>(capacity);
            }

            public ArchiveObj FindByKey(string key)
            {
                return mList.Find((item) =>
                {
                    if (item.Find(key) != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });
            }

            ArchiveObj Get(string recordName)
            {
                return mList.Find((item) =>
                {
                    if (item.recordName == recordName)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });
            }

            public bool Add(string key, ISerializable serializableObj, bool yird, string recordName, bool saveFlagResetValue)
            {
                if (serializableObj == null)
                {
                    return false;
                }

                if (FindByKey(key) != null)
                {
                    Debug.LogError("serialize obj existed: "+key);
                    return false;
                }

                ArchiveObj obj = Get(recordName);
                if (obj == null)
                {
                    obj = new ArchiveObj(recordName, saveFlagResetValue);
                    mList.Add(obj);
                }
                else
                {
                    obj.SetSaveFlagResetValue(saveFlagResetValue);
                }
                obj.Add(key, serializableObj, yird);

                return true;
            }

            public void Foreach(System.Action<ArchiveObj> action)
            {
                mList.ForEach(action);
            }
        }
    }
}