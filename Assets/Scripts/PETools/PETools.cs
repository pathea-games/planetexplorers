using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using Mono.Data.SqliteClient;
using System.Collections.Generic;

namespace PETools
{
    public static class PE
    {
        // Check a point in water or not
        public static float PointInWater(Vector3 point)
        {
            if (null == VFVoxelWater.self)
                return 0.0f;
            return VFVoxelWater.self.IsInWater(point.x, point.y, point.z) ? 1.0f : 0.0f;
        }
        // Check a point in terrain or not
        public static float PointInTerrain(Vector3 point)
        {
            if (point.y < 0)
                return 1.0f;
            return (float)(VFVoxelTerrain.self.Voxels.SafeRead(
                Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), Mathf.FloorToInt(point.z)).Volume) / 255.0f;
        }
        // Raycast voxel.
        // layer: terrain 1
        //        water 2 
        // voxel: true - raycast a point has voxel
        //        false - raycast a point doesn't have voxel
        public static bool RaycastVoxel(Ray ray, out Vector3 point, int maxdist, int step, int layer, bool voxel = true)
        {
            point = Vector3.zero;
            float dist = maxdist + step;
            for (float d = 0; d <= maxdist; d += step)
            {
                Vector3 pos = ray.GetPoint(d);
                if ((layer & 1) > 0 && (PointInTerrain(pos) > 0.52f) == voxel ||
                     (layer & 2) > 0 && (PointInWater(pos) > 0.52f) == voxel)
                {
                    dist = d - step + 1;
                    break;
                }
            }
            if (dist > maxdist)
                return false;
            if (dist < 0)
            {
                point = ray.origin;
                return true;
            }

            float final_dist = dist;
            for (float d = dist; d < dist + step && d <= maxdist; d += 1)
            {
                Vector3 pos = ray.GetPoint(d);
                if ((layer & 1) > 0 && (PointInTerrain(pos) > 0.52f) == voxel ||
                     (layer & 2) > 0 && (PointInWater(pos) > 0.52f) == voxel)
                {
                    final_dist = d;
                    break;
                }
            }
            point = ray.GetPoint(final_dist);
            return true;
        }

