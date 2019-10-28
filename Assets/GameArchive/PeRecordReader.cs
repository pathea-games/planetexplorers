using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pathea
{
    public class PeRecordReader
    {
        FileStream mFileStream;
        BinaryReader mReader;
        ArchiveIndex mIndex;
        string mPath;
        Archive.Header mHeader;

        public PeRecordReader(ArchiveIndex index, string path, Archive.Header header)
        {
            mPath = path;
            mIndex = index;
            mHeader = header;
        }

        public byte[] ReadBytesDirect()
        {
            if (!Open())
            {
                return null;
            }

            byte[] buff = mReader.ReadBytes((int)mIndex.length);

            Close();

            return buff;
        }

        public int ReadInt32()
        {
            return mReader.ReadInt32();
        }

        public byte[] ReadBytes(int count)
        {
            return mReader.ReadBytes(count);
        }

        public BinaryReader binaryReader
        {
            get
            {
                return mReader;
            }
        }

        public bool Open()
        {
            try
            {
                string filePath = Archive.GetFilePath(mPath, mIndex.fileName);

                if (!File.Exists(filePath))
                {
                    Debug.LogError("file not exist:" + filePath);
                    return false;
                }


                mFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                mReader = new BinaryReader(mFileStream);

                Archive.Header header = new Archive.Header();

                if (!header.Read(mReader) || !header.IsMatch(mHeader))
                {
                    mReader.Close();
                    mReader = null;

                    mFileStream.Close();
                    mFileStream = null;

                    return false;
                }

                mFileStream.Seek(mIndex.beginPos, SeekOrigin.Begin);

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
                return false;
            }
        }

        public void Close()
        {
            mReader.Close();
            mFileStream.Close();
        }
    }
}