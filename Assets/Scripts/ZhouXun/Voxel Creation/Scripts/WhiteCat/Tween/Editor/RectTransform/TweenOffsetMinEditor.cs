using UnityEditor;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	[CustomEditor(typeof(TweenOffsetMin))]
	class TweenOffsetMinEditor : TweenEditor<TweenOffsetMin>
	{
		int mask;
		Vector2 from, to;


		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			mask = target.mask;
			from = target.from;
			to = target.to;

			EditorGUI.BeginChangeCheck();
			DrawVector2Channels(ref mask, ref from, ref to);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, undoString);
				target.mask = mask;
				target.from = from;
				target.to = to;
				EditorUtility.SetDirty(target);
			}
		}
	}
}
