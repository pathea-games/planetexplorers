using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pathea
{
    public abstract class ArchivableSingleton<T> : Pathea.MonoLikeSingleton<T>, Pathea.ISerializable where T : class, new()
    {
        protected override void OnInit()
        {
            base.OnInit();

            ArchiveMgr.Instance.Register(GetArchiveKey(), this, GetYird(), GetRecordName(), GetSaveFlagResetValue());
        }

        #region Archive Info
        protected virtual string GetArchiveKey()
        {
            return typeof(T).ToString();
        }

        protected virtual bool GetYird()
        {
            return true;
        }

        protected virtual string GetRecordName()
        {
            return ArchiveMgr.RecordNameWorld;
        }

        protected virtual bool GetSaveFlagResetValue()
        {
            return true;
        } 
        #endregion

        public void SaveMe()
        {
            ArchiveMgr.Instance.SaveMe(GetArchiveKey());
        }

        public virtual void New()
        {
        }

        public virtual void Restore()
        {
            byte[] data = ArchiveMgr.Instance.GetData(GetArchiveKey());
            if (data == null || data.Length == 0)
            {
                New();
            }
            else
            {
                SetData(data);
            }
        }

        void ISerializable.Serialize(PeRecordWriter w)
        {
            WriteData(w.binaryWriter);
        }

		protected abstract void WriteData(BinaryWriter bw);

        protected abstract void SetData(byte[] data);
    }
}