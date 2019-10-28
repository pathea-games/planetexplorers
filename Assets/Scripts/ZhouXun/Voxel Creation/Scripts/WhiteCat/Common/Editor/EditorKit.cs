using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using WhiteCat.BitwiseOperationExtension;
using WhiteCat.Internal;

namespace WhiteCat
{
	// 控件事件
	public enum HandleEvent { unknown = 0, select, release }


	// 编辑器工具
	public struct EditorKit
	{
		// 创建资源，如果路径不存在会自动创建（示例参数："Assets/FolderName", "AssetName.asset"）
		public static void CreateAsset<T>(T asset, string directory, string name) where T : UnityEngine.Object
		{
			try
			{
				string fullPath = directory + '/' + name;
				Directory.CreateDirectory(directory);
				AssetDatabase.CreateAsset(asset, fullPath);
				AssetDatabase.SaveAssets();
			}
			catch (Exception e) { Debug.LogWarning(e); }
		}


		// 加载已存在的资源，或在不存在的情况下直接创建新资源（示例参数："Assets/FolderName", "AssetName.asset"）
		public static T LoadOrCreateAsset<T>(string directory, string name, Func<T> createInstance) where T : UnityEngine.Object
		{
			string fullPath = directory + '/' + name;
			T asset = AssetDatabase.LoadAssetAtPath(fullPath, typeof(T)) as T;

			if(asset == null)
			{
				asset = createInstance();
				CreateAsset(asset, directory, name);
			}

			return asset;
		}


		// 使用相对于插件根目录的路径加载资源 (示例: "Image/icon.png")
		public static T LoadAssetAtRelativePath<T>(string relativePath) where T : UnityEngine.Object
		{
			return AssetDatabase.LoadAssetAtPath(RootLocator.directory + '/' + relativePath, typeof(T)) as T;
		}


		// 在绘制可交互的 Handle 之前调用此方法开始事件检查
		public static void BeginHandleEventCheck()
		{
			_lastHotControl = GUIUtility.hotControl;
		}
		static int _lastHotControl;


		// 在绘制可交互的 Handle 之后调用此方法获得事件类型
		public static HandleEvent EndHandleEventCheck()
		{
			return _lastHotControl == GUIUtility.hotControl ? HandleEvent.unknown
				: (_lastHotControl == 0 ? HandleEvent.select : HandleEvent.release);
		}


		// 绘制矩形的边框
		public static void DrawWireRect(Rect rect, Color color, float size)
		{
			Rect draw = new Rect(rect.x, rect.y, rect.width, size);
			EditorGUI.DrawRect(draw, color);
			draw.y = rect.yMax - size;
			EditorGUI.DrawRect(draw, color);
			draw.yMax = draw.yMin;
			draw.yMin = rect.yMin + size;
			draw.width = size;
			EditorGUI.DrawRect(draw, color);
			draw.x = rect.xMax - size;
			EditorGUI.DrawRect(draw, color);
		}


		// 层掩码绘制
		public static void LayerMaskField(Rect rect, GUIContent label, int mask, Action<int> onClose)
		{
			string text = null;
			if (mask == 0) text = "Nothing";
			else if (mask == (~0)) text = "Everything";
			else
			{
				for (int i = 0; i < 32; i++)
				{
					if (mask.GetBit(i))
					{
						text = LayerMask.LayerToName(i);
						if (string.IsNullOrEmpty(text)) text = "Mixed...";
						else if (mask.SetBit0(i) != 0) text += ", ...";
						break;
					}
				}
			}

			if (GUI.Button(rect = EditorGUI.PrefixLabel(rect, label), text))
			{
				LayerMaskWindow.ShowAsDropDown(rect, mask, onClose);
			}
		}


		// 获取 Type 对应的 SerializedPropertyType
		public static SerializedPropertyType GetPropertyType(Type type)
		{
			if (type == typeof(float)) return SerializedPropertyType.Float;
			if (type == typeof(int)) return SerializedPropertyType.Integer;
			if (type == typeof(bool)) return SerializedPropertyType.Boolean;
			if (type == typeof(string)) return SerializedPropertyType.String;
			if (type == typeof(Color)) return SerializedPropertyType.Color;
			if (type == typeof(Vector3)) return SerializedPropertyType.Vector3;
			if (type == typeof(Vector2)) return SerializedPropertyType.Vector2;
			if (type == typeof(Vector4)) return SerializedPropertyType.Vector4;
			if (type == typeof(Rect)) return SerializedPropertyType.Rect;
			if (type == typeof(LayerMask)) return SerializedPropertyType.LayerMask;
			if (type == typeof(Quaternion)) return SerializedPropertyType.Quaternion;
			if (type == typeof(AnimationCurve)) return SerializedPropertyType.AnimationCurve;
			if (type == typeof(Bounds)) return SerializedPropertyType.Bounds;
			if (type == typeof(char)) return SerializedPropertyType.Character;
			if (type.IsSubclassOf(typeof(Enum))) return SerializedPropertyType.Enum;
			if (type.IsSubclassOf(typeof(UnityEngine.Object))) return SerializedPropertyType.ObjectReference;
			return SerializedPropertyType.Generic;
		}
	}
}
