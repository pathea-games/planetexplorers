#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;

namespace WhiteCatEditor
{
	/// <summary>
	/// GUI 相关的编辑器方法
	/// </summary>
	public partial struct GUIKit
	{
		static float _lastRecordedLabelWidth;
		static bool _lastRecordedWideMode;
		static Color _lastRecordedContentColor;
		static Color _lastRecordedBackgroundColor;
		static Color _lastRecordedColor;
		static Color _lastRecordedHandlesColor;
		//static int _lastRecordedHotControl;
		static Matrix4x4 _lastHandlesMatrix;

		static GUIStyle _buttonStyle;
		static GUIStyle _buttonLeftStyle;
		static GUIStyle _buttonMiddleStyle;
		static GUIStyle _buttonRightStyle;

		static Vector3[] _lineVertices = new Vector3[2];
		static Vector3[] _boundsVertices = new Vector3[10];


		/// <summary>
		/// 编辑器默认内容颜色 (文本, 按钮图片等)
		/// </summary>
		public static Color defaultContentColor
		{
			get { return EditorStyles.label.normal.textColor; }
		}


		/// <summary>
		/// 编辑器默认背景颜色
		/// </summary>
		public static Color defaultBackgroundColor
		{
			get
			{
				float rgb = EditorGUIUtility.isProSkin ? 56f : 194f;
				rgb /= 255f;
				return new Color(rgb, rgb, rgb, 1f);
			}
		}


		/// <summary>
		/// 记录并设置 LabelWidth
		/// </summary>
		public static void RecordAndSetLabelWidth(float newWidth)
		{
			_lastRecordedLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = newWidth;
        }


		/// <summary>
		/// 恢复 LabelWidth
		/// </summary>
		public static void RestoreLabelWidth()
		{
			EditorGUIUtility.labelWidth = _lastRecordedLabelWidth;
		}


		/// <summary>
		/// 记录并设置 WideMode
		/// </summary>
		public static void RecordAndSetWideMode(bool newWideMode)
		{
			_lastRecordedWideMode = EditorGUIUtility.wideMode;
			EditorGUIUtility.wideMode = newWideMode;
		}


		/// <summary>
		/// 恢复 WideMode
		/// </summary>
		public static void RestoreWideMode()
		{
			EditorGUIUtility.wideMode = _lastRecordedWideMode;
		}


		/// <summary>
		/// 记录并设置 ContentColor
		/// </summary>
		public static void RecordAndSetContentColor(Color newColor)
		{
			_lastRecordedContentColor = GUI.contentColor;
			GUI.contentColor = newColor;
		}


		/// <summary>
		/// 恢复 BackgroundColor
		/// </summary>
		public static void RestoreContentColor()
		{
			GUI.contentColor = _lastRecordedContentColor;
		}


		/// <summary>
		/// 记录并设置 BackgroundColor
		/// </summary>
		public static void RecordAndSetBackgroundColor(Color newColor)
		{
			_lastRecordedBackgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = newColor;
        }


		/// <summary>
		/// 恢复 BackgroundColor
		/// </summary>
		public static void RestoreBackgroundColor()
		{
			GUI.backgroundColor = _lastRecordedBackgroundColor;
        }


		/// <summary>
		/// 记录并设置 Color
		/// </summary>
		public static void RecordAndSetColor(Color newColor)
		{
			_lastRecordedColor = GUI.color;
			GUI.color = newColor;
		}


		/// <summary>
		/// 恢复 Color
		/// </summary>
		public static void RestoreColor()
		{
			GUI.color = _lastRecordedColor;
		}


		/// <summary>
		/// 记录并设置 Handles.color
		/// </summary>
		public static void RecordAndSetHandlesColor(Color newColor)
		{
			_lastRecordedHandlesColor = Handles.color;
			Handles.color = newColor;
		}


