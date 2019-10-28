using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TownData
{
    public class VATownNpcInfo
    {
        IntVector2 index;

        public IntVector2 Index
        {
            get { return index; }
            set { index = value; }
        }

        Vector3 position;
        int id;
		public int townId;
        public float PosY
        {
            set { position.y = value; }
            get { return position.y; }
        }
        
        public VATownNpcInfo(IntVector2 index,int id)
        {
            this.index = index;
            this.id = id;
            position = new Vector3(index.x, -1, index.y);
        }
        public VATownNpcInfo(Vector3 npcPos, int id)
        {
            this.index = new IntVector2(Mathf.RoundToInt(npcPos.x), Mathf.RoundToInt(npcPos.z));
            this.id = id;
            position = npcPos;
        }

        public Vector3 getPos()
        {
            return position;
        }

        public int getId()
        {
            return id;
        }

        public void setPosY(float y)
        {
            position.y = y;
        }

        public float getPosY()
        {
            return position.y;
        }
    
        internal static object DeserializeInfo(uLink.BitStream stream, params object[] codecOptions)
        {
            IntVector2 index = stream.Read<IntVector2>();
            int id = stream.Read<int>();
            VATownNpcInfo townNpcInfo = new VATownNpcInfo(index,id);
            return townNpcInfo;
        }

        internal static void SerializeInfo(uLink.BitStream stream, object value, params object[] codecOptions)
        {
            VATownNpcInfo townNpcInfo = value as VATownNpcInfo;
            stream.Write<IntVector2>(townNpcInfo.index);
            stream.Write<int>(townNpcInfo.id);
        }
    }
    
}
