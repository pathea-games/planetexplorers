using UnityEditor;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	[CustomEditor(typeof(TweenTextColor))]
	class TweenTextColorEditor : TweenEditor<TweenTextColor>
	{
		int mask;
		Color from, to;


		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			mask = target.mask;
			from = target.from;
			to = target.to;

			EditorGUI.BeginChangeCheck();
			DrawColorChannels(ref mask, ref from, ref to);
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
