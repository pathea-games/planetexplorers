using System;
using System.Text;

namespace WhiteCat.ArrayExtension
{
	/// <summary>
	/// 数组扩展
	/// </summary>
	public static class ArrayExtension
	{
		/// <summary>
		/// 设置一维数组中一个区域的元素的值
		/// </summary>
		/// <param name="index">开始设置值的下标</param>
		/// <param name="count">连续设置值的元素数，非正值表示直到数组边界</param>
		public static void SetValues<T>(this T[] array, T value, int index = 0, int count = 0)
		{
			if (count <= 0) count = array.Length;
			else count += index;

			while(index < count) array[index++] = value;
		}


		/// <summary>
		/// 设置二维数组中一个区域的元素的值
		/// </summary>
		/// <param name="beginRow">区域开始的行下标</param>
		/// <param name="beginCol">区域开始的列下标</param>
		/// <param name="endRow">区域结束的行下标（不包含），非正值表示直到数组边界</param>
		/// <param name="endCol">区域结束的列下标（不包含），非正值表示直到数组边界</param>
		public static void SetValues<T>(this T[,] array, T value, int beginRow = 0, int beginCol = 0, int endRow = 0, int endCol = 0)
		{
			if (endRow <= 0) endRow = array.GetLength(0);
			if (endCol <= 0) endCol = array.GetLength(1);

			for (int i = beginRow; i < endRow; i++)
			{
				for (int j = beginCol; j < endCol; j++)
				{
					array[i,j] = value;
				}
			}
		}


		/// <summary>
		/// 遍历任意维度的数组
		/// </summary>
		/// <param name="array">执行遍历的数组</param>
		/// <param name="onElement">遍历到每一个数组元素时执行此方法，参数 1 是当前元素从 0 开始的维度，参数 2 是此元素在每个维度的下标组成的数组</param>
		/// <param name="beginDimension">遍历每个维度开始时执行此方法，参数 1 是从 0 开始的当前维度值，参数 2 是此维度之前每个维度的下标组成的数组</param>
		/// <param name="endDimension">遍历每个维度结束时执行此方法，参数 1 是从 0 开始的当前维度值，参数 2 是此维度之前每个维度的下标组成的数组</param>
		public static void Traverse(this Array array, Action<int, int[]> onElement, Action<int, int[]> beginDimension = null, Action<int, int[]> endDimension = null)
		{
			if (array.Length != 0)
			{
				TraverseDimension(array, onElement, beginDimension, endDimension, 0, new int[array.Rank]);
			}
		}


		static void TraverseDimension(Array array, Action<int, int[]> onElement, Action<int, int[]> beginDimension, Action<int, int[]> endDimension, int dimension, int[] indices)
		{
			int size = array.GetLength(dimension);
			bool isFinal = (dimension + 1 == array.Rank);

			if (beginDimension != null) beginDimension(dimension, indices);

			for (int i = 0; i < size; i++)
			{
				indices[dimension] = i;
				if (isFinal) { if (onElement != null) onElement(dimension, indices); }
				else TraverseDimension(array, onElement, beginDimension, endDimension, dimension + 1, indices);
			}

			if (endDimension != null) endDimension(dimension, indices);
		}


		/// <summary>
		/// 获取数组内容的字符串。该字符串与 C# 的语法格式相似，数组元素中，空引用使用 “null” 表示，string 类型的两边会添加“"”，其他类型通过 ToString() 方法获得描述。
		/// </summary>
		public static string GetContentString(this Array array)
		{
			StringBuilder builder = new StringBuilder(array.Length * 4);
			array.Traverse(
				(d, i) =>
				{
					if (i[d] != 0) builder.Append(',');
					builder.Append(' ');
					object obj = array.GetValue(i);
					if(ReferenceEquals(obj, null)) builder.Append("null");
					else if(obj.GetType() == typeof(string))
					{
						builder.Append('"');
						builder.Append(obj);
						builder.Append('"');
					}
					else builder.Append(obj);
				},

				(d, i) =>
				{
					if (d != 0)
					{
						if(i[d - 1] != 0) builder.Append(',');
						builder.Append('\n');
						while(d != 0)
						{
							builder.Append('\t');
							d--;
						}
					}
					builder.Append('{');
				},

				(d, i) =>
				{
					if (d + 1 == array.Rank) builder.Append(" }");
					else
					{
						builder.Append('\n');
						while (d != 0)
						{
							builder.Append('\t');
							d--;
						}
						builder.Append('}');
					}
				});
			return builder.ToString();
		}
	}
}