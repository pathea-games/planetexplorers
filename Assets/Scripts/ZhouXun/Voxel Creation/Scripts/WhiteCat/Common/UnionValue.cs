using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace WhiteCat
{
	/// <summary>
	/// 联合体值类型. 大小为 8 字节
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct UnionValue
	{
		#region Values

		[FieldOffset(0)] [NonSerialized] public bool boolValue;
		[FieldOffset(0)] [NonSerialized] public sbyte sbyteValue;
		[FieldOffset(0)] [NonSerialized] public byte byteValue;
		[FieldOffset(0)] [NonSerialized] public char charValue;
		[FieldOffset(0)] [NonSerialized] public short shortValue;
		[FieldOffset(0)] [NonSerialized] public ushort ushortValue;
		[FieldOffset(0)] [NonSerialized] public int intValue;
		[FieldOffset(0)] [NonSerialized] public uint uintValue;
		[FieldOffset(0)] [NonSerialized] public long longValue;
		[FieldOffset(0)] [NonSerialized] public ulong ulongValue;
		[FieldOffset(0)] [NonSerialized] public float floatValue;
		[FieldOffset(0)] [NonSerialized] public double doubleValue;

		[FieldOffset(0)] [NonSerialized] public byte byte0;
		[FieldOffset(1)] [NonSerialized] public byte byte1;
		[FieldOffset(2)] [NonSerialized] public byte byte2;
		[FieldOffset(3)] [NonSerialized] public byte byte3;
		[FieldOffset(4)] [NonSerialized] public byte byte4;
		[FieldOffset(5)] [NonSerialized] public byte byte5;
		[FieldOffset(6)] [NonSerialized] public byte byte6;
		[FieldOffset(7)] [NonSerialized] public byte byte7;

		[FieldOffset(0)] [SerializeField] long _data;

		
		/// <summary>
		/// 使用索引访问字节数据
		/// </summary>
		/// <param name="index"> 0~7 的索引值 </param>
		public byte this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return byte0;
					case 1: return byte1;
					case 2: return byte2;
					case 3: return byte3;
					case 4: return byte4;
					case 5: return byte5;
					case 6: return byte6;
					case 7: return byte7;
					default: throw new IndexOutOfRangeException();
				}
			}
			set
			{
                switch (index)
				{
					case 0: byte0 = value; break;
					case 1: byte1 = value; break;
					case 2: byte2 = value; break;
					case 3: byte3 = value; break;
					case 4: byte4 = value; break;
					case 5: byte5 = value; break;
					case 6: byte6 = value; break;
					case 7: byte7 = value; break;
					default: throw new IndexOutOfRangeException();
				}
			}
		}

		#endregion

		#region Construct & Implicit Operator

		public UnionValue(bool value) : this() { boolValue = value; }
		public UnionValue(sbyte value) : this() { sbyteValue = value; }
		public UnionValue(byte value) : this() { byteValue = value; }
		public UnionValue(char value) : this() { charValue = value; }
		public UnionValue(short value) : this() { shortValue = value; }
		public UnionValue(ushort value) : this() { ushortValue = value; }
		public UnionValue(int value) : this() { intValue = value; }
		public UnionValue(uint value) : this() { uintValue = value; }
		public UnionValue(long value) : this() { longValue = value; }
		public UnionValue(ulong value) : this() { ulongValue = value; }
		public UnionValue(float value) : this() { floatValue = value; }
		public UnionValue(double value) : this() { doubleValue = value; }

		public static implicit operator bool(UnionValue value) { return value.boolValue; }
		public static implicit operator UnionValue(bool value) { return new UnionValue(value); }
		public static implicit operator sbyte (UnionValue value) { return value.sbyteValue; }
		public static implicit operator UnionValue(sbyte value) { return new UnionValue(value); }
		public static implicit operator byte (UnionValue value) { return value.byteValue; }
		public static implicit operator UnionValue(byte value) { return new UnionValue(value); }
		public static implicit operator char (UnionValue value) { return value.charValue; }
		public static implicit operator UnionValue(char value) { return new UnionValue(value); }
		public static implicit operator short (UnionValue value) { return value.shortValue; }
		public static implicit operator UnionValue(short value) { return new UnionValue(value); }
		public static implicit operator ushort (UnionValue value) { return value.ushortValue; }
		public static implicit operator UnionValue(ushort value) { return new UnionValue(value); }
		public static implicit operator int (UnionValue value) { return value.intValue; }
		public static implicit operator UnionValue(int value) { return new UnionValue(value); }
		public static implicit operator uint (UnionValue value) { return value.uintValue; }
		public static implicit operator UnionValue(uint value) { return new UnionValue(value); }
		public static implicit operator long (UnionValue value) { return value.longValue; }
		public static implicit operator UnionValue(long value) { return new UnionValue(value); }
		public static implicit operator ulong (UnionValue value) { return value.ulongValue; }
		public static implicit operator UnionValue(ulong value) { return new UnionValue(value); }
		public static implicit operator float (UnionValue value) { return value.floatValue; }
		public static implicit operator UnionValue(float value) { return new UnionValue(value); }
		public static implicit operator double (UnionValue value) { return value.doubleValue; }
		public static implicit operator UnionValue(double value) { return new UnionValue(value); }

		#endregion

		#region ReadFrom & WriteTo

		/// <summary>
		/// 从字节数组复制数据
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组复制数据的开始下标, 操作完成后增加 count </param>
		/// <param name="count"> 从字节数组复制数据的总字节数 </param>
		public void ReadFrom(byte[] buffer, ref int offset, int count)
		{
			int index = 0;
			while (index < count)
			{
				this[index++] = buffer[offset++];
            }
        }


		/// <summary>
		/// 将数据复制到字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 复制数据到字节数组的开始下标, 操作完成后增加 count </param>
		/// <param name="count"> 复制数据到字节数组的总字节数 </param>
		public void WriteTo(byte[] buffer, ref int offset, int count)
		{
			int index = 0;
			while (index < count)
			{
				buffer[offset++] = this[index++];
			}
		}


		/// <summary>
		/// 从字节数组复制一个 bool 值
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组复制数据的开始下标, 操作完成后增加 1 </param>
		public void ReadBoolFrom(byte[] buffer, ref int offset)
		{
			byte0 = buffer[offset++];
        }


		/// <summary>
		/// 将 bool 值复制到字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 复制数据到字节数组的开始下标, 操作完成后增加 1 </param>
		public void WriteBoolTo(byte[] buffer, ref int offset)
		{
			buffer[offset++] = byte0;
		}


		/// <summary>
		/// 从字节数组复制一个 sbyte 值
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组复制数据的开始下标, 操作完成后增加 1 </param>
		public void ReadSByteFrom(byte[] buffer, ref int offset)
		{
			byte0 = buffer[offset++];
		}


		/// <summary>
		/// 将 sbyte 值复制到字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 复制数据到字节数组的开始下标, 操作完成后增加 1 </param>
		public void WriteSByteTo(byte[] buffer, ref int offset)
		{
			buffer[offset++] = byte0;
		}


		/// <summary>
		/// 从字节数组复制一个 byte 值
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组复制数据的开始下标, 操作完成后增加 1 </param>
		public void ReadByteFrom(byte[] buffer, ref int offset)
		{
			byte0 = buffer[offset++];
		}


		/// <summary>
		/// 将 byte 值复制到字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 复制数据到字节数组的开始下标, 操作完成后增加 1 </param>
		public void WriteByteTo(byte[] buffer, ref int offset)
		{
			buffer[offset++] = byte0;
		}


		/// <summary>
		/// 从字节数组复制一个 char 值
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组复制数据的开始下标, 操作完成后增加 2 </param>
		public void ReadCharFrom(byte[] buffer, ref int offset)
		{
			byte0 = buffer[offset++];
			byte1 = buffer[offset++];
		}


		/// <summary>
		/// 将 char 值复制到字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 复制数据到字节数组的开始下标, 操作完成后增加 2 </param>
		public void WriteCharTo(byte[] buffer, ref int offset)
		{
			buffer[offset++] = byte0;
			buffer[offset++] = byte1;
		}


		/// <summary>
		/// 从字节数组复制一个 short 值
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组复制数据的开始下标, 操作完成后增加 2 </param>
		public void ReadShortFrom(byte[] buffer, ref int offset)
		{
			byte0 = buffer[offset++];
			byte1 = buffer[offset++];
		}


		/// <summary>
		/// 将 short 值复制到字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 复制数据到字节数组的开始下标, 操作完成后增加 2 </param>
		public void WriteShortTo(byte[] buffer, ref int offset)
		{
			buffer[offset++] = byte0;
			buffer[offset++] = byte1;
		}


		/// <summary>
		/// 从字节数组复制一个 ushort 值
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组复制数据的开始下标, 操作完成后增加 2 </param>
		public void ReadUShortFrom(byte[] buffer, ref int offset)
		{
			byte0 = buffer[offset++];
			byte1 = buffer[offset++];
		}


		/// <summary>
		/// 将 ushort 值复制到字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 复制数据到字节数组的开始下标, 操作完成后增加 2 </param>
		public void WriteUShortTo(byte[] buffer, ref int offset)
		{
			buffer[offset++] = byte0;
			buffer[offset++] = byte1;
		}


		/// <summary>
		/// 从字节数组复制一个 int 值
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组复制数据的开始下标, 操作完成后增加 4 </param>
		public void ReadIntFrom(byte[] buffer, ref int offset)
		{
			byte0 = buffer[offset++];
			byte1 = buffer[offset++];
			byte2 = buffer[offset++];
			byte3 = buffer[offset++];
		}


		/// <summary>
		/// 将 int 值复制到字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 复制数据到字节数组的开始下标, 操作完成后增加 4 </param>
		public void WriteIntTo(byte[] buffer, ref int offset)
		{
			buffer[offset++] = byte0;
			buffer[offset++] = byte1;
			buffer[offset++] = byte2;
			buffer[offset++] = byte3;
		}


		/// <summary>
		/// 从字节数组复制一个 uint 值
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组复制数据的开始下标, 操作完成后增加 4 </param>
		public void ReadUIntFrom(byte[] buffer, ref int offset)
		{
			byte0 = buffer[offset++];
			byte1 = buffer[offset++];
			byte2 = buffer[offset++];
			byte3 = buffer[offset++];
		}


		/// <summary>
		/// 将 uint 值复制到字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 复制数据到字节数组的开始下标, 操作完成后增加 4 </param>
		public void WriteUIntTo(byte[] buffer, ref int offset)
		{
			buffer[offset++] = byte0;
			buffer[offset++] = byte1;
			buffer[offset++] = byte2;
			buffer[offset++] = byte3;
		}


		/// <summary>
		/// 从字节数组复制一个 long 值
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组复制数据的开始下标, 操作完成后增加 8 </param>
		public void ReadLongFrom(byte[] buffer, ref int offset)
		{
			int index = 0;
			while (index < 8)
			{
				this[index++] = buffer[offset++];
			}
		}


		/// <summary>
		/// 将 long 值复制到字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 复制数据到字节数组的开始下标, 操作完成后增加 8 </param>
		public void WriteLongTo(byte[] buffer, ref int offset)
		{
			int index = 0;
			while (index < 8)
			{
				buffer[offset++] = this[index++];
			}
		}


		/// <summary>
		/// 从字节数组复制一个 ulong 值
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组复制数据的开始下标, 操作完成后增加 8 </param>
		public void ReadULongFrom(byte[] buffer, ref int offset)
		{
			int index = 0;
			while (index < 8)
			{
				this[index++] = buffer[offset++];
			}
		}


		/// <summary>
		/// 将 ulong 值复制到字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 复制数据到字节数组的开始下标, 操作完成后增加 8 </param>
		public void WriteULongTo(byte[] buffer, ref int offset)
		{
			int index = 0;
			while (index < 8)
			{
				buffer[offset++] = this[index++];
			}
		}


		/// <summary>
		/// 从字节数组复制一个 float 值
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组复制数据的开始下标, 操作完成后增加 4 </param>
		public void ReadFloatFrom(byte[] buffer, ref int offset)
		{
			byte0 = buffer[offset++];
			byte1 = buffer[offset++];
			byte2 = buffer[offset++];
			byte3 = buffer[offset++];
		}


		/// <summary>
		/// 将 float 值复制到字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 复制数据到字节数组的开始下标, 操作完成后增加 4 </param>
		public void WriteFloatTo(byte[] buffer, ref int offset)
		{
			buffer[offset++] = byte0;
			buffer[offset++] = byte1;
			buffer[offset++] = byte2;
			buffer[offset++] = byte3;
		}


		/// <summary>
		/// 从字节数组复制一个 double 值
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 从字节数组复制数据的开始下标, 操作完成后增加 8 </param>
		public void ReadDoubleFrom(byte[] buffer, ref int offset)
		{
			int index = 0;
			while (index < 8)
			{
				this[index++] = buffer[offset++];
			}
		}


		/// <summary>
		/// 将 double 值复制到字节数组
		/// </summary>
		/// <param name="buffer"> 字节数组 </param>
		/// <param name="offset"> 复制数据到字节数组的开始下标, 操作完成后增加 8 </param>
		public void WriteDoubleTo(byte[] buffer, ref int offset)
		{
			int index = 0;
			while (index < 8)
			{
				buffer[offset++] = this[index++];
			}
		}


		/// <summary>
		/// 从流读取数据
		/// </summary>
		/// <param name="stream"> 数据流 </param>
		/// <param name="count"> 从流读取数据的总字节数 </param>
		public void ReadFrom(Stream stream, int count)
		{
			int read;
			for (int index = 0; index < count; index++)
			{
				if ((read = stream.ReadByte()) == -1)
				{
					throw new EndOfStreamException();
				}
				this[index] = (byte)read;
            }
		}


		/// <summary>
		/// 写入数据到流
		/// </summary>
		/// <param name="stream"> 数据流 </param>
		/// <param name="count"> 写入数据到流的总字节数 </param>
		public void WriteTo(Stream stream, int count)
		{
			for (int index = 0; index < count; index++)
			{
				stream.WriteByte(this[index]);
			}
		}

		#endregion

	} // struct UnionValue

} // namespace WhiteCat