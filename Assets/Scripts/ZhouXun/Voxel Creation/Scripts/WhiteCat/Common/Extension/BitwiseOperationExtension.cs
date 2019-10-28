
namespace WhiteCat.BitwiseOperationExtension
{
	/// <summary>
	/// 位操作扩展
	/// </summary>
	public static class BitwiseOperationExtension
	{
		/// <summary>
		/// 获得将指定二进制位设置为 0 后的值。
		/// </summary>
		public static sbyte SetBit0(this sbyte value, int bit) { return (sbyte)(value & (~(1 << bit))); }


		/// <summary>
		/// 获得将指定二进制位设置为 0 后的值。
		/// </summary>
		public static byte SetBit0(this byte value, int bit) { return (byte)(value & (~(1u << bit))); }


		/// <summary>
		/// 获得将指定二进制位设置为 0 后的值。
		/// </summary>
		public static short SetBit0(this short value, int bit) { return (short)(value & (~(1 << bit))); }


		/// <summary>
		/// 获得将指定二进制位设置为 0 后的值。
		/// </summary>
		public static ushort SetBit0(this ushort value, int bit) { return (ushort)(value & (~(1u << bit))); }


		/// <summary>
		/// 获得将指定二进制位设置为 0 后的值。
		/// </summary>
		public static int SetBit0(this int value, int bit) { return value & (~(1 << bit)); }


		/// <summary>
		/// 获得将指定二进制位设置为 0 后的值。
		/// </summary>
		public static uint SetBit0(this uint value, int bit) { return value & (~(1u << bit)); }


		/// <summary>
		/// 获得将指定二进制位设置为 0 后的值。
		/// </summary>
		public static long SetBit0(this long value, int bit) { return value & (~(1L << bit)); }


		/// <summary>
		/// 获得将指定二进制位设置为 0 后的值。
		/// </summary>
		public static ulong SetBit0(this ulong value, int bit) { return value & (~(1UL << bit)); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 后的值。
		/// </summary>
		public static sbyte SetBit1(this sbyte value, int bit) { return (sbyte)((byte)value | (1u << bit)); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 后的值。
		/// </summary>
		public static byte SetBit1(this byte value, int bit) { return (byte)(value | (1u << bit)); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 后的值。
		/// </summary>
		public static short SetBit1(this short value, int bit) { return (short)((ushort)value | (1u << bit)); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 后的值。
		/// </summary>
		public static ushort SetBit1(this ushort value, int bit) { return (ushort)(value | (1u << bit)); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 后的值。
		/// </summary>
		public static int SetBit1(this int value, int bit) { return value | (1 << bit); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 后的值。
		/// </summary>
		public static uint SetBit1(this uint value, int bit) { return value | (1u << bit); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 后的值。
		/// </summary>
		public static long SetBit1(this long value, int bit) { return value | (1L << bit); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 后的值。
		/// </summary>
		public static ulong SetBit1(this ulong value, int bit) { return value | (1UL << bit); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 或 0 后的值。
		/// </summary>
		public static sbyte SetBit(this sbyte value, int bit, bool is1) { return is1 ? value.SetBit1(bit) : value.SetBit0(bit); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 或 0 后的值。
		/// </summary>
		public static byte SetBit(this byte value, int bit, bool is1) { return is1 ? value.SetBit1(bit) : value.SetBit0(bit); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 或 0 后的值。
		/// </summary>
		public static short SetBit(this short value, int bit, bool is1) { return is1 ? value.SetBit1(bit) : value.SetBit0(bit); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 或 0 后的值。
		/// </summary>
		public static ushort SetBit(this ushort value, int bit, bool is1) { return is1 ? value.SetBit1(bit) : value.SetBit0(bit); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 或 0 后的值。
		/// </summary>
		public static int SetBit(this int value, int bit, bool is1) { return is1 ? value.SetBit1(bit) : value.SetBit0(bit); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 或 0 后的值。
		/// </summary>
		public static uint SetBit(this uint value, int bit, bool is1) { return is1 ? value.SetBit1(bit) : value.SetBit0(bit); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 或 0 后的值。
		/// </summary>
		public static long SetBit(this long value, int bit, bool is1) { return is1 ? value.SetBit1(bit) : value.SetBit0(bit); }


		/// <summary>
		/// 获得将指定二进制位设置为 1 或 0 后的值。
		/// </summary>
		public static ulong SetBit(this ulong value, int bit, bool is1) { return is1 ? value.SetBit1(bit) : value.SetBit0(bit); }


		/// <summary>
		/// 获得将指定二进制位反转后的值。
		/// </summary>
		public static sbyte ReverseBit(this sbyte value, int bit) { return (sbyte)(value ^ (1 << bit)); }


		/// <summary>
		/// 获得将指定二进制位反转后的值。
		/// </summary>
		public static byte ReverseBit(this byte value, int bit) { return (byte)(value ^ (1u << bit)); }


		/// <summary>
		/// 获得将指定二进制位反转后的值。
		/// </summary>
		public static short ReverseBit(this short value, int bit) { return (short)(value ^ (1 << bit)); }


		/// <summary>
		/// 获得将指定二进制位反转后的值。
		/// </summary>
		public static ushort ReverseBit(this ushort value, int bit) { return (ushort)(value ^ (1u << bit)); }


		/// <summary>
		/// 获得将指定二进制位反转后的值。
		/// </summary>
		public static int ReverseBit(this int value, int bit) { return value ^ (1 << bit); }


		/// <summary>
		/// 获得将指定二进制位反转后的值。
		/// </summary>
		public static uint ReverseBit(this uint value, int bit) { return value ^ (1u << bit); }


		/// <summary>
		/// 获得将指定二进制位反转后的值。
		/// </summary>
		public static long ReverseBit(this long value, int bit) { return value ^ (1L << bit); }


		/// <summary>
		/// 获得将指定二进制位反转后的值。
		/// </summary>
		public static ulong ReverseBit(this ulong value, int bit) { return value ^ (1UL << bit); }


		/// <summary>
		/// 获得指定二进制位的值。
		/// </summary>
		public static bool GetBit(this sbyte value, int bit) { return (value & (1 << bit)) != 0; }


		/// <summary>
		/// 获得指定二进制位的值。
		/// </summary>
		public static bool GetBit(this byte value, int bit) { return (value & (1u << bit)) != 0; }


		/// <summary>
		/// 获得指定二进制位的值。
		/// </summary>
		public static bool GetBit(this short value, int bit) { return (value & (1 << bit)) != 0; }


		/// <summary>
		/// 获得指定二进制位的值。
		/// </summary>
		public static bool GetBit(this ushort value, int bit) { return (value & (1u << bit)) != 0; }


		/// <summary>
		/// 获得指定二进制位的值。
		/// </summary>
		public static bool GetBit(this int value, int bit) { return (value & (1 << bit)) != 0; }


		/// <summary>
		/// 获得指定二进制位的值。
		/// </summary>
		public static bool GetBit(this uint value, int bit) { return (value & (1u << bit)) != 0; }


		/// <summary>
		/// 获得指定二进制位的值。
		/// </summary>
		public static bool GetBit(this long value, int bit) { return (value & (1L << bit)) != 0; }


		/// <summary>
		/// 获得指定二进制位的值。
		/// </summary>
		public static bool GetBit(this ulong value, int bit) { return (value & (1UL << bit)) != 0; }
	}
}