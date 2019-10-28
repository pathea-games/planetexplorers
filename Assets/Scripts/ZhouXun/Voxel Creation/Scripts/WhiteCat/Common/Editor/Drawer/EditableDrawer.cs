using UnityEngine;
using UnityEditor;

namespace WhiteCat
{
	[CustomPropertyDrawer(typeof(EditableAttribute), true)]
	public class EditableDrawer : AttributePropertyDrawer<EditableAttribute>
	{
		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginDisabledGroup(Application.isPlaying ? !attribute.playMode : !attribute.editMode);
			base.OnGUI(rect, property, label);
			EditorGUI.EndDisabledGroup();
		}
	}
}