		/// <summary>
		/// 恢复 Handles.color
		/// </summary>
		public static void RestoreHandlesColor()
		{
			Handles.color = _lastRecordedHandlesColor;
		}


		/// <summary>
		/// 记录并设置 Handles.matrix
		/// </summary>
		public static void RecordAndSetHandlesMatrix(ref Matrix4x4 newMatrix)
		{
			_lastHandlesMatrix = Handles.matrix;
			Handles.matrix = newMatrix;
		}


		/// <summary>
		/// 记录并设置 Handles.matrix
		/// </summary>
		public static void RecordAndSetHandlesMatrix(Matrix4x4 newMatrix)
		{
			_lastHandlesMatrix = Handles.matrix;
			Handles.matrix = newMatrix;
		}


		/// <summary>
		/// 恢复 Handles.matrix
		/// </summary>
		public static void RestoreHandlesMatrix()
		{
			Handles.matrix = _lastHandlesMatrix;
		}


		/// <summary>
		/// 在绘制控件之前调用, 用以检查控件是否被鼠标选中
		/// </summary>
		public static void BeginHotControlChangeCheck()
		{
			//_lastRecordedHotControl = GUIUtility.hotControl;
        }


		/// <summary>
		/// 按钮 GUIStyle
		/// </summary>
		public static GUIStyle buttonStyle
		{
			get
			{
				if (_buttonStyle == null) _buttonStyle = "Button";
				return _buttonStyle;
            }
		}


		/// <summary>
		/// 左侧按钮 GUIStyle
		/// </summary>
		public static GUIStyle buttonLeftStyle
		{
			get
			{
				if (_buttonLeftStyle == null) _buttonLeftStyle = "ButtonLeft";
				return _buttonLeftStyle;
			}
		}


		/// <summary>
		/// 中部按钮 GUIStyle
		/// </summary>
		public static GUIStyle buttonMiddleStyle
		{
			get
			{
				if (_buttonMiddleStyle == null) _buttonMiddleStyle = "ButtonMid";
				return _buttonMiddleStyle;
			}
		}


		/// <summary>
		/// 右侧按钮 GUIStyle
		/// </summary>
		public static GUIStyle buttonRightStyle
		{
			get
			{
				if (_buttonRightStyle == null) _buttonRightStyle = "ButtonRight";
				return _buttonRightStyle;
			}
		}


		/// <summary>
		/// 绘制矩形的边框
		/// </summary>
		/// <param name="rect"> 矩形 </param>
		/// <param name="color"> 边框颜色 </param>
		/// <param name="size"> 边框大小 </param>
		public static void DrawWireRect(Rect rect, Color color, float borderSize = 1f)
		{
			Rect draw = new Rect(rect.x, rect.y, rect.width, borderSize);
			EditorGUI.DrawRect(draw, color);
			draw.y = rect.yMax - borderSize;
			EditorGUI.DrawRect(draw, color);
			draw.yMax = draw.yMin;
			draw.yMin = rect.yMin + borderSize;
			draw.width = borderSize;
			EditorGUI.DrawRect(draw, color);
			draw.x = rect.xMax - borderSize;
			EditorGUI.DrawRect(draw, color);
		}


		/// <summary>
		/// 绘制抗锯齿线段
		/// </summary>
		public static void HandlesDrawAALine(Vector3 point1, Vector3 point2)
		{
			_lineVertices[0] = point1;
			_lineVertices[1] = point2;
			Handles.DrawAAPolyLine(_lineVertices);
		}


		/// <summary>
		/// 绘制抗锯齿线段
		/// </summary>
		public static void HandlesDrawAALine(Vector3 point1, Vector3 point2, float width)
		{
			_lineVertices[0] = point1;
			_lineVertices[1] = point2;
			Handles.DrawAAPolyLine(width, _lineVertices);
		}


