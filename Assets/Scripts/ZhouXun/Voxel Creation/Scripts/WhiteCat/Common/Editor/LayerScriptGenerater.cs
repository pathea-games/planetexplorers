#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WhiteCatEditor
{
	class LayerScriptGenerater
	{
		[MenuItem("Assets/Create/Layer Script")]
		public static void CreateScript()
		{
			using (var stream = new FileStream(activeDirectory + "/Layer.cs", FileMode.Create, FileAccess.Write))
			{
				using (var writer = new StreamWriter(stream))
				{
					writer.Write(
@"
namespace Pathea
{
	/// <summary> Constants of Layers. </summary>
	public struct Layer
	{"					);

					List<string> list = new List<string>(32);

					for (int i = 0; i < 32; i++)
					{
						var name = LayerMask.LayerToName(i);

						writer.Write(string.Format(
		@"
		/// <summary> Layer {0} / Original Layer Name: {1} </summary>
		public const int {2} = {0};
"							, i, string.IsNullOrEmpty(name) ? "(none)" : name, LayerNameToVariableName(name, i, list)));
					}

					writer.Write(
		@"

		/// <summary> Constants of LayerMasks. </summary>
		public struct Mask
		{"				);

					for (int i = 0; i < 32; i++)
					{
						var name = LayerMask.LayerToName(i);

						writer.Write(string.Format(
			@"
			/// <summary> LayerMask of Layer {0} / Original Layer Name: {1} </summary>
			public const int {2} = {3};
"							, i, string.IsNullOrEmpty(name) ? "(none)" : name, list[i], 1 << i));
					}

					writer.Write(
@"		}
	}
}"						);
				}
			}
		
			AssetDatabase.Refresh();
		}


		// 获取激活的目录, 如果没有文件夹或文件被选中, 返回 Assets 目录
		static string activeDirectory
		{
			get
			{
				var objects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

				if (objects != null && objects.Length != 0)
				{
					string path;

					// 优先选择文件夹
					foreach (var obj in objects)
					{
						path = AssetDatabase.GetAssetPath(obj);
						if (Directory.Exists(path)) return path;
					}

					path = AssetDatabase.GetAssetPath(objects[0]);
					return path.Substring(0, path.LastIndexOf('/'));
				}

				return "Assets";
			}
		}


		// 判断一个字符是否为阿拉伯数字字符
		static bool IsDigit(char c)
		{
			return c >= '0' && c <= '9';
		}


		// 判断一个字符是否为英文小写字母
		static bool IsLower(char c)
		{
			return c >= 'a' && c <= 'z';
		}


		// 判断一个字符是否为英文大写字母
		static bool IsUpper(char c)
		{
			return c >= 'A' && c <= 'Z';
		}


		// 判断一个字符是否为英文字母
		static bool IsLowerOrUpper(char c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
		}


		// 层名转化为变量名
		// 仅支持下划线，英文字母 和 数字，其他字符被忽略
		// 如果无法获取有效的变量名，则使用 “Layer0” 的格式作为变量名
		static string LayerNameToVariableName(string name, int layer, List<string> list)
		{
			var chars = new List<char>(name);
			bool invalid;
			bool upper = true;

			for (int i = 0; i < chars.Count; i++)
			{
				var c = chars[i];

				if (i==0) invalid = (c != '_' && !IsLowerOrUpper(c));
				else invalid = (c != '_' && !IsDigit(c) && !IsLowerOrUpper(c));

				if (invalid)
				{
					chars.RemoveAt(i--);
					upper = true;
				}
				else if (upper)
				{
					if (IsLower(c))
					{
						chars[i] = (char)(c + 'A' - 'a');
					}
					upper = false;
				}
			}

			name = new string(chars.ToArray());
			if (name.Length == 0 || list.Contains(name))
			{
				name = string.Format("Layer{0}", layer);
			}

			list.Add(name);
			return name;
		}
	}
}

#endif