using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pathea
{
    public class PeRecordWriter
    {
        BinaryWriter mWriter;
        string mKey;

        public PeRecordWriter(string key, BinaryWriter writer)
        {
            mKey = key;
            mWriter = writer;
        }

        public string key
        {
            get
            {
                return mKey;
            }
        }
        public BinaryWriter binaryWriter
        {
            get
            {
                return mWriter;
            }
        }

        public void Write(int value)
        {
            mWriter.Write(value);
        }

        public void Write(byte[] buff)
        {
			if(buff != null)
            	mWriter.Write(buff);
        }
    }
}