		/// <summary>
		/// 绘制本地边界框的线框
		/// </summary>
		public static void HandlesDrawWireLocalBounds(Bounds bounds)
		{
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;

			_boundsVertices[0] = min;
			_boundsVertices[1] = new Vector3(min.x, min.y, max.z);
			_boundsVertices[2] = new Vector3(max.x, min.y, max.z);
			_boundsVertices[3] = new Vector3(max.x, min.y, min.z);
			_boundsVertices[4] = min;

			_boundsVertices[5] = new Vector3(min.x, max.y, min.z);
			_boundsVertices[6] = new Vector3(min.x, max.y, max.z);
			_boundsVertices[7] = max;
			_boundsVertices[8] = new Vector3(max.x, max.y, min.z);
			_boundsVertices[9] = new Vector3(min.x, max.y, min.z);

			Handles.DrawAAPolyLine(_boundsVertices);

			HandlesDrawAALine(_boundsVertices[1], _boundsVertices[6]);
			HandlesDrawAALine(_boundsVertices[2], _boundsVertices[7]);
			HandlesDrawAALine(_boundsVertices[3], _boundsVertices[8]);
		}


		/// <summary>
		/// 绘制可调节的进度条控件
		/// </summary>
		/// <param name="rect"> 绘制的矩形范围 </param>
		/// <param name="value"> [0, 1] 范围的进度 </param>
		/// <param name="backgroundColor"> 背景色 </param>
		/// <param name="foregroundColor"> 进度填充色 </param>
		/// <returns> 用户修改后的进度 </returns>
		public static float ProgressBar(
			Rect rect,
			float value,
			Color backgroundColor,
			Color foregroundColor)
		{
			int controlID = GUIUtility.GetControlID(FocusType.Passive);

			switch (Event.current.GetTypeForControl(controlID))
			{
				case EventType.Repaint:
					{
						Color oldColor = GUI.color;

						GUI.color = backgroundColor;
						GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);

						rect.width = Mathf.Round(rect.width * value);
						GUI.color = foregroundColor;
						GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);

						GUI.color = oldColor;
						break;
					}

				case EventType.MouseDown:
					{
						if (Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
						{
							GUIUtility.hotControl = controlID;
						}
						break;
					}

				case EventType.MouseUp:
					{
						if (GUIUtility.hotControl == controlID)
						{
							GUIUtility.hotControl = 0;
						}
						break;
					}
			}

			if (Event.current.isMouse && GUIUtility.hotControl == controlID)
			{
				float offset = Event.current.mousePosition.x - rect.x + 1f;
				value = Mathf.Clamp01(offset / rect.width);

				GUI.changed = true;
				Event.current.Use();
			}

			return value;
		}


		/// <summary>
		/// 绘制可调节的进度条控件
		/// </summary>
		/// <param name="rect"> 绘制的矩形范围 </param>
		/// <param name="value"> [0, 1] 范围的进度 </param>
		/// <param name="backgroundColor"> 背景色 </param>
		/// <param name="foregroundColor"> 进度填充色 </param>
		/// <param name="borderColor"> 绘制的边界框颜色 </param>
		/// <param name="drawForegroundBorder"> 是否绘制进度条右侧的边界线 </param>
		/// <returns> 用户修改后的进度 </returns>
		public static float ProgressBar(
			Rect rect,
			float value,
			Color backgroundColor,
			Color foregroundColor,
			Color borderColor,
			bool drawForegroundBorder = false)
		{
			float result = ProgressBar(rect, value, backgroundColor, foregroundColor);

			if (Event.current.type == EventType.Repaint)
			{
				DrawWireRect(rect, borderColor);

				if (drawForegroundBorder)
				{
					rect.width = Mathf.Round(rect.width * value);
					if (rect.width > 0f)
					{
						rect.xMin = rect.xMax - 1f;
						EditorGUI.DrawRect(rect, borderColor);
					}
				}
			}

			return result;
		}


	} // struct EditorKit

} // namespace WhiteCatEditor

#endif // UNITY_EDITOR