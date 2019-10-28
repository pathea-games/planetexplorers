using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pathea
{
    public class ArchiveIndex
    {
        string mFileName;
        long mBeginPos;
        long mLength;
        bool mYird;

        public ArchiveIndex() { }
        public ArchiveIndex(string fileName, bool yird, long beginPos, long endPos)
        {
            mFileName = fileName;
            mYird = yird;
            mBeginPos = beginPos;
            mLength = endPos - beginPos;
        }

        public string fileName
        {
            get
            {
                return mFileName;
            }
        }

        public bool yird
        {
            get
            {
                return mYird;
            }
        }

        public long beginPos
        {
            get
            {
                return mBeginPos;
            }
        }

        public long length
        {
            get
            {
                return mLength;
            }
        }

        //public long endPos
        //{
        //    get
        //    {
        //        return beginPos + length;
        //    }

        //    set
        //    {
        //        mLength = value - beginPos;
        //    }
        //}

        public void Read(BinaryReader r)
        {
            mFileName = r.ReadString();
            mYird = r.ReadBoolean();
            mBeginPos = r.ReadInt64();
            mLength = r.ReadInt64();
        }

        public void Write(BinaryWriter w)
        {
            w.Write(mFileName);
            w.Write((bool)mYird);
            w.Write((long)mBeginPos);
            w.Write((long)mLength);
        }
    }
}