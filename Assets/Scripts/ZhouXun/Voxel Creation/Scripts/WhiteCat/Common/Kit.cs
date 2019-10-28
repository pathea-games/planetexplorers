using UnityEngine;

namespace WhiteCat
{
	public class Kit
	{
		public const float OneMillionth = 1e-6f;
		public const float Million = 1e6f;


		public const int sizeOfVector3 = 12;
		public const int sizeOfQuaternion = 16;
		public const int sizeOfUshort = 2;


		public static bool IsNullOrEmpty(System.Collections.ICollection c)
		{
			return c == null || c.Count == 0;
		}


		/// <summary>
		/// 将 Vector3 值写入字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 写入字节数组的开始下标, 操作完成后增加 12 </param>
		/// <param name="value"> 被写入的值 </param>
		public static void WriteToBuffer(byte[] buffer, ref int offset, Vector3 value)
		{
			UnionValue union = new UnionValue();

			union.floatValue = value.x;
			union.WriteFloatTo(buffer, ref offset);

			union.floatValue = value.y;
			union.WriteFloatTo(buffer, ref offset);

			union.floatValue = value.z;
			union.WriteFloatTo(buffer, ref offset);
		}


		/// <summary>
		/// 从字节数组里读取 Vector3
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组里开始读取的下标, 操作完成后增加 12 </param>
		/// <returns> 读取的 Vector3 值 </returns>
		public static Vector3 ReadVector3FromBuffer(byte[] buffer, ref int offset)
		{
			Vector3 value = new Vector3();
			UnionValue union = new UnionValue();

			union.ReadFloatFrom(buffer, ref offset);
			value.x = union.floatValue;

			union.ReadFloatFrom(buffer, ref offset);
			value.y = union.floatValue;

			union.ReadFloatFrom(buffer, ref offset);
			value.z = union.floatValue;

			return value;
		}


		/// <summary>
		/// 将 Quaternion 值写入字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 写入字节数组的开始下标, 操作完成后增加 16 </param>
		/// <param name="value"> 被写入的值 </param>
		public static void WriteToBuffer(byte[] buffer, ref int offset, Quaternion value)
		{
			UnionValue union = new UnionValue();

			union.floatValue = value.x;
			union.WriteFloatTo(buffer, ref offset);

			union.floatValue = value.y;
			union.WriteFloatTo(buffer, ref offset);

			union.floatValue = value.z;
			union.WriteFloatTo(buffer, ref offset);

			union.floatValue = value.w;
			union.WriteFloatTo(buffer, ref offset);
		}


		/// <summary>
		/// 从字节数组里读取 Quaternion
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组里开始读取的下标, 操作完成后增加 16 </param>
		/// <returns> 读取的 Quaternion 值 </returns>
		public static Quaternion ReadQuaternionFromBuffer(byte[] buffer, ref int offset)
		{
			Quaternion value = new Quaternion();
			UnionValue union = new UnionValue();

			union.ReadFloatFrom(buffer, ref offset);
			value.x = union.floatValue;

			union.ReadFloatFrom(buffer, ref offset);
			value.y = union.floatValue;

			union.ReadFloatFrom(buffer, ref offset);
			value.z = union.floatValue;

			union.ReadFloatFrom(buffer, ref offset);
			value.w = union.floatValue;

			return value;
		}

	}
}