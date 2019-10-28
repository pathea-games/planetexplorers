using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pathea
{
    public class IdGenerator
    {
        public const int Invalid = -1;

        const int DefaultBegin = 0;
        const int DefaultEnd = DefaultBegin + 100000;

        int mCurId;
        int mEnd;
        int mBegin;

        public IdGenerator(int curId):this(curId, DefaultBegin, DefaultEnd)
        {
        }

        public IdGenerator(int curId, int min, int max)
        {
            mCurId = curId;
            mBegin = min;
            mEnd = max;
        }

        public int Cur
        {
            get
            {
                return mCurId;
            }
            set
            {
                mCurId = value;
            }
        }

        public int Fetch()
        {
            if (mCurId > mEnd || mCurId < mBegin)
            {
                Debug.LogError("id generater pool has run out, use it from start.");
                mCurId = mBegin;
            }

            return mCurId++;
        }

		public void Export(BinaryWriter bw)
        {
			bw.Write ((int)Cur);
        }
        public void Import(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer, false))
            {
                using (BinaryReader _in = new BinaryReader(ms))
                {
                    Cur = _in.ReadInt32();
                }
            }
        }
		public void Serialize(BinaryWriter bw)
		{
			bw.Write((int)Cur);
		}
		public void Deserialize(BinaryReader br)
		{
			Cur = br.ReadInt32();
		}
    }
}