using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TownData
{
    public class NativePointInfo
    {
        public IntVector2 index;
        public Vector3 position;
        public int id;

        public int townId;

        public int ID {
            get {
                return id;
            }
        }

        public NativePointInfo(IntVector2 index,int id)
        {
            this.index = index;
            this.id = id;

            position = new Vector3(index.x, -1, index.y);
        }
		public NativePointInfo(Vector3 pos,int id)
		{
			this.index = new IntVector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
			this.id = id;
			position = pos;
		}
        public void SetPosY(float height)
        {
            position.y = height;
        }

        public float PosY
        {
            get { return position.y; }
        }
        
    }
}
