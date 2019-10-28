using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ItemAsset
{
    /// <summary>
    /// ItemObject传输时用的数据结构
    /// </summary>
    public class ItemObjectData
    {
        public int itemId;
        public int objId;
        public int num;
        public int[] properties;
        public float[] values;

        public ItemObjectData() { }

        public ItemObjectData(int itemId, int objId, int num, int[] properties, float[] values)
        {
            this.itemId = itemId;
            this.objId = objId;
            this.num = num;
            this.properties = properties;
            this.values = values;
        }




        /// <summary>
        /// ulink接收数据
        /// </summary>
        internal static object ReadItem(uLink.BitStream stream, params object[] codecOptions)
        {
            ItemObjectData _itemObjData = new ItemObjectData();
            _itemObjData.itemId = stream.Read<int>();
            _itemObjData.objId = stream.Read<int>();
            _itemObjData.num = stream.Read<int>();
            _itemObjData.properties = stream.Read<int[]>();
            _itemObjData.values = stream.Read<float[]>();


            return _itemObjData;
        }

        /// <summary>
        /// ulink发送数据
        /// </summary>
        internal static void WriteItem(uLink.BitStream stream, object obj, params object[] codecOptions)
        {
            ItemObjectData _itemObjData = (ItemObjectData)obj;
            stream.Write<int>(_itemObjData.itemId);
            stream.Write<int>(_itemObjData.objId);
            stream.Write<int>(_itemObjData.num);
            stream.Write<int[]>(_itemObjData.properties);
            stream.Write<float[]>(_itemObjData.values);
        }
    }
}