		public static bool CheckHumanSafePos(Vector3 pos)
		{
			if(PETools.PEUtil.CheckPositionUnderWater(pos))
				return true;
			
			if(null != VFVoxelTerrain.self)
				return VFVoxelTerrain.self.Voxels.SafeRead(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y + 0.5f), Mathf.RoundToInt(pos.z)).Volume < 128
					&& VFVoxelTerrain.self.Voxels.SafeRead(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y + 1.5f), Mathf.RoundToInt(pos.z)).Volume < 128;
			return false;
		}

		public static bool FindHumanSafePos(Vector3 originPos, out Vector3 safePos, int checkDis = 10)
		{
			for(int i = 0; i < checkDis; i++)
			{
				if(CheckHumanSafePos(originPos + i * Vector3.up))
				{
					safePos = originPos + i * Vector3.up;
					return true;
				}
			}

			safePos = originPos;
			return false;
		}
		
		public static RagdollHitInfo CapsuleHitToRagdollHit(Pathea.PECapsuleHitResult hitResult)
		{
			RagdollHitInfo hitInfo = new RagdollHitInfo();
			hitInfo.hitTransform = hitResult.hitTrans;
			hitInfo.hitPoint = hitResult.hitPos;
			hitInfo.hitForce = hitResult.hitDir * 1000.0f;
			return hitInfo;
		}

		public static bool WeaponCanCombat(Pathea.PeEntity entity,IWeapon weapon)
		{
			return ItemAsset.SelectItem.EquipCanAttack(entity,weapon.ItemObj);//weapon.ItemObj != null && weapon.ItemObj.protoData.weaponInfo != null;
		}

    }

    public static class Serialize
    {
		public static void WriteData(Action<System.IO.BinaryWriter> writer, System.IO.BinaryWriter bw)
		{
			long posLen = bw.BaseStream.Position;
			bw.Write((int)0);

			if (writer != null) {
				long posBeg = bw.BaseStream.Position;
				writer (bw);
				long posEnd = bw.BaseStream.Position;

				if (posEnd != posBeg) {
					bw.BaseStream.Position = posLen;
					bw.Write ((int)(posEnd - posBeg));
					bw.BaseStream.Position = posEnd;
				}
			}
		}

        static public void WriteBytes(byte[] buff, System.IO.BinaryWriter w)
        {
            if (null != buff && buff.Length > 0)
            {
                w.Write((int)buff.Length);
                w.Write(buff);
            }
            else
            {
                w.Write((int)0);
            }
        }

        static public byte[] ReadBytes(System.IO.BinaryReader r)
        {
			int len;
			if(r.BaseStream.Position > r.BaseStream.Length - sizeof(int) || (len = r.ReadInt32()) <= 0)
            {
                return null;
            }
            else
            {
                return r.ReadBytes(len);
            }
        }

        static public void WriteColor(System.IO.BinaryWriter bw, Color c)
        {
            bw.Write((float)c.r);
            bw.Write((float)c.g);
            bw.Write((float)c.b);
            bw.Write((float)c.a);
        }

        static public Color ReadColor(System.IO.BinaryReader br)
        {
            Color c;
            c.r = br.ReadSingle();
            c.g = br.ReadSingle();
            c.b = br.ReadSingle();
            c.a = br.ReadSingle();
            return c;
        }

        static public Vector3 ReadVector3(System.IO.BinaryReader r)
        {
            float x = r.ReadSingle();
            float y = r.ReadSingle();
            float z = r.ReadSingle();

            return new Vector3(x, y, z);
        }

        static public void WriteVector3(System.IO.BinaryWriter w, Vector3 v)
        {
            w.Write(v.x);
            w.Write(v.y);
            w.Write(v.z);
        }

        public static void WriteVector4(System.IO.BinaryWriter w, IntVector4 v)
        {
            w.Write(v.x);
            w.Write(v.y);
            w.Write(v.z);
            w.Write(v.w);
        }

        public static IntVector4 ReadVector4(BinaryReader r)
        {
            int x = r.ReadInt32();
            int y = r.ReadInt32();
            int z = r.ReadInt32();
            int w = r.ReadInt32();

            return new IntVector4(x, y, z, w);
        }

        static public Quaternion ReadQuaternion(System.IO.BinaryReader r)
        {
            float x = r.ReadSingle();
            float y = r.ReadSingle();
            float z = r.ReadSingle();
            float w = r.ReadSingle();

            return new Quaternion(x, y, z, w);
        }

        static public void WriteQuaternion(System.IO.BinaryWriter w, Quaternion v)
        {
            w.Write(v.x);
            w.Write(v.y);
            w.Write(v.z);
            w.Write(v.w);
        }

        static public void WriteNullableString(System.IO.BinaryWriter w, string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                w.Write(true);
            }
            else
            {
                w.Write(false);
                w.Write(v);
            }
        }

        static public string ReadNullableString(System.IO.BinaryReader r)
        {
            bool isNull = r.ReadBoolean();
            if (isNull)
            {
                return null;
            }
            else
            {
                return r.ReadString();
            }
        }

        public class PeExporter
        {
            public delegate void Export(BinaryWriter w);

            public byte[] Do(Export export, int suggestCapacity = 100)
            {
                if (null == export)
                {
                    return null;
                }

                try
                {
                    using (MemoryStream ms = new MemoryStream(suggestCapacity))
                    {
                        using (BinaryWriter w = new BinaryWriter(ms))
                        {
                            export(w);
                        }
                        return ms.ToArray();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("catched exception:" + e);
                    return null;
                }
            }
        }

        public class PeImporter
        {
            public delegate void Import(BinaryReader r);

            public void Do(byte[] buff, Import import)
            {
                if (null == buff || null == import)
                {
                    return;
                }

                try
                {
                    using (MemoryStream ms = new MemoryStream(buff, false))
                    {
                        using (BinaryReader r = new BinaryReader(ms))
                        {
                            import(r);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("catched exception:" + e);
                }
            }
        }

        static PeExporter exporter;
        static PeExporter Exporter
        {
            get
            {
                if (null == exporter)
                {
                    exporter = new PeExporter();
                }
                return exporter;
            }
        }
        public static byte[] Export(PeExporter.Export export, int suggestCapacity = 100)
        {
            return Exporter.Do(export, suggestCapacity);
        }
        static PeImporter importer;
        static PeImporter Importer
        {
            get
            {
                if (null == importer)
                {
                    importer = new PeImporter();
                }
                return importer;
            }
        }
        public static void Import(byte[] buff, PeImporter.Import import)
        {
            Importer.Do(buff, import);
        }
    }

    /// <summary>
    /// supperted type: int,float,string,bool,Enum(int Enum, string Enum), int[]
    /// </summary>
    public static class DbReader
    {
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
        public class DbFieldAttribute : Attribute
        {
            public DbFieldAttribute(string fieldName, bool enumValue = false)
            {
                FieldName = fieldName;
                EnumValue = enumValue;
            }

            public string FieldName;
            public bool EnumValue;
        }

        public static List<T> Read<T>(string tableName, int capacity = 20) where T : class, new()
        {
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable(tableName);
            if (reader == null)
            {
                return null;
            }

            return Read<T>(reader, capacity);
        }

        public static List<T> Read<T>(SqliteDataReader reader, int capacity = 20) where T : class, new()
        {
            /*MemberInfo[] mems = */GetFieldProperty<T>();

            System.Type t = typeof(T);
            FieldInfo[] fieldInfos = GetFieldInfos(t);
            PropertyInfo[] propertyInfos = GetPropertyInfos(t);

            List<T> list = new List<T>(capacity);

            while (reader.Read())
            {
                T item = BuildItem<T>(reader, fieldInfos, propertyInfos);

                list.Add(item);
            }

            return list;
        }

        public static T ReadItem<T>(SqliteDataReader reader) where T : class, new()
        {
            System.Type t = typeof(T);
            FieldInfo[] fieldInfos = GetFieldInfos(t);
            PropertyInfo[] propertyInfos = GetPropertyInfos(t);

            return BuildItem<T>(reader, fieldInfos, propertyInfos);
        }

        private static MemberInfo[] GetFieldProperty<T>() where T : class, new()
        {
            System.Type t = typeof(T);

            MemberInfo[] mems = t.GetMembers(BindingFlags.Instance
                | BindingFlags.NonPublic
                | BindingFlags.Public
                );

            /*FieldInfo[] fieldInfos = */t.GetFields(BindingFlags.Instance);
            return mems;
        }

        private static FieldInfo[] GetFieldInfos(System.Type t)
        {
            return t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private static PropertyInfo[] GetPropertyInfos(System.Type t)
        {
            return t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        static DbFieldAttribute GetDbFieldAttribute(MemberInfo memInfo)
        {
            object[] attrs = memInfo.GetCustomAttributes(typeof(DbFieldAttribute), true);
            
            return (attrs.Length > 0) ? (attrs[0] as DbFieldAttribute) : (null);
        }

        private static T BuildItem<T>(SqliteDataReader reader, FieldInfo[] fieldInfos, PropertyInfo[] propertyInfos) where T : class, new()
        {
            T item = new T();

            foreach (FieldInfo field in fieldInfos)
            {
                DbFieldAttribute dbFieldAttr = GetDbFieldAttribute(field);
                if (dbFieldAttr == null)
                {
                    continue;
                }

                object v = GetValue(reader, field.FieldType, dbFieldAttr);

                field.SetValue(item, v);
            }

            foreach (PropertyInfo property in propertyInfos)
            {
                DbFieldAttribute dbFieldAttr = GetDbFieldAttribute(property);
                if (dbFieldAttr == null)
                {
                    continue;
                }

                object v = GetValue(reader, property.PropertyType, dbFieldAttr);

                property.SetValue(item, v, null);
            }

            return item;
        }

        static object GetValue(SqliteDataReader reader, System.Type fieldType, DbFieldAttribute attr)
        {
            object v = null;

            if (fieldType == typeof(int))
            {
                v = Db.GetInt(reader, attr.FieldName);
            }
			else if (fieldType == typeof(int[]))
			{
				v = PETools.Db.GetIntArray(reader, attr.FieldName);
			}
			else if (fieldType == typeof(float))
            {
                v = Db.GetFloat(reader, attr.FieldName);
            }			
			else if (fieldType == typeof(float[]))
			{
				v = PETools.Db.GetFloatArray(reader, attr.FieldName);
			}
            else if (fieldType == typeof(string))
            {
                v = Db.GetString(reader, attr.FieldName);
            }
            else if (fieldType == typeof(bool))
            {
                v = Db.GetBool(reader, attr.FieldName);
            }
            else if(fieldType == typeof(Color))
            {
                v = Db.GetColor(reader, attr.FieldName);
            }
            else if (fieldType.IsEnum)
            {
                if (attr.EnumValue){	v = Db.GetInt(reader, attr.FieldName);                						}
                else               {    v = System.Enum.Parse(fieldType, Db.GetString(reader, attr.FieldName));     }
            }
			else if (fieldType == typeof(Vector3))
			{
				v = PETools.Db.GetVector3(reader, attr.FieldName);
			}			
			else if (fieldType == typeof(Vector3[]))
			{
				v = PETools.Db.GetVector3Array(reader, attr.FieldName);
			}
			else if (fieldType == typeof(Quaternion))
			{
				v = PETools.Db.GetQuaternion(reader, attr.FieldName);
			}
			else if (fieldType == typeof(Quaternion[]))
			{
				v = PETools.Db.GetQuaternionArray(reader, attr.FieldName);
			}
			else
			{
				Debug.LogError("not supported value type:" + fieldType);
            }
            return v;
        }
    }

    public static class Db
    {
		public static int[] ReadIntArray(string text)
		{
			if (string.IsNullOrEmpty(text) || text == "0")
			{
				return null;
			}
			
			string[] tmplist = text.Split(',');
			int[] intArray = new int[tmplist.Length];
			for (int i = 0; i < tmplist.Length; i++)
			{
				intArray[i] = Convert.ToInt32(tmplist[i]);
			}
			return intArray;
		}
		public static float[] ReadFloatArray(string text)
		{
			if (string.IsNullOrEmpty(text) || text == "0")
			{
				return null;
			}
			
			string[] tmplist = text.Split(',');
			float[] floatArray = new float[tmplist.Length];
			for (int i = 0; i < tmplist.Length; i++)
			{
				floatArray[i] = Convert.ToSingle(tmplist[i]);
			}
			return floatArray;
		}
		static Vector3[] ReadVector3Array(string text)
		{
			if (string.IsNullOrEmpty(text) || text == "0")
			{
				return null;
			}
			
			string[] tmplist = text.Split(';');
			Vector3[] vecArray = new Vector3[tmplist.Length];
			for (int i = 0; i < tmplist.Length; i++)
			{
				float[] vals = ReadFloatArray(tmplist[i]);
				if(vals != null && vals.Length >= 3){
					vecArray[i] = new Vector3(vals[0], vals[1], vals[2]);
				}
			}
			return vecArray;
		}
		static Quaternion[] ReadQuaternionArray(string text)
		{
			if (string.IsNullOrEmpty(text) || text == "0")
			{
				return null;
			}
			
			string[] tmplist = text.Split(';');
			Quaternion[] quaArray = new Quaternion[tmplist.Length];
			for (int i = 0; i < tmplist.Length; i++)
			{
				float[] vals = ReadFloatArray(tmplist[i]);
				if(vals != null && vals.Length >= 4){
					quaArray[i] = new Quaternion(vals[0], vals[1], vals[2], vals[3]);
				}
			}
			return quaArray;
		}

		static Color ReadColor(string text)
		{
			if(string.IsNullOrEmpty(text) || text == "0")
				return Color.black;
			string[] subStr = text.Split(',');
			return new Color(Convert.ToSingle(subStr[0])/255f, Convert.ToSingle(subStr[1])/255f,
			                 Convert.ToSingle(subStr[2])/255f, Convert.ToSingle(subStr[3])/255f);
		}

		public static Vector3 GetVector3(SqliteDataReader reader, string fieldName)
		{
			float[] vals = ReadFloatArray(reader.GetString(reader.GetOrdinal(fieldName)));
			if (vals != null && vals.Length >= 3) {
				return new Vector3(vals[0], vals[1], vals[2]);
			}
			return Vector3.zero;
		}
		public static Vector3[] GetVector3Array(SqliteDataReader reader, string fieldName)
		{
			return ReadVector3Array(reader.GetString(reader.GetOrdinal(fieldName)));
		}

		public static Quaternion GetQuaternion(SqliteDataReader reader, string fieldName)
		{
			float[] vals = ReadFloatArray(reader.GetString(reader.GetOrdinal(fieldName)));
			if (vals != null && vals.Length >= 4) {
				return new Quaternion(vals[0], vals[1], vals[2], vals[3]);
			}
			return Quaternion.identity;
		}
		public static Quaternion[] GetQuaternionArray(SqliteDataReader reader, string fieldName)
		{
			return ReadQuaternionArray(reader.GetString(reader.GetOrdinal(fieldName)));
		}
		
		public static int GetInt(SqliteDataReader reader, string fieldName)
        {
            return System.Convert.ToInt32(reader.GetString(reader.GetOrdinal(fieldName)));
        }
        public static int[] GetIntArray(SqliteDataReader reader, string fieldName)
        {
			return ReadIntArray(reader.GetString(reader.GetOrdinal(fieldName)));
        }

        public static float GetFloat(SqliteDataReader reader, string fieldName)
        {
            return System.Convert.ToSingle(reader.GetString(reader.GetOrdinal(fieldName)));
        }
		public static float[] GetFloatArray(SqliteDataReader reader, string fieldName)
		{
			return ReadFloatArray(reader.GetString(reader.GetOrdinal(fieldName)));
		}

        public static string GetString(SqliteDataReader reader, string fieldName)
        {
            return reader.GetString(reader.GetOrdinal(fieldName));
        }

        public static bool GetBool(SqliteDataReader reader, string fieldName)
        {
            return GetInt(reader, fieldName) == 0 ? false : true;
        }

		public static Color GetColor(SqliteDataReader reader, string fieldName)
		{
			return ReadColor(reader.GetString(reader.GetOrdinal(fieldName)));
		}

		public static object TraverseHierarchySerial(this Transform root, Func<Transform, int, object> operate, int depthLimit = -1)
		{
			if (operate != null)
			{
				object obj = operate(root, depthLimit);
				if (obj != null || depthLimit == 0) return obj;
				
				for (int i = root.childCount - 1; i >= 0; i--)
				{
					obj = TraverseHierarchySerial(root.GetChild(i), operate, depthLimit - 1);
					if (obj != null) return obj;
				}
			}
			return null;
		}

		public static void TraverseHierarchySerial(this Transform root, Action<Transform, int> operate, int depthLimit = -1)
		{
			if (operate != null)
			{
				operate(root, depthLimit);
				if (depthLimit == 0) return;
				
				for (int i = 0; i < root.childCount; ++i)
					TraverseHierarchySerial(root.GetChild(i), operate, depthLimit - 1);
			}
		}
    }
}