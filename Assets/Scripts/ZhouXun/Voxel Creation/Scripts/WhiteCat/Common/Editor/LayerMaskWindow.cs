using System;
using UnityEngine;
using UnityEditor;
using WhiteCat.ArrayExtension;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	public class LayerMaskWindow : EditorWindow
	{
		static LayerMaskWindow window;

		static Color borderColor = new Color(0.5f, 0.5f, 0.5f, 1);
		static Color hoverColor = new Color(0, 0, 0, 0.12f);

		const float interval = 5;
		const float buttonHeightScale = 1.25f;
		const float lineInterval = 2f;
		const float windowWidth = 186;

		static string[] layerNames = new string[32];
		static Vector2 size;

		static GUIStyle rightAlign = new GUIStyle(GUI.skin.label);
		static float lineHeight;
		static float buttonHeight;

		static int hover = -1;

		static int mask;
		static Action<int> onClose;


		static LayerMaskWindow()
		{
			rightAlign.alignment = TextAnchor.MiddleRight;
			rightAlign.padding = new RectOffset(0, (int)interval, 0, 0);
			lineHeight = EditorGUIUtility.singleLineHeight + lineInterval;
			buttonHeight = buttonHeightScale * EditorGUIUtility.singleLineHeight;
		}


		public static void ShowAsDropDown(Rect buttonRect, int mask, Action<int> onClose)
		{
			window = ScriptableObject.CreateInstance<LayerMaskWindow>();
			window.wantsMouseMove = true;
			LayerMaskWindow.mask = mask;
			LayerMaskWindow.onClose = onClose;
			layerNames.SetValues(null);
			hover = -1;

			int count = 0;
			string name;
			for(int i=0; i<32; i++)
			{
				name = LayerMask.LayerToName(i);
				if(!string.IsNullOrEmpty(name))
				{
					layerNames[i] = ' ' + name;
					count++;
				}
			}

			size.x = buttonRect.x;
			size.y = buttonRect.y;
			size = EditorGUIUtility.GUIToScreenPoint(size);
			buttonRect.x = size.x;
			buttonRect.y = size.y;
			size.x = windowWidth;
			size.y = buttonHeight + lineHeight * count + 4 * interval;
			window.ShowAsDropDown(buttonRect, size);
		}


		void OnGUI()
		{
			if(Event.current.type == EventType.mouseMove)
			{
				float y = Event.current.mousePosition.y - interval * 3 - buttonHeight;
				int nowHover = y < 0 ? -1 : (int)(y / lineHeight);
				if(nowHover != hover)
				{
					hover = nowHover;
					Repaint();
				}
			}
			else
			{
				Rect rect = new Rect(interval, interval, (size.x - 3 * interval) * 0.5f, buttonHeight);
				if (GUI.Button(rect, "Nothing")) mask = 0;

				rect.x += rect.width + interval;
				if (GUI.Button(rect, "Everything")) mask = ~0;

				rect.Set(0, rect.yMax + interval, size.x, 1);
				EditorGUI.DrawRect(rect, borderColor);

				rect.Set(interval, rect.y + interval, size.x - interval, lineHeight);
				int count = 0;

				for (int i = 0; i < 32; i++)
				{
					if (layerNames[i] != null)
					{
						if (hover == count)
						{
							rect.xMin = 0;
							EditorGUI.DrawRect(rect, hoverColor);
							rect.xMin = interval;
						}
						mask = mask.SetBit(i, GUI.Toggle(rect, mask.GetBit(i), layerNames[i]));
						GUI.Label(rect, i.ToString(), rightAlign);

						rect.y += lineHeight;
						count++;
					}
				}

				rect.Set(0, 0, size.x, size.y);
				EditorKit.DrawWireRect(rect, borderColor, 1);
			}
		}


		void OnDestroy()
		{
			onClose(mask);
		}
	}
}
