using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RedGrass
{

	public static class EditorTools 
	{

		static public GUIStyle _LineStyle;
		static public GUIStyle LineStyle
		{
			get
			{
				if(_LineStyle == null)
				{
					_LineStyle = new GUIStyle();
					_LineStyle.normal.background = EditorGUIUtility.whiteTexture;
					_LineStyle.stretchWidth = true;
				}
				
				return _LineStyle;
			}
		}

		static private GUIStyle _HeaderLabel;
		static private GUIStyle _HeaderLabelPro;
		static private GUIStyle HeaderLabel
		{
			get
			{
				if(_HeaderLabel == null)
				{
					_HeaderLabel = new GUIStyle(EditorStyles.label);
					_HeaderLabel.fontStyle = FontStyle.Bold;
					_HeaderLabel.normal.textColor = new Color(0.25f,0.25f,0.25f);
					
					_HeaderLabelPro = new GUIStyle(_HeaderLabel);
					_HeaderLabelPro.normal.textColor = new Color(0.7f,0.7f,0.7f);
				}
				return EditorGUIUtility.isProSkin ? _HeaderLabelPro : _HeaderLabel;
			}
		}

		static public void GUILine(Color color, float height = 2f)
		{
			Rect position = GUILayoutUtility.GetRect(0f, float.MaxValue, height, height, LineStyle);
			
			if(Event.current.type == EventType.Repaint)
			{
				Color orgColor = GUI.color;
				GUI.color = orgColor * color;
				LineStyle.Draw(position, false, false, false, false);
				GUI.color = orgColor;
			}
		}

		static public void Header(string header, string tooltip = null, bool expandWidth = false)
		{
			if(tooltip != null)
				EditorGUILayout.LabelField(new GUIContent(header, tooltip), HeaderLabel, GUILayout.ExpandWidth(expandWidth));
			else
				EditorGUILayout.LabelField(header, HeaderLabel, GUILayout.ExpandWidth(expandWidth));
		}


		static public void Separator()
		{
			GUILayout.Space(4);
			GUILine(new Color(.3f,.3f,.3f), 1);
			GUILine(new Color(.9f,.9f,.9f), 1);
			GUILayout.Space(4);
		}
	}
}
